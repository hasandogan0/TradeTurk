using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Assets.Queries;

public class GetUserAssetsQueryHandler : IRequestHandler<GetUserAssetsQuery, IReadOnlyCollection<AssetDto>>
{
    private readonly IRepository<Asset> _assetRepository;

    public GetUserAssetsQueryHandler(IRepository<Asset> assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<IReadOnlyCollection<AssetDto>> Handle(GetUserAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await _assetRepository.FindAsync(a => a.UserId == request.UserId && a.Amount > 0, cancellationToken);

        return assets.Select(a => new AssetDto
        {
            Id = a.Id,
            UserId = a.UserId,
            WalletId = a.WalletId,
            Symbol = a.Symbol,
            Amount = a.Amount,
            AverageCost = a.AverageCost
        }).ToArray();
    }
}
