namespace MsHMI.UserManagement.Core.Entities;

/// <summary>
/// Represents an active editing session in the EDITOR table.
/// Used for record locking to prevent concurrent edits.
/// </summary>
/// <remarks>
/// Note: In the web application, we may use optimistic concurrency instead of
/// this table-based locking. This entity is provided for legacy compatibility.
/// </remarks>
public class Editor
{
    /// <summary>
    /// Unique identifier for the editing session.
    /// </summary>
    public int EditorId { get; set; }

    /// <summary>
    /// The screen/form being edited (e.g., "ManageUsers").
    /// </summary>
    public string Screen { get; set; } = string.Empty;

    /// <summary>
    /// The key/identifier of the record being edited.
    /// </summary>
    public string KeyValue { get; set; } = string.Empty;

    /// <summary>
    /// The username of the person editing.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The computer/station name where the edit is happening.
    /// </summary>
    public string? Station { get; set; }

    /// <summary>
    /// When the editing session started.
    /// Stored as string in legacy DB (VARCHAR2).
    /// </summary>
    public string? OpenTime { get; set; }

    /// <summary>
    /// Gets OpenTime as a DateTime if parseable.
    /// </summary>
    public DateTime? OpenTimeAsDateTime => 
        DateTime.TryParse(OpenTime, out var dt) ? dt : null;
}
