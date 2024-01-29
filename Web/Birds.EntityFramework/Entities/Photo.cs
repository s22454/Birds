namespace Birds.EntityFramework.Entities;

public class Photo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[] Data { get; set; }
    public Prediction Prediction { get; set; }
    public Guid PredictionId { get; set; }
}