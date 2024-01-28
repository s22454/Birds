using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using WebApplication1.ML;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private string _modelPath = "ML/BirdModel24.model";
    private Model? _model = null;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        _model = Model.GetInstance();
        _model.LoadModel(_modelPath);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }

    [HttpPost("~/api/checkimage")]
    public async Task<IActionResult> CheckImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Brak przesłanego pliku");
        }
        
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        
        // Create request id
        Guid requestId = Guid.NewGuid();
        
        // Create request sub-folder
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(uploadsFolder, requestId.ToString()));
        
        // Create unique file name
        string uniqueFileName = $"{requestId.ToString()}_{file.FileName}";
        
        // Find file path
        var filePath = Path.Combine(directoryInfo.ToString(), uniqueFileName);

        // Copy img to its unique directory
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // Get img url
        var imageUrl = Path.Combine(directoryInfo.ToString(), uniqueFileName);

        // Return error if model wasn't loaded
        if (_model is null) return Problem("Model not loaded");
        
        // Get image folder
        string? imgDir = Path.GetDirectoryName(imageUrl);
        
        // Return error if img directory wasn't found
        if (imgDir is null) return Problem("Error with loading image");
        
        // Get predictions from model
        IDataView manualTestData = _model.PrepareDataFromDirectory(imgDir);
        ModelOutput predictionsResults = _model.ClassifySingleImage(manualTestData);

        // Prepare result
        string res = predictionsResults
            .PredictedLabel
            .Split(".")[1]
            .Replace("_", " ");
        
        // Return predictions results
        return Ok(res);
    }
}