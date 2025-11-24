using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
