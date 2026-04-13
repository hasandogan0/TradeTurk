using Microsoft.EntityFrameworkCore;
using TRadeTurk.Infrastructure.Data;
using TRadeTurk.Domain.Interfaces;
using TRadeTurk.Infrastructure.Repositories;
using TRadeTurk.Infrastructure.Services;
using TRadeTurk.Infrastructure.BackgroundJobs;
using TRadeTurk.Application.Features.Assets.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection - Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Dependency Injection - Services
builder.Services.AddMemoryCache(); // Proxy Pattern içi mühim
builder.Services.AddScoped<IBinanceService, BinanceProxyService>();

// Dependency Injection - Background Worker
builder.Services.AddHostedService<BinanceDataWorker>();

// Dependency Injection - MediatR (CQRS)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BuyAssetCommandHandler).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
