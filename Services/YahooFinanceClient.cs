using System.Text.Json;

namespace StockSummaryApi.Services;

public class YahooFinanceClient
{
    private readonly HttpClient _httpClient;

    public YahooFinanceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<JsonDocument> GetChartDataAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?range=1mo&interval=15m";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(jsonString);
    }
}
