using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Assets.Queries;

public class GetUserAssetsQuery : IRequest<IReadOnlyCollection<AssetDto>>
{
    public Guid UserId { get; set; }
}
