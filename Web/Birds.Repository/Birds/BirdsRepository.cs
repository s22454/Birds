using Birds.EntityFramework.Contexts;
using Birds.EntityFramework.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Birds.Repository.Birds;

public class BirdsRepository : IBirdsRepository
{
    private BirdsContext _birdsContext;
    private DataLoader _dataLoader;
    private string _modelPath = "ML/BirdModel24.model";
    private Model? _model;

    public BirdsRepository(BirdsContext birdsContext, DataLoader dataLoader)
    {
        _birdsContext = birdsContext;
        _dataLoader = dataLoader;
        _model = _dataLoader.Model;
    }

    public async Task<List<Prediction>> GetPredictions()
    {
        List<Prediction> result =
            await _birdsContext.Predictions
                .Include(x => x.Photo)
                .ToListAsync();
        return result;
    }

    public async Task<string> PredictImage(IFormFile file)
    {
        Guid predictionId = Guid.NewGuid();

        ModelOutput? predictionsResults;
        byte[] fileBytes;

        await using (MemoryStream memoryStream = new())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();

            string folderPath = Path.Combine(Path.GetTempPath(), predictionId.ToString());
            DirectoryInfo directoryInfo =
                Directory.CreateDirectory(folderPath);

            string uniqueFileName = $"{predictionId.ToString()}_{file.FileName}";
            string filePath = Path.Combine(directoryInfo.ToString(), uniqueFileName);

            await using (FileStream fileStream = new(filePath, FileMode.Create))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(fileStream);
            }

            string imageUrl = Path.Combine(directoryInfo.ToString(), uniqueFileName);
            string? imgDir = Path.GetDirectoryName(imageUrl);

            IDataView manualTestData = _model.PrepareDataFromDirectory(imgDir);
            predictionsResults = _model.ClassifySingleImage(manualTestData);

            Directory.Delete(imgDir, true);
        }

        string birdName = predictionsResults
            .PredictedLabel
            .Split(".")[1]
            .Replace("_", " ");

        try
        {
            Guid model = 
                await _birdsContext.Models
                    .Where(x => x.Name == "BirdModel24.model")
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

            Prediction prediction = new()
            {
                Id = predictionId,
                BirdName = birdName,
                TimeSpent = DateTime.Now,
                Photo = new Photo()
                {
                    Id = Guid.NewGuid(),
                    PredictionId = predictionId,
                    Data = fileBytes,
                    Name = file.FileName,
                },
                ModelId = model
            };

            _birdsContext.Predictions.Add(prediction);
            await _birdsContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
        }

        return birdName;
    }

    public async Task DeletePhoto(Guid id)
    {
        Photo? photoToDelete = await _birdsContext.Photos.FirstOrDefaultAsync(x => x.Id == id);

        if (photoToDelete != null)
        {
            _birdsContext.Photos.Remove(photoToDelete);
            await _birdsContext.SaveChangesAsync();
        }
    }
}