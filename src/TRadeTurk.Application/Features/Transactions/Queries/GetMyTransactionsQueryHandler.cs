using MediatR;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Application.DTOs;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Features.Transactions.Queries;

public class GetMyTransactionsQueryHandler : IRequestHandler<GetMyTransactionsQuery, IReadOnlyCollection<TransactionDto>>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public GetMyTransactionsQueryHandler(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyCollection<TransactionDto>> Handle(GetMyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.FindAsync(t => t.UserId == request.UserId, cancellationToken);

        return transactions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString().ToUpperInvariant(),
                Symbol = t.Symbol ?? string.Empty,
                Amount = t.Amount,
                Price = t.Price,
                Total = t.Amount * t.Price,
                CreatedAt = t.CreatedAt,
                Status = t.Status.ToString().ToUpperInvariant()
            })
            .ToArray();
    }
}
