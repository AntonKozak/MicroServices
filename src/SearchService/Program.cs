using SearchService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SearchService API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SearchService API v1"));
}

app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch (System.Exception)
{
    Console.WriteLine("Failed to initialize database ------------ --- --- -");
}

app.Run();
