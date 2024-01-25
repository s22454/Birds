using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ML;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private string _modelPath = "ML/BirdModel24";
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
        
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        var imageUrl = Path.Combine("uploads", uniqueFileName);
        return Ok(imageUrl);
        
        return Ok("Plik został przesłany i zapisany.");
    }
}