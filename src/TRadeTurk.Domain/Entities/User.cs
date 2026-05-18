using TRadeTurk.Domain.Common;

namespace TRadeTurk.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; private set; } = string.Empty;

    private User()
    {
    }

    public User(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name cannot be empty.");

        UserName = userName.Trim();
    }
}
