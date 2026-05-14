using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsHMI.Common.Auth;
using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Api.Controllers;

/// <summary>
/// API endpoints for managing groups.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    /// <summary>
    /// Get all groups with summary information.
    /// </summary>
    [HttpGet]
    [RequireRight("Read:ManageUsers")]
    public async Task<ActionResult<IReadOnlyList<GroupSummary>>> GetAll(CancellationToken ct)
    {
        var groups = await _groupService.GetAllGroupsAsync(ct);
        return Ok(groups);
    }

    /// <summary>
    /// Get detailed information about a specific group.
    /// </summary>
    [HttpGet("{groupName}")]
    [RequireRight("Read:ManageUsers")]
    public async Task<ActionResult<GroupDetail>> Get(string groupName, CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupName, ct);
        if (group is null)
            return NotFound();

        return Ok(group);
    }

    /// <summary>
    /// Create a new group.
    /// </summary>
    [HttpPost]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult<GroupDetail>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Group name is required");

        var success = await _groupService.CreateGroupAsync(request.Username, request.Rights, ct);
        if (!success)
            return Conflict($"Group '{request.Username}' already exists");

        var group = await _groupService.GetGroupAsync(request.Username, ct);
        return CreatedAtAction(nameof(Get), new { groupName = request.Username }, group);
    }

    /// <summary>
    /// Update a group's rights.
    /// </summary>
    [HttpPut("{groupName}/rights")]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult> UpdateRights(string groupName, [FromBody] UpdateRightsRequest request, CancellationToken ct)
    {
        var success = await _groupService.UpdateGroupRightsAsync(groupName, request.Rights, ct);
        if (!success)
            return BadRequest("Failed to update group rights");

        return NoContent();
    }

    /// <summary>
    /// Rename a group.
    /// </summary>
    [HttpPut("{groupName}/rename")]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult> Rename(string groupName, [FromBody] RenameRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
            return BadRequest("New name is required");

        var success = await _groupService.RenameGroupAsync(groupName, request.NewName, ct);
        if (!success)
            return Conflict($"Cannot rename to '{request.NewName}' - it may already exist");

        return NoContent();
    }

    /// <summary>
    /// Delete a group.
    /// </summary>
    [HttpDelete("{groupName}")]
    [RequireRight("Write:ManageUsers")]
    public async Task<ActionResult> Delete(string groupName, CancellationToken ct)
    {
        var success = await _groupService.DeleteGroupAsync(groupName, ct);
        if (!success)
            return BadRequest("Failed to delete group");

        return NoContent();
    }
}
