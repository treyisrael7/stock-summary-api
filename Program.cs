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
    // Validate symbol input
    var trimmedSymbol = symbol?.Trim() ?? string.Empty;
    if (string.IsNullOrEmpty(trimmedSymbol))
    {
        return Results.BadRequest(new { error = "Symbol cannot be empty" });
    }
    
    if (trimmedSymbol.Contains(' '))
    {
        return Results.BadRequest(new { error = "Symbol cannot contain spaces" });
    }
    
    try
    {
        var intradayPoints = await client.GetIntradayPointsAsync(trimmedSymbol);
        
        var daySummaries = intradayPoints
            .GroupBy(p => p.Time.Date)
            .Select(g =>
            {
                var lowValues = g.Where(p => p.Low.HasValue).Select(p => p.Low!.Value).ToList();
                var highValues = g.Where(p => p.High.HasValue).Select(p => p.High!.Value).ToList();
                var volumeValues = g.Where(p => p.Volume.HasValue).Select(p => p.Volume!.Value).ToList();
                
                var lowAverage = lowValues.Count > 0 ? Math.Round(lowValues.Average(), 4) : 0.0;
                var highAverage = highValues.Count > 0 ? Math.Round(highValues.Average(), 4) : 0.0;
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
    catch (InvalidOperationException ex) when (ex.Message == "Symbol not found")
    {
        return Results.NotFound(new { error = "Symbol not found" });
    }
    catch (HttpRequestException)
    {
        return Results.Json(new { error = "Upstream service error" }, statusCode: 502);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error processing data: {ex.Message}");
    }
});

app.MapGet("/api/raw/{symbol}", async (string symbol, YahooFinanceClient client) =>
{
    // Validate symbol input
    var trimmedSymbol = symbol?.Trim() ?? string.Empty;
    if (string.IsNullOrEmpty(trimmedSymbol))
    {
        return Results.BadRequest(new { error = "Symbol cannot be empty" });
    }
    
    if (trimmedSymbol.Contains(' '))
    {
        return Results.BadRequest(new { error = "Symbol cannot contain spaces" });
    }
    
    try
    {
        var jsonDocument = await client.GetChartDataAsync(trimmedSymbol);
        return Results.Ok(jsonDocument);
    }
    catch (HttpRequestException)
    {
        return Results.Json(new { error = "Upstream service error" }, statusCode: 502);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error fetching data: {ex.Message}");
    }
});

app.Run();
