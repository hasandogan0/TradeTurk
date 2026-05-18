using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TRadeTurk.Application.Common.Behaviors;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.Features.Assets.Commands;
using TRadeTurk.Infrastructure.BackgroundJobs;
using TRadeTurk.Infrastructure.Data;
using TRadeTurk.Infrastructure.Hubs;
using TRadeTurk.Infrastructure.Repositories;
using TRadeTurk.Infrastructure.Services;
using TRadeTurk.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<BinanceService>();
builder.Services.AddScoped<IBinancePriceService, BinanceProxyService>();
builder.Services.AddScoped<IPriceProviderStrategy, BinancePriceProviderStrategy>();
builder.Services.AddScoped<IPriceProviderStrategy, MockPriceProviderStrategy>();
builder.Services.AddScoped<IPriceProviderContext, PriceProviderContext>();

builder.Services.AddHostedService<BinanceDataWorker>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(BuyAssetCommandHandler).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(BuyAssetCommandHandler).Assembly);
builder.Services.AddSignalR();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PriceHub>("/priceHub");

app.Run();

public partial class Program
{
}
