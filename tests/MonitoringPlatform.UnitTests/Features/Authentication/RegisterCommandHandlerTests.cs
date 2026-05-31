using FluentAssertions;
using Moq;
using MonitoringPlatform.Application.Features.Authentication.Commands;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;
using Xunit;

namespace MonitoringPlatform.UnitTests.Features.Authentication;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _organizationRepositoryMock = new Mock<IOrganizationRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();

        _handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _organizationRepositoryMock.Object,
            _jwtServiceMock.Object,
            _passwordHashingServiceMock.Object,
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<RegisterCommandHandler>()
        );
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowDuplicateException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Test@123",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<Guid?>()))
            .ReturnsAsync(new User { UserId = Guid.NewGuid(), Email = command.Email });

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<DuplicateException>()
            .WithMessage($"User with email {command.Email} already exists");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateUserAndReturnResponse()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "new@example.com",
            Password = "Test@123",
            FirstName = "Jane",
            LastName = "Doe"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<Guid?>()))
            .ReturnsAsync((User?)null);

        _organizationRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Organization>()))
            .ReturnsAsync((Organization o) =>
            {
                o.OrganizationId = Guid.NewGuid();
                return o;
            });

        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        _jwtServiceMock.Setup(x => x.GenerateTokensAsync(It.IsAny<User>()))
            .ReturnsAsync(new AuthTokens
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            });

        _userRepositoryMock.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Tokens.Should().NotBeNull();
        result.Tokens.AccessToken.Should().Be("test-access-token");
        result.Tokens.RefreshToken.Should().Be("test-refresh-token");
    }
}