# API Endpoints

## Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

## Users

- `GET /api/users/me`
- `PUT /api/users/me`
- `PUT /api/users/me/password`

## Wallet

- `GET /api/wallet/me`

## Markets

- `GET /api/markets/symbols`
- `GET /api/markets/tickers`
- `GET /api/prices/{symbol}`

## Trading

- `POST /api/trade/buy`
- `POST /api/trade/sell`

## Advanced Orders

- `POST /api/orders`
- `GET /api/orders/open`
- `GET /api/orders/history`
- `GET /api/orders/{id}`
- `DELETE /api/orders/{id}`

Order create body:

```json
{
  "symbol": "BTCUSDT",
  "side": "BUY",
  "type": "LIMIT",
  "quantity": 0.1,
  "price": 50000,
  "triggerPrice": null
}
```

## Portfolio

- `GET /api/portfolio/summary/me`
- `GET /api/portfolio/history/me?range=7D`

Supported ranges: `1D`, `7D`, `1M`, `3M`, `1Y`.

## Transactions

- `GET /api/transactions/me`
