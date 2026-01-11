using StockSummaryApi.Models;
using StockSummaryApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<YahooFinanceClient>();

var app = builder.Build();

app.MapGet("/", () => new
{
    message = "Stock Summary API",
    endpoints = new[]
    {
        "GET /api/summary/{symbol} - Get stock summary aggregated by day",
        "GET /api/raw/{symbol} - Get raw Yahoo Finance data"
    }
});

app.MapGet("/api/summary/{symbol}", async (string symbol, YahooFinanceClient client) =>
{
    try
    {
        var intradayPoints = await client.GetIntradayPointsAsync(symbol);
        
        var daySummaries = intradayPoints
            .GroupBy(p => p.Time.Date)
            .Select(g =>
            {
                var lowValues = g.Where(p => p.Low.HasValue).Select(p => p.Low!.Value).ToList();
                var highValues = g.Where(p => p.High.HasValue).Select(p => p.High!.Value).ToList();
                var volumeValues = g.Where(p => p.Volume.HasValue).Select(p => p.Volume!.Value).ToList();
                
                var lowAverage = lowValues.Count > 0 ? lowValues.Average() : 0.0;
                var highAverage = highValues.Count > 0 ? highValues.Average() : 0.0;
                var volume = volumeValues.Sum();
                
                return new DaySummary(
                    Day: g.Key.ToString("yyyy-MM-dd"),
                    LowAverage: lowAverage,
                    HighAverage: highAverage,
                    Volume: volume
                );
            })
            .OrderBy(ds => ds.Day)
            .ToList();
        
        return Results.Ok(daySummaries);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error processing data: {ex.Message}");
    }
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
