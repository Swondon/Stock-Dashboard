# Stock Dashboard

Full-stack app that pulls intraday market data from Yahoo Finance's public chart endpoint,
groups it into daily low/high averages and volume, and displays it in a React dashboard.

- **Backend:** C# / ASP.NET Core (.NET 8), minimal dependency footprint (no third-party NuGet
  packages — everything ships in the ASP.NET Core shared framework).
- **Frontend:** React + TypeScript (Vite), charting via [Recharts](https://recharts.org/).

## Prerequisites

Either:

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose (v2, bundled with modern
  Docker Desktop/Engine installs) - see [Running with Docker](#running-with-docker), or
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later, plus
  [Node.js](https://nodejs.org/) 20+ and npm, for running each side natively (see below)

## Project layout

```
stock-dashboard/
  backend/
    StockDashboard.Api/         ASP.NET Core Web API (+ Dockerfile)
    StockDashboard.Api.Tests/   xUnit tests
  frontend/                     React + TypeScript (Vite) app (+ Dockerfile, nginx.conf)
  docker-compose.yml            Runs both containers together
```

## Running with Docker

```
docker compose up --build
```

Builds and starts both containers - the backend on `http://localhost:5080` and the frontend on
`http://localhost:5173` - wired together the same way as running them natively (see
`docker-compose.yml`). The frontend's container serves the static production build via nginx
(`frontend/Dockerfile`, `frontend/nginx.conf`); `VITE_API_BASE_URL` is baked into that build at
image-build time (Vite inlines `VITE_*` env vars at build time, not read at container start), so
changing it requires rebuilding the frontend image, not just restarting the container. The
backend is a standard multi-stage build (`backend/StockDashboard.Api/Dockerfile`) - SDK image to
publish, then the much smaller ASP.NET Core runtime image to actually run it.

Stop everything with `docker compose down`.

## Running the backend (without Docker)

```
cd backend/StockDashboard.Api
dotnet run
```

The API listens on `http://localhost:5080` (fixed in `Properties/launchSettings.json` so the
frontend's default config can point at it reliably).

Try it directly:

```
curl "http://localhost:5080/api/stocks/TSLA/daily-summary"
```

Response shape:

```json
[
  { "day": "2026-07-31", "lowAverage": 341.2201, "highAverage": 349.8843, "volume": 41823110 }
]
```

Errors (invalid symbol, upstream failure) come back as a standard
[`ProblemDetails`](https://www.rfc-editor.org/rfc/rfc7807) JSON body with an appropriate HTTP
status code (400 for a malformed symbol, 404 for a symbol Yahoo doesn't recognize, 502 if Yahoo
itself is unreachable/erroring).

There's also a symbol-search endpoint, used by the frontend's autocomplete dropdown:

```
curl "http://localhost:5080/api/stocks/search?q=app"
```

```json
[
  { "symbol": "APP", "name": "AppLovin Corporation", "exchange": "NASDAQ" },
  { "symbol": "AAPL", "name": "Apple Inc.", "exchange": "NASDAQ" }
]
```

This proxies Yahoo's own symbol-search endpoint (same CORS reasoning as the chart endpoint —
Yahoo doesn't allow direct browser calls) and is intentionally best-effort: a failed or empty
lookup just returns `[]` rather than an error, since it backs an autocomplete list rather than a
user-facing action.

### Running the backend tests

```
cd backend/StockDashboard.Api.Tests
dotnet test
```

- `DailyAggregationServiceTests` covers the day-grouping/averaging logic directly, with no HTTP
  mocking required since that logic takes plain data in and returns plain data out.
- `StockDataServiceTests` covers the symbol-validation error path (invalid/empty symbols are
  rejected before ever reaching the upstream HTTP client), using small hand-written fakes for
  `IYahooFinanceClient`/`IDailyAggregationService`/`IMemoryCache` rather than a mocking library.
- `GlobalExceptionHandlerTests` covers the exception → HTTP status code + `ProblemDetails` body
  mapping for every domain exception, plus the generic-500 fallback for anything unrecognized.

### Configuration

`backend/StockDashboard.Api/appsettings.json`:

| Key                            | Purpose                                              | Default                                |
|---------------------------------|-------------------------------------------------------|-----------------------------------------|
| `YahooFinance:BaseUrl`          | Yahoo chart API base URL                              | `https://query1.finance.yahoo.com/`     |
| `YahooFinance:TimeoutSeconds`   | HTTP client timeout when calling Yahoo                | `10`                                    |
| `YahooFinance:CacheDurationSeconds` | How long a symbol's result is cached in memory   | `60`                                    |
| `Cors:AllowedOrigins`           | Origins allowed to call the API (the frontend's URL)  | `http://localhost:5173`                 |

## Running the frontend (without Docker)

```
cd frontend
npm install
npm run dev
```

Opens on `http://localhost:5173` by default. It reads the backend URL from
`VITE_API_BASE_URL` (see `.env` / `.env.example`); defaults to `http://localhost:5080`.

## How it works

1. The frontend posts a symbol to `GET /api/stocks/{symbol}/daily-summary`.
2. The backend validates the symbol format, then (unless a cached result is still fresh) calls
   Yahoo's chart endpoint (`/v8/finance/chart/{symbol}?interval=15m&period1=...&period2=...`) for
   the last 31 days of 15-minute bars.
3. Each bar's timestamp is converted to the exchange's local calendar day (using the `gmtoffset`
   Yahoo returns), then grouped: `lowAverage`/`highAverage` are the mean of that day's bar lows/
   highs (rounded to 4 decimal places), and `volume` is the sum of that day's bar volumes.
4. The result is cached in memory per symbol for `CacheDurationSeconds` to avoid hammering
   Yahoo's unauthenticated endpoint on repeated requests for the same symbol.

## Known limitations / next steps

- Yahoo's chart endpoint is unofficial and unauthenticated — it can rate-limit or change shape
  without notice. There's no retry/circuit-breaker policy yet; that's a reasonable next addition
  (e.g. `Microsoft.Extensions.Http.Resilience`) once real usage patterns are known.
- No OpenAPI/Swagger UI is wired up, to keep the backend's dependency footprint at zero external
  NuGet packages (useful in a network-restricted environment). Worth adding once the project has
  more than one endpoint.
- The in-memory cache is per-process; a multi-instance deployment would need a shared cache
  (e.g. Redis) instead.

See `PROMPT_LOG.md` for the AI-collaboration notes on how this was built.
