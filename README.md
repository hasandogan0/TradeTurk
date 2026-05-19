# TRadeTurk

TRadeTurk is a premium fintech/trading platform demo built with .NET, Clean Architecture, CQRS, SignalR, React and live Binance market data. It provides a demo wallet, virtual card, multi-market trading, advanced order tickets, portfolio analytics and real-time dashboard experiences.

## Features

- JWT authentication with refresh token rotation and logout revoke
- Automatic 50,000 USDT demo wallet and masked virtual card on registration
- Multi-market watchlist for BTC, ETH, BNB, SOL, XRP, ADA, DOGE, AVAX, DOT, LINK, MATIC, LTC, TRX, ATOM and NEAR
- Market, limit, stop-loss and take-profit order flow
- Open orders, order history and cancel pending order support
- Wallet, assets, transactions and portfolio summary screens
- Portfolio snapshot worker and historical equity curve endpoint
- SignalR live price, ticker and order notifications
- Binance proxy service with cache, rate-limit protection and deterministic fallback for tests/demo
- Responsive React dashboard with TradingView chart, premium fintech cards and mobile bottom navigation
- Docker, docker-compose and GitHub Actions CI workflow

## Architecture

TRadeTurk follows Clean Architecture:

```text
Domain -> Application -> Infrastructure -> WebAPI
```

- `Domain`: entities, enums and core business rules. It knows nothing about EF Core, controllers, JWT, SignalR or Binance.
- `Application`: CQRS commands/queries, DTOs and abstractions.
- `Infrastructure`: EF Core, repositories, unit of work, Binance proxy, token services and background workers.
- `WebAPI`: controllers, middleware, authentication scheme, CORS, rate limiting and SignalR endpoint.
- `frontend`: React + TypeScript + Tailwind trading terminal.

See [docs/architecture.md](docs/architecture.md) for the longer version.

## Tech Stack

- .NET 10, ASP.NET Core, EF Core, MediatR, FluentValidation
- SQL Server, Unit of Work, optimistic concurrency via `RowVersion`
- SignalR for live updates
- React, TypeScript, TanStack Query, Tailwind, Lucide Icons
- Binance API through Proxy Pattern and strategy-based price providers
- xUnit, FluentAssertions, WebApplicationFactory integration tests

## Setup

```bash
dotnet restore TRadeTurk.slnx
dotnet build TRadeTurk.slnx
dotnet test TRadeTurk.slnx
dotnet run --project src/TRadeTurk.WebAPI
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Default local URLs:

- API: `http://localhost:5129`
- Frontend: `http://localhost:5173`
- SignalR: `/priceHub`

## Docker

```bash
cp .env.example .env
cp frontend/.env.example frontend/.env
docker compose up --build
```

## API

Endpoint details are documented in [docs/api-endpoints.md](docs/api-endpoints.md).

## Testing

Tests avoid real Binance calls. Integration tests replace price and market services with deterministic fakes.

```bash
dotnet test TRadeTurk.slnx
cd frontend
npm run build
```

See [docs/testing.md](docs/testing.md).

## Roadmap

- Email verification and password reset flows
- Device/session management UI
- Persisted notification center
- Advanced chart overlays and saved layouts
- Live trading provider abstraction beyond simulated execution
