using StockSummaryApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/summary/{symbol}", (string symbol) =>
{
    var summary = new DaySummary[]
    {
        new DaySummary(
            Day: "2024-01-15",
            LowAverage: 150.25,
            HighAverage: 155.75,
            Volume: 1000000L
        )
    };
    
    return Results.Ok(summary);
});

app.Run();
