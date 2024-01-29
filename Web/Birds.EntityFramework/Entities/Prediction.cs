namespace Birds.EntityFramework.Entities;

public class Prediction
{
    public Guid Id { get; set; }
    public string BirdName { get; set; }
    public DateTime TimeSpent { get; set; }
    public Photo Photo { get; set; }
    public Model Model { get; set; }
    public Guid ModelId { get; set; }
}