using Microsoft.ML;

namespace WebApplication1.ML;

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
    
    private static IEnumerable<WebApplication1.ML.ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
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
            yield return new WebApplication1.ML.ImageData()
            {
                ImagePath = file,
                Label = label
            };
        }
    }

    public IDataView PrepareDataFromDirectory(string dataPath)
    {
        // Load image data
        IEnumerable<WebApplication1.ML.ImageData> images = LoadImagesFromDirectory(folder: dataPath, useFolderNameAsLabel: true);
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

    public DataViewSchema LoadModel(string path)
    {
        DataViewSchema modelSchema;
        _trainedModel = _mlContext.Model.Load(path, out modelSchema);
        return modelSchema;
    }
}