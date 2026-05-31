using FluentAssertions;
using Moq;
using MonitoringPlatform.Application.Features.Monitors.Commands;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;
using Xunit;

namespace MonitoringPlatform.UnitTests.Features.Monitors;

public class CreateMonitorCommandHandlerTests
{
    private readonly Mock<IMonitorRepository> _monitorRepositoryMock;
    private readonly CreateMonitorCommandHandler _handler;

    public CreateMonitorCommandHandlerTests()
    {
        _monitorRepositoryMock = new Mock<IMonitorRepository>();
        _handler = new CreateMonitorCommandHandler(_monitorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateMonitorAndReturnDto()
    {
        // Arrange
        var command = new CreateMonitorCommand
        {
            OrganizationId = Guid.NewGuid(),
            Name = "HTTP Test Monitor",
            Description = "Monitoring our main website",
            Type = MonitorType.Http,
            Target = "https://example.com",
            IntervalSeconds = 60,
            TimeoutSeconds = 30,
            Retries = 3,
            HttpMethod = "GET"
        };

        _monitorRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.Monitor>()))
            .ReturnsAsync((Domain.Entities.Monitor m) => m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(command.Name);
        result.Value.Target.Should().Be(command.Target);
        result.Value.Type.Should().Be(command.Type);
        result.Value.IntervalSeconds.Should().Be(command.IntervalSeconds);
        result.Value.TimeoutSeconds.Should().Be(command.TimeoutSeconds);

        _monitorRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.Monitor>()), Times.Once);
    }
}
