using Microsoft.ML;

namespace Birds;

public class Program
{
    // Paths and pre-initialized variables
    private static string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
    //private static string workspaceRelativePath = Path.Combine(projectDirectory, "Workspace");
    private static string assetsRelativePath = Path.Combine(projectDirectory, "Assets");
    private static string modelsRelativePath = Path.Combine(projectDirectory, "Models");
    
    // Manual testing 
    private static string testingRelativePath = Path.Combine(projectDirectory, "Test");
    
    public static void Main(string[] args)
    {
        // MENU 
        string? input = "start-1";
        Model? model = null;

        while (input is not null && !input.Equals("exit-0"))
        {
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine(model is null ? "MODEL NOT LOADED" : "MODEL LOADED");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("CHOOSE ACTION");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("1 - Train model");
            Console.WriteLine("2 - Save model");
            Console.WriteLine("3 - Load model");
            Console.WriteLine("4 - Test model on test set");
            Console.WriteLine("5 - Test model on image of choice");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("-----------------------------------------------------");
            Console.Write("Input: ");
            input = Console.ReadLine();

            // Handle action 
            switch (input)
            {
                
                // Train model
                case "1":
                    model = Model.GetInstance();
                    model.TrainModel(assetsRelativePath);
                    break;
                
                // Save model
                case "2":
                    if (model is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine("Name the model");
                        Console.WriteLine("-----------------------------------------------------");
                        Console.Write("Name: ");
                        input = Console.ReadLine();
                        
                        Console.WriteLine();
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine("SAVING THE MODEL");
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine();
                        if (input != null) model.SaveModel(modelsRelativePath, input);
                    }
                    break;
                
                // Load model
                case "3":
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Select the model");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.Write("Path: ");
                    input = Console.ReadLine();
                        
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("LOADING THE MODEL");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine();
                    if (input != null) model.LoadModel(input);
                    model = Model.GetInstance();
                    break;
                
                // Test model on test set
                case "4":
                    if (model is not null)
                        model.TestModel();
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine("First you have to load or train the model!");
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine();
                    }
                    
                    break;
                
                // Test model on image of choice
                case "5":
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Choose file");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.Write("Path: ");
                    input = Console.ReadLine();
                    
                    IDataView manualTestData = model.PrepareDataFromDirectory(input);
                    ModelOutput predictionsResults = model.ClassifySingleImage(manualTestData);
                    
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Manual test");
                    Console.WriteLine("-----------------------------------------------------");
                    Model.OutputSinglePrediction(predictionsResults);
                    break;
                
                // Exit
                case "0":
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Exiting Application");
                    Console.WriteLine("-----------------------------------------------------");
                    input = "exit-0";
                    break;
                
                // Error
                default:
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("WRONG ACTION!!!");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine();
                    break;
            }
        }
    }
}