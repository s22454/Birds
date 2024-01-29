using Birds.EntityFramework.Contexts;

namespace Birds.Repository;

public class DataLoader
{
    private const string ModelName = "BirdsModel24v2.zip";
    const string ModelPath = $"ML/{ModelName}";
    
    public Model Model { get; set; }

    public DataLoader()
    {
    }

    public void Load(BirdsContext birdsContext)
    {
        EntityFramework.Entities.Model? model;
        Model = Model.GetInstance();
        
        if (!birdsContext.Models.Where(x => x.Name == ModelName).Any())
        {
            byte[] file;
            using (FileStream fileStream = new(ModelPath, FileMode.Open, FileAccess.Read))
            {
                file = new byte[fileStream.Length];
                fileStream.Read(file, 0, file.Length);
            }

            model = new()
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileName(ModelPath),
                Data = file
            };

            birdsContext.Models.Add(model);
            birdsContext.SaveChanges();
        }
        else
        {
            model = birdsContext.Models.FirstOrDefault(x => x.Name == ModelName);
        }
        
        using MemoryStream memoryStream = new(model.Data);
        Model.LoadModel(memoryStream);
    }
}
