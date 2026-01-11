using StockSummaryApi.Models;
using StockSummaryApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<YahooFinanceClient>();

var app = builder.Build();

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

app.MapGet("/api/raw/{symbol}", async (string symbol, YahooFinanceClient client) =>
{
    try
    {
        var jsonDocument = await client.GetChartDataAsync(symbol);
        return Results.Ok(jsonDocument);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error fetching data: {ex.Message}");
    }
});

app.Run();
