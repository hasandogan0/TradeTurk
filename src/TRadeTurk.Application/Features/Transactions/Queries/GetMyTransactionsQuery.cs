using MediatR;
using TRadeTurk.Application.DTOs;

namespace TRadeTurk.Application.Features.Transactions.Queries;

public class GetMyTransactionsQuery : IRequest<IReadOnlyCollection<TransactionDto>>
{
    public Guid UserId { get; set; }
}
