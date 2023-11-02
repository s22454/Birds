namespace Birds;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Birds;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;

public class Model
{
    // Paths and pre-initialized variables
    private static string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
    private static string workspaceRelativePath = Path.Combine(projectDirectory, "Workspace");
    private static string assetsRelativePath = Path.Combine(projectDirectory, "Assets");
    
    public static void Main(string[] args)
    {
        // Initialize MLContext
        MLContext mlContext = new MLContext();
        
        // Load image data
        IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: assetsRelativePath, useFolderNameAsLabel: true);
        IDataView imageData = mlContext.Data.LoadFromEnumerable(images);

        // Shuffle the data
        IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);
        
        // Preprocess that data 
        var preprocessingPipeline = mlContext.Transforms.Conversion
            .MapValueToKey(
                inputColumnName: "Label",
                outputColumnName: "LabelAsKey")
            .Append(mlContext.Transforms.LoadRawImageBytes(
                        outputColumnName: "Image",
                        imageFolder: assetsRelativePath,
                        inputColumnName: "ImagePath"
                        ));
        
        // Use GPU
        
        // Apply data to preprocessingPipeline 
        IDataView preProcessedData = preprocessingPipeline
            .Fit(shuffledData)
            .Transform(shuffledData);
        
        // Split dataset into training, validation and test sets
        TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: .3);
        TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

        IDataView trainSet = trainSplit.TrainSet;
        IDataView validationSet = validationTestSplit.TrainSet;
        IDataView testSet = validationTestSplit.TestSet;
        
        // ImageClassificationTrainer
        var classifierOptions = new ImageClassificationTrainer.Options()
        {
            FeatureColumnName = "Image",
            LabelColumnName = "LabelAsKey",
            ValidationSet = validationSet,
            Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
            MetricsCallback = (metrics) => Console.WriteLine(metrics),
            TestOnTrainSet = false,
            ReuseTrainSetBottleneckCachedValues = true,
            ReuseValidationSetBottleneckCachedValues = true
        };
        
        // Training pipeline 
        var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
        
        // Train the model
        ITransformer trainedModel = trainingPipeline.Fit(trainSet);
        
        // Classify single image
        ClassifySingleImage(mlContext, testSet, trainedModel);
        
        // Classifying multiple images
        ClassifyImages(mlContext, testSet, trainedModel);
    }
    
    public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
    {
        // Get all files from folder 
        var files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);

        foreach (var file in files)
        {
            // If file is not an image skip it 
            if (Path.GetExtension(file) != ".jpg" && Path.GetExtension(file) != ".png")
                continue;

            // Select file label
            var label = Path.GetFileName(file);

            if (useFolderNameAsLabel)
                label = Directory.GetParent(file).Name;
            else
            {
                for (int i = 0; i < label.Length; i++)
                {
                    if (!char.IsLetter(label[i]))
                    {
                        label = label.Substring(0, i);
                        break;
                    }
                }
            }

            // Return new ImageData instantiation 
            yield return new ImageData()
            {
                ImagePath = file,
                Label = label
            };
        }
    }

    public static void OutputPrediction(ModelOutput prediction)
    {
        string imageName = Path.GetFileName(prediction.ImagePath);
        Console.WriteLine($"Image: {imageName} | Actual Value: {prediction.Label} | Predicted Value: {prediction.PredictedLabel}");
    }

    public static void ClassifySingleImage(MLContext mlContext, IDataView data, ITransformer trainedModel)
    {
        // Create prediction engine 
        PredictionEngine<ModelInput, ModelOutput> predictionEngine =
            mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

        // Convert IDataView into IEnumerable
        ModelInput image = mlContext.Data.CreateEnumerable<ModelInput>(data, reuseRowObject: true).First();
        
        // Classify image
        ModelOutput prediction = predictionEngine.Predict(image);
        
        // Output prediction 
        Console.WriteLine("Classifying single image");
        OutputPrediction(prediction);
    }

    public static void ClassifyImages(MLContext mlContext, IDataView data, ITransformer trainedModel)
    {
        // IDataView containing predictions
        IDataView predictionData = trainedModel.Transform(data);
        
        // Convert data into IEnumerable
        IEnumerable<ModelOutput> predictions =
            mlContext.Data.CreateEnumerable<ModelOutput>(predictionData, reuseRowObject: true).Take(10);
        
        // Iterate and output 
        Console.WriteLine("Classifying multiple images");
        foreach (var prediction in predictions)
            OutputPrediction(prediction);
    }
}