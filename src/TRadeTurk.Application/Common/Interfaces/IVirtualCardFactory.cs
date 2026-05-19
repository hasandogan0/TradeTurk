using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Common.Interfaces;

public interface IVirtualCardFactory
{
    Card Create(Guid userId, Guid walletId, string cardHolderName, decimal initialBalance);
}
