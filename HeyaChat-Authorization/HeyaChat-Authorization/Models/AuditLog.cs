namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table which stores logs of user actions such as email changes, new logins etc...</para>
///     <para>Do not assign PerformedAt with code, because it's handled by the database.</para>
/// </summary>
public partial class AuditLog
{
    public long LogId { get; set; }

    public string? PerformedAction { get; set; }

    public DateTime? PerformedAt { get; set; }

    public long? DeviceId { get; set; }

    public virtual Device? Device { get; set; }
}
