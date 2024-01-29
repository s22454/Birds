using Birds.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Birds.EntityFramework.Contexts;

public class BirdsContext : DbContext
{
    public BirdsContext(DbContextOptions<BirdsContext> options) : base(options)
    {
    }

    public DbSet<Prediction> Predictions { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Model> Models { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prediction>(x =>
        {
            x.Property(p => p.BirdName).HasColumnType("nvarchar(255)");
            x.Property(p => p.TimeSpent).HasColumnType("datetime2");
            x.HasOne(prediction => prediction.Photo)
                .WithOne(photo => photo.Prediction)
                .HasForeignKey<Photo>(photo => photo.PredictionId);
        });

        modelBuilder.Entity<Photo>(x =>
        {
            x.Property(p => p.Name).HasColumnType("nvarchar(255)");
            x.Property(p => p.Data).HasColumnType("varbinary(max)");
        });

        modelBuilder.Entity<Model>(x =>
        {
            x.Property(p => p.Name).HasColumnType("nvarchar(255)");
            x.Property(p => p.Data).HasColumnType("varbinary(max)");
            x.HasMany(model => model.Predictions)
                .WithOne(prediction => prediction.Model)
                .HasForeignKey(prediction => prediction.ModelId);
        });
    }
}