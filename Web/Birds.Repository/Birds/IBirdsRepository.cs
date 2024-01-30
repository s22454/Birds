using Birds.EntityFramework.Entities;
using Microsoft.AspNetCore.Http;

namespace Birds.Repository.Birds;

public interface IBirdsRepository
{
    public Task<List<Prediction>> GetPredictions();
    public Task<string> PredictImage(IFormFile file);
    public Task DeletePhoto(Guid id);
    public Task<List<Bird>> GetBirds();
}