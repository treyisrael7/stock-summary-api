namespace StockSummaryApi.Models;

public record DaySummary(
    string Day,
    double LowAverage,
    double HighAverage,
    long Volume
);
