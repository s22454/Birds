using Birds.EntityFramework.Entities;

namespace WebApplication1.Models;

public class PredictionViewModel
{
    public IList<PredictionResult> Predictions { get; set; }
}

public class PredictionResult
{
    public string Name { get; set; }
    public string Date { get; set; }
    public string Base64 { get; set; }
}