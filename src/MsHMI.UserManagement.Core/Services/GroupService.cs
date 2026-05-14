using Microsoft.Extensions.Logging;
using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Core.Services;

/// <summary>
/// Service for managing groups and their rights.
/// Groups are stored with a "*" prefix in the database (e.g., "*OPERATORS").
/// </summary>
public class GroupService : IGroupService
{
    private readonly IUserRightRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GroupService> _logger;

    private const string GroupPrefix = "*";
    private const string DefaultRight = "Read:MMI";

    public GroupService(
        IUserRightRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<GroupService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private static string ToDbName(string groupName) =>
        groupName.StartsWith(GroupPrefix) ? groupName.ToUpperInvariant() : $"{GroupPrefix}{groupName.ToUpperInvariant()}";

    private static string ToDisplayName(string dbName) =>
        dbName.StartsWith(GroupPrefix) ? dbName[1..] : dbName;

    public async Task<IReadOnlyList<GroupSummary>> GetAllGroupsAsync(CancellationToken ct = default)
    {
        var groupNames = await _repository.GetAllGroupNamesAsync(ct);
        var summaries = new List<GroupSummary>();

        foreach (var groupName in groupNames)
        {
            var rights = await _repository.GetRightsForGroupAsync(groupName, ct);
            summaries.Add(new GroupSummary(groupName, rights.Count));
        }

        return summaries;
    }

    public async Task<GroupDetail?> GetGroupAsync(string groupName, CancellationToken ct = default)
    {
        var dbName = ToDbName(groupName);
        
        if (!await _repository.ExistsAsync(dbName, ct))
            return null;

        var displayName = ToDisplayName(dbName);
        var rights = await _repository.GetRightsForGroupAsync(displayName, ct);
        var availableRights = await _repository.GetAvailableRightsForGroupAsync(displayName, ct);

        return new GroupDetail(displayName, rights, availableRights);
    }

    public async Task<bool> CreateGroupAsync(string groupName, IEnumerable<string>? rights = null, CancellationToken ct = default)
    {
        var dbName = ToDbName(groupName);

        if (await _repository.ExistsAsync(dbName, ct))
        {
            _logger.LogWarning("Attempted to create group {GroupName} but it already exists", groupName);
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
            await _repository.AddRightsAsync(dbName, rightsToAdd, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Created group {GroupName} with {RightCount} rights", groupName, rightsToAdd.Count);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to create group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> UpdateGroupRightsAsync(string groupName, IEnumerable<string> rights, CancellationToken ct = default)
    {
        var dbName = ToDbName(groupName);
        var rightsList = rights.ToList();

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            await _repository.DeleteAllRightsAsync(dbName, ct);
            
            if (rightsList.Count > 0)
            {
                await _repository.AddRightsAsync(dbName, rightsList, ct);
            }

            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Updated rights for group {GroupName}: {RightCount} rights", groupName, rightsList.Count);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to update rights for group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> RenameGroupAsync(string oldName, string newName, CancellationToken ct = default)
    {
        var oldDbName = ToDbName(oldName);
        var newDbName = ToDbName(newName);

        if (await _repository.ExistsAsync(newDbName, ct))
        {
            _logger.LogWarning("Cannot rename group {OldName} to {NewName}: target already exists", oldName, newName);
            return false;
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            await _repository.RenameAsync(oldDbName, newDbName, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Renamed group {OldName} to {NewName}", oldName, newName);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to rename group {OldName} to {NewName}", oldName, newName);
            return false;
        }
    }

    public async Task<bool> DeleteGroupAsync(string groupName, CancellationToken ct = default)
    {
        var dbName = ToDbName(groupName);

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            await _repository.DeleteAllRightsAsync(dbName, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Deleted group {GroupName}", groupName);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to delete group {GroupName}", groupName);
            return false;
        }
    }
}
