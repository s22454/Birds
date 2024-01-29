namespace Birds.EntityFramework.Entities;

public class Model
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[] Data { get; set; }
    public List<Prediction> Predictions { get; set; } = new();
}