namespace StockSummaryApi.Models;

public record IntradayPoint(
    DateTimeOffset Time,
    double? Low,
    double? High,
    long? Volume
);
