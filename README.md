# TRadeTurk 📈

TRadeTurk is an enterprise-grade Trading Simulation application built with .NET 10 and Clean Architecture. It allows users to simulate cryptocurrency trading using real-time data from the Binance API, manage virtual wallets, and execute buy/sell transactions with realistic conditions such as commission fees and price slippage.

## 🚀 Key Features

- **Real-time Market Data:** Fetches live cryptocurrency prices using a background worker integrated with the Binance API.
- **Proxy-Supported Binance Service:** Reliable integration with external APIs using caching and proxy patterns.
- **Live Price Updates:** Uses SignalR for real-time streaming of price updates to connected clients (`/priceHub`).
- **Virtual Wallets & Assets:** Manage virtual portfolios, and securely deposit/withdraw simulated funds via virtual cards.
- **Trading Engine:** Execute Buy/Sell transactions with realistic slippage algorithms and commission calculations.
- **CQRS Pattern:** Implements Command Query Responsibility Segregation using MediatR for scalable and decoupled architecture.

## 🏗️ Architecture & Technologies

The project strictly follows **Clean Architecture** principles to ensure separation of concerns, testability, and long-term maintainability.

- **.NET 10** (C# 14)
- **Entity Framework Core** (SQL Server)
- **MediatR** (CQRS Pattern)
- **FluentValidation** (Pipeline Exception Handling & Request Validation)
- **SignalR** (WebSockets for real-time price feeds)
- **AutoMapper** (Object-to-object mapping for DTOs)
- **Swagger/OpenAPI** (API Documentation)
- **Background Services** (`IHostedService` for scheduled background jobs like `BinanceDataWorker`)

## 📂 Project Structure

- `TRadeTurk.Domain`: Core domain entities (`Asset`, `Wallet`, `Transaction`, `Card`), enums (`TransactionStatus`), trading strategies, and repository interfaces.
- `TRadeTurk.Application`: Business logic, CQRS Handlers (MediatR), Validators (FluentValidation), and Mappers.
- `TRadeTurk.Infrastructure`: Database context (`ApplicationDbContext`), Entity Framework Core migrations, generic repositories, Unit of Work, Binance API proxy service, and Background Jobs.
- `TRadeTurk.WebAPI`: ASP.NET Core API controllers, Exception Handling Middleware, SignalR Hubs (`PriceHub`), and Dependency Injection setup.

## 🛠️ Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- SQL Server (LocalDB, Developer Edition, or Docker container)
- Supported IDE (Visual Studio 2022, Rider, or VS Code)

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/hasandogan0/TradeTurk.git
   cd TRadeTurk
   ```

2. **Configure the Database:**
   Update the `ConnectionStrings:DefaultConnection` in `src/TRadeTurk.WebAPI/appsettings.json` (or `appsettings.Development.json`) to point to your local SQL Server instance.

3. **Apply Database Migrations:**
   Using the .NET CLI, run the following command to update your database schema:
   ```bash
   dotnet ef database update --project src/TRadeTurk.Infrastructure --startup-project src/TRadeTurk.WebAPI
   ```

4. **Run the Application:**
   ```bash
   dotnet run --project src/TRadeTurk.WebAPI
   ```

5. **Access the API Documentation:**
   Once the application is running, open your browser and navigate to `https://localhost:<port>/swagger` to view and test the API endpoints.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the issues page.

## 📄 License

This project is licensed under the terms of the license provided in the `LICENSE` file.