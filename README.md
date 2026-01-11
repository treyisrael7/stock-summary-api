# StockSummaryApi

A .NET 8 API project for stock summary information.

## Prerequisites

- .NET 8 SDK

## How to Run

Run the following command from the project directory:

```bash
dotnet run
```

## Base URL

The API will be available at:

- HTTPS: `https://localhost:7148`
- HTTP: `http://localhost:5274`

## API Endpoints

### GET /api/summary/{symbol}

Returns stock summary data aggregated by day for the given symbol.

**Example Request:**
```
GET http://localhost:5274/api/summary/AAPL
```

**Example Response:**
```json
[
  {
    "day": "2024-01-15",
    "lowAverage": 150.2500,
    "highAverage": 155.7500,
    "volume": 1000000
  },
  {
    "day": "2024-01-16",
    "lowAverage": 152.1234,
    "highAverage": 157.5678,
    "volume": 1250000
  }
]
```

**Response Format:**
- `day`: String formatted as `yyyy-MM-dd`
- `lowAverage`: Double rounded to 4 decimal places
- `highAverage`: Double rounded to 4 decimal places
- `volume`: Whole number (long)