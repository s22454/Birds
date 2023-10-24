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
        IEnumerable<ImageData> images = ModelUtilities.LoadImagesFromDirectory(folder: assetsRelativePath, useFolderNameAsLabel: true);
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
        
        // Apply data to preprocessingPipeline 
        IDataView preProcessedData = preprocessingPipeline
            .Fit(shuffledData)
            .Transform(shuffledData);
        
        // Split dataset into training, validation and test sets
        TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: .3);
        TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

        IDataView trainSet = trainSplit.TrainSet;
        IDataView validationSplit = validationTestSplit.TrainSet;
        IDataView testSet = validationTestSplit.TestSet;
    }
}