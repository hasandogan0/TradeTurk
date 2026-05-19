# Testing

TRadeTurk uses xUnit, FluentAssertions and `WebApplicationFactory`.

## Principles

- No tests call the real Binance API.
- Integration tests replace price providers with deterministic fakes.
- Authenticated endpoints are tested with JWT tokens from the real login flow.
- User-owned resources are checked through `/me` endpoints to avoid frontend-supplied `userId`.

## Commands

```bash
dotnet build TRadeTurk.slnx --no-restore
dotnet test TRadeTurk.slnx --no-build --no-restore
cd frontend
npm run build
```

## Covered Areas

- registration creates wallet and virtual card
- login returns access and refresh tokens
- wallet, transactions and portfolio endpoints are authenticated
- buy/sell updates wallet and assets
- market endpoints return supported symbols and deterministic tickers
- advanced order endpoints create, list and cancel orders
- portfolio history endpoint returns authenticated user snapshot data
