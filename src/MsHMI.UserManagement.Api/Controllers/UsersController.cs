using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsHMI.Common.Auth;
using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Api.Controllers;

/// <summary>
/// API endpoints for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with summary information.
    /// </summary>
    [HttpGet]
    [RequireRight("Read:ManageUsers")]
    public async Task<ActionResult<IReadOnlyList<UserSummary>>> GetAll(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    /// <summary>
    /// Get detailed information about a specific user.
    /// </summary>
    [HttpGet("{username}")]
    [RequireRight("Read:ManageUsers")]
    public async Task<ActionResult<UserDetail>> Get(string username, CancellationToken ct)
    {
        var user = await _userService.GetUserAsync(username, ct);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    [HttpPost]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult<UserDetail>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required");

        var success = await _userService.CreateUserAsync(request.Username, request.Rights, ct);
        if (!success)
            return Conflict($"User '{request.Username}' already exists");

        var user = await _userService.GetUserAsync(request.Username, ct);
        return CreatedAtAction(nameof(Get), new { username = request.Username }, user);
    }

    /// <summary>
    /// Update a user's rights.
    /// </summary>
    [HttpPut("{username}/rights")]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult> UpdateRights(string username, [FromBody] UpdateRightsRequest request, CancellationToken ct)
    {
        var success = await _userService.UpdateUserRightsAsync(username, request.Rights, ct);
        if (!success)
            return BadRequest("Failed to update user rights");

        return NoContent();
    }

    /// <summary>
    /// Rename a user.
    /// </summary>
    [HttpPut("{username}/rename")]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult> Rename(string username, [FromBody] RenameRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
            return BadRequest("New name is required");

        var success = await _userService.RenameUserAsync(username, request.NewName, ct);
        if (!success)
            return Conflict($"Cannot rename to '{request.NewName}' - it may already exist");

        return NoContent();
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    [HttpDelete("{username}")]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult> Delete(string username, CancellationToken ct)
    {
        var success = await _userService.DeleteUserAsync(username, ct);
        if (!success)
            return BadRequest("Failed to delete user");

        return NoContent();
    }
}
