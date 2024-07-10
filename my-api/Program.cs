using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost2999",
        builder =>
        {
            builder.WithOrigins("http://localhost:2999")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Enable CORS with the defined policy
app.UseCors("AllowLocalhost2999");

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/custom-trace", () =>
{
    using (Activity? activity = MyApi.DiagnosticsConfig.ActivitySource.StartActivity("custom-trace", ActivityKind.Internal))
    {
        // your logic for Main activity
        activity?.SetTag("foo", "bar1");

        var _logger = app.Services.GetRequiredService<ILogger<Program>>();
        _logger.LogInformation("Custom trace");
    }

})
.WithName("GetWithCustomChildTrace")
.WithOpenApi();

//app.MapGet("/custom-trace", () =>
//{
//    using (var scope = Tracer.Instance.StartActive($"Custom trace"))
//    {
//        var _logger = app.Services.GetRequiredService<ILogger<Program>>();
//        _logger.LogInformation("Custom trace");
//    }
//})
//.WithName("GetWithCustomChildTrace")
//.WithOpenApi();

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
