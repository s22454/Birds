namespace Birds;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;

public class Model
{
    // Model instance
    private static Model? _modelInstance;

    // Trained model and MlContext
    private ITransformer? _trainedModel;
    private readonly MLContext _mlContext = new MLContext();

    // Data sets
    private IDataView? _trainSet;
    private IDataView? _validationSet;
    private IDataView? _testSet;

    private Model() {}
    
    public static Model GetInstance()
    {
        return _modelInstance ??= new Model();
    }
        
    public void TrainModel(string dataPath)
    {
        // Load image data
        IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: dataPath, useFolderNameAsLabel: true);
        IDataView imageData = _mlContext.Data.LoadFromEnumerable(images);
        
        // Preprocess that data 
        IDataView preProcessedData = PreProcessData(imageData, dataPath);
        
        // Split dataset into training, validation and test sets
        TrainTestData trainSplit = _mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: .3);
        TrainTestData validationTestSplit = _mlContext.Data.TrainTestSplit(trainSplit.TestSet);

        _trainSet = trainSplit.TrainSet;
        _validationSet = validationTestSplit.TrainSet;
        _testSet = validationTestSplit.TestSet;
        
        // ImageClassificationTrainer
        var classifierOptions = new ImageClassificationTrainer.Options()
        {
            FeatureColumnName = "Image",
            LabelColumnName = "LabelAsKey",
            ValidationSet = _validationSet,
            Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
            MetricsCallback = (metrics) => Console.WriteLine(metrics),
            TestOnTrainSet = false,
            ReuseTrainSetBottleneckCachedValues = true,
            ReuseValidationSetBottleneckCachedValues = true
        };
        
        // Training pipeline 
        var trainingPipeline = _mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
        
        // Train the model
        _trainedModel = trainingPipeline.Fit(_trainSet);
    }
    
    private static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
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

    public IDataView PrepareDataFromDirectory(string dataPath)
    {
        // Load image data
        IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: dataPath, useFolderNameAsLabel: true);
        IDataView imageData = _mlContext.Data.LoadFromEnumerable(images);
        
        // Preprocess that data 
        IDataView preProcessedData = PreProcessData(imageData, dataPath);
        
        // Return loaded and preprocessed images
        return preProcessedData;
    }

    private IDataView PreProcessData(IDataView imageData, string dataPath)
    {
        // Shuffle the data
        IDataView shuffledData = _mlContext.Data.ShuffleRows(imageData);
        
        // Preprocess that data 
        var preprocessingPipeline = _mlContext.Transforms.Conversion
            .MapValueToKey(
                inputColumnName: "Label",
                outputColumnName: "LabelAsKey")
            .Append(_mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: dataPath,
                inputColumnName: "ImagePath"
            ));
        
        // Apply data to preprocessingPipeline and return it
        var preProcessedData = preprocessingPipeline
            .Fit(shuffledData)
            .Transform(shuffledData);
        
        // Return data
        return preProcessedData;
    }

    public void TestModel()
    {
        // Write error if testSet wasn't set
        if (_testSet is null)
        {
            Console.WriteLine("ERROR: TestSet is null!");
            return;
        }
        
        // Make predictions
        IEnumerable<ModelOutput> predictions = ClassifyImages(_testSet);
        
        // Output predictions and stats
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------------");
        Console.WriteLine("Testing the model");
        Console.WriteLine("-----------------------------------------------------");
        OutputPredictions(predictions);
    }

    public static void OutputSinglePrediction(ModelOutput prediction)
    {
        string imageName = Path.GetFileName(prediction.ImagePath);
        Console.WriteLine($"Image: {imageName} | Actual Value: {prediction.Label} | Predicted Value: {prediction.PredictedLabel}");
    }

    public static void OutputPredictions(IEnumerable<ModelOutput> predictions)
    {
        // Initialize variables 
        int dataCount = 0;
        int goodPredictions = 0;

        // Iterate over predictions set
        foreach (ModelOutput prediction in predictions)
        {
            dataCount++;

            if (prediction.Label.Equals(prediction.PredictedLabel))
                goodPredictions++;
            
            OutputSinglePrediction(prediction);
        }
        
        // Write results to console 
        Console.WriteLine("-----------------------------------------------------");
        Console.WriteLine("Model statistics");
        Console.WriteLine("-----------------------------------------------------");
        Console.WriteLine("Data count: " + dataCount);
        Console.WriteLine("Correct predictions: " + goodPredictions);
        Console.WriteLine("Accuracy: " + goodPredictions / (float)dataCount);
    }

    public ModelOutput ClassifySingleImage(IDataView data)
    {
        // Return if model wasn't trained
        if (_trainedModel is null)
            return new ModelOutput()
            {
                ImagePath = "No model was trained!",
                Label = "No model was trained!",
                PredictedLabel = "No model was trained!"
            };
        
        // Create prediction engine 
        PredictionEngine<ModelInput, ModelOutput> predictionEngine =
            _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(_trainedModel);

        // Convert IDataView into IEnumerable
        ModelInput image = _mlContext.Data.CreateEnumerable<ModelInput>(data, reuseRowObject: true).First();
        
        // Classify image
        ModelOutput prediction = predictionEngine.Predict(image);
        
        // Return prediction 
        return prediction;
    }

    public IEnumerable<ModelOutput> ClassifyImages(IDataView data)
    {
        // Return if model wasn't trained
        if (_trainedModel is null)
        {
            ModelOutput[] error = new ModelOutput[1];
            
            error[0] = new ModelOutput()
            {
                ImagePath = "No model was trained!",
                Label = "No model was trained!",
                PredictedLabel = "No model was trained!"
            };

            return error;
        }

        
        // IDataView containing predictions
        IDataView predictionData = _trainedModel.Transform(data);
        
        // Convert data into IEnumerable
        IEnumerable<ModelOutput> predictions =
            _mlContext.Data.CreateEnumerable<ModelOutput>(predictionData, reuseRowObject: true);
        
        // Iterate and output 
        return predictions;
    }
}