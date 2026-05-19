# Architecture

TRadeTurk uses Clean Architecture with CQRS.

## Dependency Direction

```text
TRadeTurk.Domain
  <- TRadeTurk.Application
    <- TRadeTurk.Infrastructure
    <- TRadeTurk.WebAPI
```

The Domain layer contains entities such as `User`, `Wallet`, `Asset`, `Transaction`, `Order`, `PortfolioSnapshot`, `RefreshToken` and `AuditLog`. It does not reference EF Core, JWT, SignalR or controller concerns.

## CQRS Flow

Controllers send commands and queries through MediatR:

```text
Controller -> MediatR -> Command/Query Handler -> Repository/UnitOfWork -> DbContext
```

Examples:

- `CreateOrderCommand` creates and optionally executes orders.
- `GetOpenOrdersQuery` returns only the authenticated user's pending orders.
- `GetPortfolioHistoryQuery` returns snapshot history for the authenticated user.

## Strategy Pattern

Price resolution uses strategy-style abstractions:

- `IPriceProviderStrategy`
- `BinancePriceProviderStrategy`
- `MockPriceProviderStrategy`
- `PriceProviderContext`

This allows production code to use live Binance data while tests use deterministic prices.

## Proxy Pattern

`BinanceProxyService` protects live Binance calls with:

- memory cache
- simple request spacing
- last-good fallback
- deterministic demo fallback

## SignalR Flow

`BinanceDataWorker` publishes live ticker updates to `PriceHub`.
`PendingOrderWorker` processes executable pending orders and broadcasts notification events.

## Background Workers

- `BinanceDataWorker`: streams supported market prices.
- `PendingOrderWorker`: checks limit, stop-loss and take-profit orders.
- `PortfolioSnapshotWorker`: stores portfolio value snapshots every five minutes.
