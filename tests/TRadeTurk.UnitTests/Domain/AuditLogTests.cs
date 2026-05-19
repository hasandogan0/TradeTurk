using FluentAssertions;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.UnitTests.Domain;

public class AuditLogTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var userId = Guid.NewGuid();
        var log = new AuditLog(userId, "Login", "127.0.0.1", "TestAgent/1.0");

        log.UserId.Should().Be(userId);
        log.Action.Should().Be("Login");
        log.IpAddress.Should().Be("127.0.0.1");
        log.UserAgent.Should().Be("TestAgent/1.0");
    }

    [Fact]
    public void Constructor_ShouldAllowNullUserId()
    {
        var log = new AuditLog(null, "FailedLogin", "127.0.0.1", "TestAgent");

        log.UserId.Should().BeNull();
        log.Action.Should().Be("FailedLogin");
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyAction()
    {
        var act = () => new AuditLog(Guid.NewGuid(), "", "127.0.0.1", "TestAgent");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldTrimValues()
    {
        var log = new AuditLog(Guid.NewGuid(), "  Register  ", " 192.168.1.1 ", " Chrome ");

        log.Action.Should().Be("Register");
        log.IpAddress.Should().Be("192.168.1.1");
        log.UserAgent.Should().Be("Chrome");
    }
}
