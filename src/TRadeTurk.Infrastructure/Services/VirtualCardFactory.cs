using System.Security.Cryptography;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Services;

public class VirtualCardFactory : IVirtualCardFactory
{
    private readonly IPasswordHasher _passwordHasher;

    public VirtualCardFactory(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public Card Create(Guid userId, Guid walletId, string cardHolderName, decimal initialBalance)
    {
        var cardNumber = GenerateDigits(16);
        var cvv = GenerateDigits(3);
        var expiryMonth = RandomNumberGenerator.GetInt32(1, 13);
        var expiryYear = DateTime.UtcNow.Year + RandomNumberGenerator.GetInt32(3, 7);

        return new Card(userId, cardHolderName, cardNumber, expiryMonth, expiryYear, _passwordHasher.Hash(cvv), initialBalance, walletId);
    }

    private static string GenerateDigits(int length)
    {
        Span<char> chars = stackalloc char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(chars);
    }
}
