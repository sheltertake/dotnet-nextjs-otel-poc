using MyApiChat;

var builder = WebApplication.CreateBuilder(args);

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
app.UseWebSockets();
app.UseChatWebSocket();
app.UseHttpsRedirection();

app.UseCors("AllowLocalhost2999");

// Configura il middleware per supportare il routing
app.UseRouting();

// Definisce l'endpoint /health
app.MapGet("/health", async context =>
{
    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("Healthy");
});


await app.RunAsync();
