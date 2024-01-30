using System.Diagnostics;
using Birds.EntityFramework.Entities;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Birds.Repository.Birds;

namespace WebApplication1.Controllers;

public class BirdsController : Controller
{
    private readonly ILogger<BirdsController> _logger;
    private IBirdsRepository _birdsRepository;

    public BirdsController(ILogger<BirdsController> logger, IBirdsRepository birdsRepository)
    {
        _logger = logger;
        _birdsRepository = birdsRepository;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpGet("~/api/predictions")]
    public async Task<IActionResult> GetPredictions()
    {
        List<Prediction> predictions = await _birdsRepository.GetPredictions();
        List<PredictionResult> predictionResults = new();
        foreach (Prediction prediction in predictions)
        {
            string base64String = Convert.ToBase64String(prediction.Photo.Data);

            PredictionResult result = new()
            {
                Name = prediction.BirdName,
                Date = prediction.TimeSpent.ToString("dd.MM.yyyy HH:mm:ss"),
                Base64 = base64String
            };
            predictionResults.Add(result);
        }
        return Json(predictionResults);
    }
    
    [HttpGet("~/api/birds")]
    public async Task<IActionResult> GetBirds()
    {
        List<Bird> birds = await _birdsRepository.GetBirds();
        return Json(birds);
    }

    [HttpPost("~/api/deleteimage")]
    public async Task<IActionResult> DeletePhoto(Guid id)
    {
        await _birdsRepository.DeletePhoto(id);

        return Ok("Usunięto");
    }

    [HttpPost("~/api/checkimage")]
    public async Task<IActionResult> CheckImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Brak przesłanego pliku");
        }

        string birdName = await _birdsRepository.PredictImage(file);

        return Ok(birdName);
    }
    
    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}