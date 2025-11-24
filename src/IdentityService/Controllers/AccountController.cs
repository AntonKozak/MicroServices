using System.Web;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        IEmailService emailService,
        ILogger<AccountController> logger,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            return BadRequest(new { error = "Email already exists" });
        }

        if (await _userManager.FindByNameAsync(registerDto.UserName) != null)
        {
            return BadRequest(new { error = "Username already exists" });
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            EmailConfirmed = _environment.IsDevelopment() // Auto-confirm in development
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // Add user to default role
        await _userManager.AddToRoleAsync(user, "User");

        // In production, send email confirmation
        if (!_environment.IsDevelopment())
        {
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(emailToken);
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/account/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

            _logger.LogInformation("User {UserName} registered successfully", user.UserName);

            return Ok(new
            {
                message = "Registration successful. Please check your email to confirm your account.",
                userId = user.Id
            });
        }

        // In development, auto-login
        var token = await _tokenService.CreateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserName} registered successfully (Development mode - auto-confirmed)", user.UserName);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserName = user.UserName!,
            Email = user.Email ?? string.Empty,
            ExpiresAt = _tokenService.GetTokenExpiration()
        });
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest(new { error = "Invalid email confirmation request" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        _logger.LogInformation("Email confirmed for user {UserName}", user.UserName);

        return Ok(new { message = "Email confirmed successfully. You can now login." });
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<ActionResult> ResendConfirmationEmail([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return Ok(new { message = "If the email exists, a confirmation link has been sent." });
        }

        if (user.EmailConfirmed)
        {
            return BadRequest(new { error = "Email already confirmed" });
        }

        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(emailToken);
        var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/account/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);

        return Ok(new { message = "If the email exists, a confirmation link has been sent." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            return Unauthorized(new { error = "Invalid username or password" });
        }

        if (!user.EmailConfirmed)
        {
            return Unauthorized(new { error = "Email not confirmed. Please check your email." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Unauthorized(new { error = "Account locked due to multiple failed login attempts. Try again later." });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new { error = "Invalid username or password" });
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var token = await _tokenService.CreateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserName} logged in successfully", user.UserName);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email ?? string.Empty,
            ExpiresAt = _tokenService.GetTokenExpiration()
        });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.Token);
        if (principal == null)
        {
            return BadRequest(new { error = "Invalid access token" });
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest(new { error = "Invalid refresh token" });
        }

        var newAccessToken = await _tokenService.CreateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            Token = newAccessToken,
            UserName = user.UserName!,
            Email = user.Email ?? string.Empty,
            ExpiresAt = _tokenService.GetTokenExpiration()
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult> GetCurrentUser()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.FirstName,
            user.LastName,
            user.EmailConfirmed,
            user.CreatedAt,
            user.LastLoginAt,
            Roles = roles
        });
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<ActionResult> UpdateProfile(UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
        if (user == null)
        {
            return NotFound();
        }

        var emailChanged = false;

        if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
        {
            if (await _userManager.FindByEmailAsync(updateUserDto.Email) != null)
            {
                return BadRequest(new { error = "Email already in use" });
            }
            user.Email = updateUserDto.Email;
            user.EmailConfirmed = false; // Require email confirmation again
            emailChanged = true;
        }

        if (!string.IsNullOrEmpty(updateUserDto.FirstName))
            user.FirstName = updateUserDto.FirstName;

        if (!string.IsNullOrEmpty(updateUserDto.LastName))
            user.LastName = updateUserDto.LastName;

        if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
            user.PhoneNumber = updateUserDto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        if (emailChanged)
        {
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(emailToken);
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/account/confirm-email?userId={user.Id}&token={encodedToken}";
            await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
        }

        _logger.LogInformation("User {UserName} updated profile", user.UserName);

        return Ok(new { message = "Profile updated successfully", emailChanged });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user,
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        await _emailService.SendPasswordChangedNotificationAsync(user.Email!);

        _logger.LogInformation("User {UserName} changed password", user.UserName);

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

        // Always return success to prevent email enumeration
        if (user == null || !user.EmailConfirmed)
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(resetToken);
        var resetLink = $"{Request.Scheme}://{Request.Host}/api/account/reset-password?email={user.Email}&token={encodedToken}";

        await _emailService.SendPasswordResetAsync(user.Email!, resetLink);

        _logger.LogInformation("Password reset requested for {Email}", forgotPasswordDto.Email);

        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return BadRequest(new { error = "Invalid password reset request" });
        }

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        await _emailService.SendPasswordChangedNotificationAsync(user.Email!);

        _logger.LogInformation("Password reset successful for {Email}", resetPasswordDto.Email);

        return Ok(new { message = "Password reset successfully. You can now login with your new password." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
        }

        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<ActionResult> DeleteAccount()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        _logger.LogInformation("User {UserName} deleted their account", user.UserName);

        return Ok(new { message = "Account deleted successfully" });
    }
}
