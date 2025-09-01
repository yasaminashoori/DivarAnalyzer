using DivarAnalyzer;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<DivarDataAnalyzer>();
builder.Services.AddScoped<DivarScraper>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Divar Real Estate Analyzer API",
        Version = "v1",
        Description = "API for analyzing Tehran real estate market data from Divar.ir"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Divar Analyzer API v1");
        c.RoutePrefix = "api-docs";
    });
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
});

app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "https://localhost:5001";
    Console.WriteLine($"Divar Real Estate Analyzer is running!");
    Console.WriteLine($"Web Interface: {urls}");
    Console.WriteLine($"API Documentation: {urls}/api-docs");
});

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex.Message}");
    throw;
}