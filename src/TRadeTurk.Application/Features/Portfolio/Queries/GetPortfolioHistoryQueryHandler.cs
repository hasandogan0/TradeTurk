using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Portfolio.Queries;

public class GetPortfolioHistoryQueryHandler : IRequestHandler<GetPortfolioHistoryQuery, IReadOnlyCollection<PortfolioHistoryPointDto>>
{
    private readonly IRepository<PortfolioSnapshot> _snapshotRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetPortfolioHistoryQueryHandler(IRepository<PortfolioSnapshot> snapshotRepository, ICurrentUserContext currentUserContext)
    {
        _snapshotRepository = snapshotRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<PortfolioHistoryPointDto>> Handle(GetPortfolioHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId ?? throw new InvalidOperationException("Oturum bulunamadi.");
        var since = DateTime.UtcNow - ParseRange(request.Range);

        return (await _snapshotRepository.FindAsync(s => s.UserId == userId && s.CreatedAt >= since, cancellationToken))
            .OrderBy(s => s.CreatedAt)
            .Select(s => new PortfolioHistoryPointDto
            {
                CreatedAt = s.CreatedAt,
                TotalValue = s.TotalValue,
                AvailableUSDT = s.AvailableUSDT,
                AssetValue = s.AssetValue,
                TotalPnL = s.TotalPnL
            })
            .ToArray();
    }

    private static TimeSpan ParseRange(string range)
    {
        return range.Trim().ToUpperInvariant() switch
        {
            "1D" => TimeSpan.FromDays(1),
            "7D" => TimeSpan.FromDays(7),
            "1M" => TimeSpan.FromDays(30),
            "3M" => TimeSpan.FromDays(90),
            "1Y" => TimeSpan.FromDays(365),
            _ => TimeSpan.FromDays(7)
        };
    }
}
