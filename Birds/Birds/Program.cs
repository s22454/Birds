using Microsoft.ML;

namespace Birds;

public class Program
{
    // Paths and pre-initialized variables
    private static string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
    private static string workspaceRelativePath = Path.Combine(projectDirectory, "Workspace");
    private static string assetsRelativePath = Path.Combine(projectDirectory, "Assets");
    
    // Manual testing 
    private static string testingRelativePath = Path.Combine(projectDirectory, "Test");
    
    public static void Main(string[] args)
    {
        Model model = Model.GetInstance();
        
        // Train the model
        model.TrainModel(assetsRelativePath);
        
        // Test the model
        model.TestModel();
        
        // Manual testing 
        IDataView manualTestData = model.PrepareDataFromDirectory(testingRelativePath);
        ModelOutput predictionsResults = model.ClassifySingleImage(manualTestData);
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------------");
        Console.WriteLine("Manual test");
        Console.WriteLine("-----------------------------------------------------");
        Model.OutputSinglePrediction(predictionsResults);
    }
}