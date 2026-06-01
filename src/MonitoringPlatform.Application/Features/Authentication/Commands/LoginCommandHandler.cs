using MediatR;
using Microsoft.Extensions.Logging;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Authentication.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHashingService passwordHashingService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<LoginResponse>.Failure("Email hoặc mật khẩu không hợp lệ.");
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Result<LoginResponse>.Failure("Email hoặc mật khẩu không hợp lệ.");
        }

        // Check user status
        if (user.Status != Domain.Enums.UserStatus.Active)
        {
            return Result<LoginResponse>.Failure("Tài khoản chưa được kích hoạt.");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

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

        _logger.LogInformation("User {UserId} logged in successfully", user.UserId);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization.Name,
            Role = user.Role,
            Tokens = tokens
        });
    }
}
