using System.Text.Json;
using StockSummaryApi.Models;

namespace StockSummaryApi.Services;

public class YahooFinanceClient
{
    private const string BaseUrl = "https://query1.finance.yahoo.com/v8/finance/chart";
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
    
    private readonly HttpClient _httpClient;

    public YahooFinanceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public async Task<JsonDocument> GetChartDataAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{symbol}?range=1mo&interval=15m";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(jsonString);
    }

    public async Task<List<IntradayPoint>> GetIntradayPointsAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var jsonDocument = await GetChartDataAsync(symbol, cancellationToken);
        return ParseIntradayPoints(jsonDocument);
    }

    private static List<IntradayPoint> ParseIntradayPoints(JsonDocument jsonDocument)
    {
        var root = jsonDocument.RootElement;
        if (!TryGetFirstResult(root, out var firstResult))
            return new List<IntradayPoint>();

        var timestamps = GetArray(firstResult, "timestamp");
        if (timestamps.Count == 0)
            return new List<IntradayPoint>();

        if (!TryGetQuote(firstResult, out var quote))
            return new List<IntradayPoint>();

        var lowValues = GetArray(quote, "low");
        var highValues = GetArray(quote, "high");
        var volumeValues = GetArray(quote, "volume");

        return MapToIntradayPoints(timestamps, lowValues, highValues, volumeValues);
    }

    private static bool TryGetFirstResult(JsonElement root, out JsonElement firstResult)
    {
        firstResult = default;
        if (root.TryGetProperty("chart", out var chart) &&
            chart.TryGetProperty("result", out var result) &&
            result.ValueKind == JsonValueKind.Array &&
            result.GetArrayLength() > 0)
        {
            firstResult = result[0];
            return true;
        }
        return false;
    }

    private static bool TryGetQuote(JsonElement firstResult, out JsonElement quote)
    {
        quote = default;
        if (firstResult.TryGetProperty("indicators", out var indicators) &&
            indicators.TryGetProperty("quote", out var quoteArray) &&
            quoteArray.ValueKind == JsonValueKind.Array &&
            quoteArray.GetArrayLength() > 0)
        {
            quote = quoteArray[0];
            return true;
        }
        return false;
    }

    private static List<JsonElement> GetArray(JsonElement parent, string propertyName)
    {
        if (parent.TryGetProperty(propertyName, out var element) && 
            element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().ToList();
        }
        return new List<JsonElement>();
    }

    private static List<IntradayPoint> MapToIntradayPoints(
        List<JsonElement> timestamps,
        List<JsonElement> lowValues,
        List<JsonElement> highValues,
        List<JsonElement> volumeValues)
    {
        var points = new List<IntradayPoint>();

        for (int i = 0; i < timestamps.Count; i++)
        {
            if (timestamps[i].ValueKind != JsonValueKind.Number)
                continue;

            var time = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64());
            var low = GetDoubleValue(lowValues, i);
            var high = GetDoubleValue(highValues, i);
            var volume = GetLongValue(volumeValues, i);

            points.Add(new IntradayPoint(time, low, high, volume));
        }

        return points;
    }

    private static double? GetDoubleValue(List<JsonElement> values, int index)
    {
        return GetNumericValue(values, index, e => e.GetDouble());
    }

    private static long? GetLongValue(List<JsonElement> values, int index)
    {
        return GetNumericValue(values, index, e => e.GetInt64());
    }

    private static T? GetNumericValue<T>(List<JsonElement> values, int index, Func<JsonElement, T> converter) where T : struct
    {
        if (index < values.Count && values[index].ValueKind == JsonValueKind.Number)
        {
            return converter(values[index]);
        }
        return null;
    }
}
