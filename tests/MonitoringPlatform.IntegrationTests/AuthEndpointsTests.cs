using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MonitoringPlatform.Application.Features.Authentication.Commands;
using Xunit;

namespace MonitoringPlatform.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Test@12345",
            FirstName = "Test",
            LastName = "User"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(command),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.Tokens.Should().NotBeNull();
        result.Tokens.AccessToken.Should().NotBeNullOrEmpty();
        result.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // First register a user
        var registerCommand = new RegisterCommand
        {
            Email = $"login_{Guid.NewGuid()}@example.com",
            Password = "Test@12345",
            FirstName = "Login",
            LastName = "Test"
        };

        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerCommand),
            Encoding.UTF8,
            "application/json");

        await _client.PostAsync("/api/v1/auth/register", registerContent);

        // Now try to login
        var loginCommand = new LoginCommand
        {
            Email = registerCommand.Email,
            Password = registerCommand.Password
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginCommand),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(loginCommand.Email);
        result.Tokens.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(command),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}