var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (!Directory.Exists("./assets")) {
  Directory.CreateDirectory("./assets");
}

app.Run();
