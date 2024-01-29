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

    [HttpGet("list")]
    public async Task<IActionResult> List()
    {
        PredictionViewModel viewModel = new()
        {
            Predictions = await _birdsRepository.GetPredictions()
        };
        
        return View(viewModel);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeletePhoto(Guid id)
    {
        return RedirectToAction("List");
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
    public async Task<IActionResult> Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}