using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuctionDatabase")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();


app.UseAuthorization();
app.MapControllers();

try
{
    DBInitializer.InitializeDb(app);
}
catch (System.Exception e)
{
    Console.WriteLine("An error occurred while migrating or initializing the database.", e.Message);
}
app.Run();
