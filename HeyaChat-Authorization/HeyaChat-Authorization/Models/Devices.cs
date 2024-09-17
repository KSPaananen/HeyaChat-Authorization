namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table for storing devices associated with users. Enforce only one active device at a time.</para>
///     <para>Do not assign UsedAt with code, because it's handled by the database.</para>
/// </summary>
public partial class Devices
{
    public long DeviceId { get; set; }

    public string DeviceName { get; set; } = null!;

    public Guid? DeviceIdentifier { get; set; }

    public string CountryTag { get; set; } = null!;

    public DateTime? UsedAt { get; set; } // Automatically updated by database

    public long? UserId { get; set; }

    public virtual ICollection<AuditLogs> AuditLogs { get; set; } = new List<AuditLogs>();

    public virtual ICollection<Tokens> Tokens { get; set; } = new List<Tokens>();

    public virtual Users? User { get; set; }
}
