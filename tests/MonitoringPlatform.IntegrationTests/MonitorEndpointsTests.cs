using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MonitoringPlatform.Application.Features.Authentication.Commands;
using MonitoringPlatform.API.Controllers;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Domain.Enums;
using Xunit;

namespace MonitoringPlatform.IntegrationTests;

public class MonitorEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MonitorEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> AuthenticateAsync()
    {
        var registerCommand = new RegisterCommand
        {
            Email = $"user_{Guid.NewGuid()}@example.com",
            Password = "SecurePassword@123",
            FirstName = "Integration",
            LastName = "User",
            OrganizationName = "Test Org"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return result!.Tokens.AccessToken;
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateMonitorRequest
        {
            Name = "Integration HTTP Monitor",
            Description = "Checking Google",
            Type = MonitorType.Http,
            Target = "https://google.com",
            IntervalSeconds = 60,
            TimeoutSeconds = 30,
            Retries = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/monitors", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MonitorDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        result.Target.Should().Be(request.Target);
        result.IntervalSeconds.Should().Be(request.IntervalSeconds);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateMonitorRequest
        {
            Name = "Details HTTP Monitor",
            Type = MonitorType.Http,
            Target = "https://example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/monitors", request);
        var createdMonitor = await createResponse.Content.ReadFromJsonAsync<MonitorDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/monitors/{createdMonitor!.MonitorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MonitorDto>();
        result.Should().NotBeNull();
        result!.MonitorId.Should().Be(createdMonitor.MonitorId);
    }
}
