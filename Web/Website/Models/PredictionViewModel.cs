using Birds.EntityFramework.Entities;

namespace WebApplication1.Models;

public class PredictionViewModel
{
    public IList<Prediction> Predictions { get; set; }
}