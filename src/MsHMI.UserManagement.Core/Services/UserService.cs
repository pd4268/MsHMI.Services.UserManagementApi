using Microsoft.Extensions.Logging;
using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Core.Services;

/// <summary>
/// Service for managing users and their rights.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRightRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    // Default right granted to new users (matches legacy behavior)
    private const string DefaultRight = "Read:MMI";

    public UserService(
        IUserRightRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserSummary>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var usernames = await _repository.GetAllUsernamesAsync(ct);
        var summaries = new List<UserSummary>();

        foreach (var username in usernames)
        {
            var rights = await _repository.GetRightsForUserAsync(username, ct);
            summaries.Add(new UserSummary(username, rights.Count));
        }

        return summaries;
    }

    public async Task<UserDetail?> GetUserAsync(string username, CancellationToken ct = default)
    {
        var normalizedUsername = username.ToUpperInvariant();
        
        if (!await _repository.ExistsAsync(normalizedUsername, ct))
            return null;

        var rights = await _repository.GetRightsForUserAsync(normalizedUsername, ct);
        var availableRights = await _repository.GetAvailableRightsForUserAsync(normalizedUsername, ct);

        return new UserDetail(normalizedUsername, rights, availableRights);
    }

    public async Task<bool> CreateUserAsync(string username, IEnumerable<string>? rights = null, CancellationToken ct = default)
    {
        var normalizedUsername = username.ToUpperInvariant();

        if (await _repository.ExistsAsync(normalizedUsername, ct))
        {
            _logger.LogWarning("Attempted to create user {Username} but it already exists", normalizedUsername);
            return false;
        }

        var rightsToAdd = rights?.ToList() ?? [DefaultRight];
        if (rightsToAdd.Count == 0)
        {
            rightsToAdd = [DefaultRight];
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            await _repository.AddRightsAsync(normalizedUsername, rightsToAdd, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Created user {Username} with {RightCount} rights", normalizedUsername, rightsToAdd.Count);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to create user {Username}", normalizedUsername);
            return false;
        }
    }

    public async Task<bool> UpdateUserRightsAsync(string username, IEnumerable<string> rights, CancellationToken ct = default)
    {
        var normalizedUsername = username.ToUpperInvariant();
        var rightsList = rights.ToList();

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            // Delete-then-insert pattern (matches legacy behavior, but now with a transaction!)
            await _repository.DeleteAllRightsAsync(normalizedUsername, ct);
            
            if (rightsList.Count > 0)
            {
                await _repository.AddRightsAsync(normalizedUsername, rightsList, ct);
            }

            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Updated rights for user {Username}: {RightCount} rights", normalizedUsername, rightsList.Count);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to update rights for user {Username}", normalizedUsername);
            return false;
        }
    }

    public async Task<bool> RenameUserAsync(string oldUsername, string newUsername, CancellationToken ct = default)
    {
        var normalizedOld = oldUsername.ToUpperInvariant();
        var normalizedNew = newUsername.ToUpperInvariant();

        if (await _repository.ExistsAsync(normalizedNew, ct))
        {
            _logger.LogWarning("Cannot rename {OldUsername} to {NewUsername}: target already exists", normalizedOld, normalizedNew);
            return false;
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            await _repository.RenameAsync(normalizedOld, normalizedNew, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Renamed user {OldUsername} to {NewUsername}", normalizedOld, normalizedNew);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to rename user {OldUsername} to {NewUsername}", normalizedOld, normalizedNew);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string username, CancellationToken ct = default)
    {
        var normalizedUsername = username.ToUpperInvariant();

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            await _repository.DeleteAllRightsAsync(normalizedUsername, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Deleted user {Username}", normalizedUsername);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to delete user {Username}", normalizedUsername);
            return false;
        }
    }
}
