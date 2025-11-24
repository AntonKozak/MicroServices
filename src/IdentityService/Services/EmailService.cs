namespace IdentityService.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
    Task SendPasswordChangedNotificationAsync(string email);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // TODO: Implement actual email sending (e.g., SendGrid, SMTP, etc.)
        _logger.LogInformation("Email confirmation link for {Email}: {Link}", email, confirmationLink);

        // For development, just log the link
        Console.WriteLine($"\n========================================");
        Console.WriteLine($"Email Confirmation Link for {email}:");
        Console.WriteLine(confirmationLink);
        Console.WriteLine($"========================================\n");

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string resetLink)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("Password reset link for {Email}: {Link}", email, resetLink);

        // For development, just log the link
        Console.WriteLine($"\n========================================");
        Console.WriteLine($"Password Reset Link for {email}:");
        Console.WriteLine(resetLink);
        Console.WriteLine($"========================================\n");

        return Task.CompletedTask;
    }

    public Task SendPasswordChangedNotificationAsync(string email)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("Password changed notification sent to {Email}", email);

        Console.WriteLine($"\n========================================");
        Console.WriteLine($"Password Changed Notification for {email}");
        Console.WriteLine($"========================================\n");

        return Task.CompletedTask;
    }
}
