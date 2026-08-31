using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StockDashboard.Api.ErrorHandling;
using StockDashboard.Api.Options;
using StockDashboard.Api.Services;

// Application entry point
var builder = WebApplication.CreateBuilder(args);

// Binds the YahooFinance section
builder.Services.Configure<YahooFinanceOptions>(builder.Configuration.GetSection("YahooFinance"));

// TimeProvider.System is injected
builder.Services.AddSingleton(TimeProvider.System);

// In-memory cache for daily-summary results
builder.Services.AddMemoryCache();

// Typed HttpClient for talking to Yahoo Finance
builder.Services.AddHttpClient<IYahooFinanceClient, YahooFinanceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<YahooFinanceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");
});

// Application services
builder.Services.AddScoped<IDailyAggregationService, DailyAggregationService>();
builder.Services.AddScoped<IStockDataService, StockDataService>();

builder.Services.AddControllers();

// Routes every unhandled exception
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("Frontend");
app.MapControllers();

app.Run();
