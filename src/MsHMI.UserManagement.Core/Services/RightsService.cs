using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Core.Services;

/// <summary>
/// Service for querying available rights.
/// </summary>
public class RightsService : IRightsService
{
    private readonly IUserRightRepository _repository;

    public RightsService(IUserRightRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RightInfo>> GetAllRightsAsync(CancellationToken ct = default)
    {
        var allRights = await _repository.GetAllDistinctRightsAsync(ct);
        var result = new List<RightInfo>();

        foreach (var right in allRights)
        {
            var (userCount, groupCount) = await _repository.GetRightStatsAsync(right, ct);
            result.Add(new RightInfo(right, userCount, groupCount));
        }

        return result.OrderBy(r => r.Name).ToList();
    }
}
