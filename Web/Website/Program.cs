using Birds.EntityFramework.Contexts;
using Birds.Repository;
using Birds.Repository.Birds;
using Microsoft.EntityFrameworkCore;
using Model = Birds.EntityFramework.Entities.Model;

const string modelPath = "ML/BirdModel24.model";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BirdsContext>(
    option => option.UseSqlServer(builder.Configuration.GetConnectionString("BirdsConnectionString"),
        b => b.MigrationsAssembly("Birds.EntityFramework"))
);

builder.Services.AddScoped<IBirdsRepository, BirdsRepository>();
builder.Services.AddSingleton<DataLoader>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}");

    endpoints.MapControllerRoute(
        name: "api",
        pattern: "api/{controller}/{id?}");
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Birds}/{action=Index}/{id?}");

using (IServiceScope scope = app.Services.CreateScope())
{
    BirdsContext birdsContext = scope.ServiceProvider.GetRequiredService<BirdsContext>();

    if (!birdsContext.Models.Any())
    {
        byte[] file;
        using (FileStream fileStream = new(modelPath, FileMode.Open, FileAccess.Read))
        {
            file = new byte[fileStream.Length];
            fileStream.Read(file, 0, file.Length);
        }

        Model model = new()
        {
            Id = Guid.NewGuid(),
            Name = Path.GetFileName(modelPath),
            Data = file
        };

        birdsContext.Models.Add(model);
        birdsContext.SaveChanges();
    }
}

using (IServiceScope scope = app.Services.CreateScope())
{
    BirdsContext birdsContext = scope.ServiceProvider.GetRequiredService<BirdsContext>();
    DataLoader dataLoader = app.Services.GetRequiredService<DataLoader>();
    dataLoader.Load(birdsContext);
}

app.Run();