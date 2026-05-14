using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsHMI.Common.Auth;
using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Api.Controllers;

/// <summary>
/// API endpoints for querying security rights.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RightsController : ControllerBase
{
    private readonly IRightsService _rightsService;

    public RightsController(IRightsService rightsService)
    {
        _rightsService = rightsService;
    }

    /// <summary>
    /// Get all distinct rights in the system with usage statistics.
    /// </summary>
    [HttpGet]
    [RequireRight("Read:ManageUsers")]
    public async Task<ActionResult<IReadOnlyList<RightInfo>>> GetAll(CancellationToken ct)
    {
        var rights = await _rightsService.GetAllRightsAsync(ct);
        return Ok(rights);
    }
}
