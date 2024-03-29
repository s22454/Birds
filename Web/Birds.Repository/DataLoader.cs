﻿using Birds.EntityFramework.Contexts;
using Birds.EntityFramework.Entities;

namespace Birds.Repository;

public class DataLoader
{
    public const string ModelName = "bigtest1.zip";
    const string ModelPath = $"ML/{ModelName}";
    
    public Model Model { get; set; }

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

        if (!birdsContext.Birds.Any())
        {
            string filePath = "ML/birds.txt";
            List<Bird> birds = new List<Bird>();
            
            using StreamReader reader = new(filePath);
            while (reader.ReadLine() is { } line)
            {
                Bird bird = new()
                {
                    Id = Guid.NewGuid(),
                    Name = line.Trim()
                };
                birds.Add(bird);
            }
            
            birdsContext.Birds.AddRange(birds);
            birdsContext.SaveChanges();
        }
        
        using MemoryStream memoryStream = new(model.Data);
        Model.LoadModel(memoryStream);
    }
}
