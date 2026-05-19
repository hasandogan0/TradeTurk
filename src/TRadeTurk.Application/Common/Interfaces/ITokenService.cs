using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Application.Common.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}
