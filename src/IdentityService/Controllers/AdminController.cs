using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet("users")]
    public ActionResult GetUsers()
    {
        var users = _userManager.Users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.FirstName,
            u.LastName,
            u.CreatedAt
        }).ToList();

        return Ok(users);
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult> GetUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
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
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            Roles = roles
        });
    }

    [HttpPost("users/{userId}/roles/{roleName}")]
    public async Task<ActionResult> AddUserToRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return BadRequest("Role does not exist");
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("User {UserId} added to role {RoleName}", userId, roleName);
        return Ok(new { message = $"User added to role {roleName}" });
    }

    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<ActionResult> RemoveUserFromRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("User {UserId} removed from role {RoleName}", userId, roleName);
        return Ok(new { message = $"User removed from role {roleName}" });
    }

    [HttpGet("roles")]
    public ActionResult GetRoles()
    {
        var roles = _roleManager.Roles.Select(r => new
        {
            r.Id,
            r.Name
        }).ToList();

        return Ok(roles);
    }

    [HttpPost("roles")]
    public async Task<ActionResult> CreateRole([FromBody] string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return BadRequest("Role name is required");
        }

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return BadRequest("Role already exists");
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("Role {RoleName} created", roleName);
        return Ok(new { message = $"Role {roleName} created successfully" });
    }
}
