using MyApiOtel;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
                tracerProviderBuilder
                    .AddSource(DiagnosticsConfig.ActivitySource.Name)
                                .ConfigureResource(resource => resource
                                    .AddService(DiagnosticsConfig.ServiceName))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter()
                )
            .WithMetrics(metricsProviderBuilder => 
                metricsProviderBuilder.ConfigureResource(configure => 
                    configure.AddService(DiagnosticsConfig.ServiceName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS with the defined policy
app.UseCors("AllowLocalhost2999");

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

app.MapGet("/fibonacci/{n:long}", (long n) =>
{
    static long Fibonacci(long n)
    {
        long a = 0, b = 0, c = 1;
        for (int i = 1; i < n; i++)
        {
            a = b;
            b = c;
            c = a + b;
        }
        return c;
    }

    // Validate input
    if (n < 0)
    {
        return Results.BadRequest("Please provide a non-negative integer");
    }

    // Calculate and return the result
    var result = Fibonacci(n);
    return Results.Ok($"Fibonacci of {n} is {result}");
})
.WithName("GetFibonacci")
.WithOpenApi();

app.MapGet("/custom-trace", () =>
{
    using (var activity = DiagnosticsConfig.ActivitySource.StartActivity("custom-trace", ActivityKind.Internal))
    {

        // your logic for Main activity
        activity?.SetTag("foo", "bar1");

        var _logger = app.Services.GetRequiredService<ILogger<Program>>();
        _logger.LogInformation("Custom trace");
    }
    
})
.WithName("GetWithCustomChildTrace")
.WithOpenApi();

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
