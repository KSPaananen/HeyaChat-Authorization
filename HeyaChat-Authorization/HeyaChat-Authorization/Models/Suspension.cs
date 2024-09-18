namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table for storing suspensions of varying lengths.</para>
///     <para>Do not assign SuspendedAt with code, because it's handled by the database.</para>
///     <para>ExpiresAt can be assigned through code.</para>
/// </summary>
public partial class Suspension
{
    public long SuspensionId { get; set; }

    public string? Reason { get; set; }

    public DateTime? SuspendedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LiftedAt { get; set; }

    public long? UserId { get; set; }

    public virtual User? User { get; set; }
}
