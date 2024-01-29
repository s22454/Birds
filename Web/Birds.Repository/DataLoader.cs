using Birds.EntityFramework.Contexts;

namespace Birds.Repository;

public class DataLoader
{
    public Model Model { get; set; }

    public DataLoader()
    {
    }

    public void Load(BirdsContext birdsContext)
    {
        Model = Model.GetInstance();
        EntityFramework.Entities.Model model = birdsContext.Models.FirstOrDefault(x => x.Name == "BirdModel24.model");
        using MemoryStream memoryStream = new(model.Data);
        Model.LoadModel(memoryStream);
    }
}
