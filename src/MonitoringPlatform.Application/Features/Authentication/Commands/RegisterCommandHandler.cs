using MediatR;
using Microsoft.Extensions.Logging;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Authentication.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IJwtService jwtService,
        IPasswordHashingService passwordHashingService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _jwtService = jwtService;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<RegisterResponse>.Failure("Người dùng đã tồn tại.");
        }

        // Create or get organization
        var organization = await GetOrCreateOrganizationAsync(request);

        // Create user
        var user = new User
        {
            UserId = Guid.NewGuid(),
            OrganizationId = organization.OrganizationId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHashingService.HashPassword(request.Password),
            Role = UserRole.Owner,
            Status = UserStatus.Active,
            EmailVerifiedAt = null
        };

        await _userRepository.CreateAsync(user);

        // Generate tokens
        var tokens = await _jwtService.GenerateTokensAsync(user);

        // Store refresh token
        await _userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = _passwordHashingService.HashToken(tokens.RefreshToken),
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            RemoteIp = request.RemoteIp,
            UserAgent = request.UserAgent
        });

        _logger.LogInformation("User {UserId} registered successfully", user.UserId);

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationId = organization.OrganizationId,
            OrganizationName = organization.Name,
            Role = user.Role,
            Tokens = tokens
        });
    }

    private async Task<Organization> GetOrCreateOrganizationAsync(RegisterCommand request)
    {
        var orgName = request.OrganizationName ?? $"{request.FirstName}'s Organization";
        var orgSlug = GenerateSlug(orgName);

        var organization = new Organization
        {
            OrganizationId = Guid.NewGuid(),
            Name = orgName,
            Slug = orgSlug,
            Status = OrganizationStatus.Active,
            PlanType = PlanType.Starter,
            MaxMonitors = 10,
            MaxAlerts = 3
        };

        await _organizationRepository.CreateAsync(organization);
        return organization;
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = slug.Substring(0, Math.Min(slug.Length, 50));
        return $"{slug}-{Guid.NewGuid():N}".Substring(0, Math.Min(slug.Length + 8, 100));
    }
}
