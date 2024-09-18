namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table for storing JTI's of tokens.</para>
///     <para>ExpiresAt can be assigned through code. Read duration from appsettings.</para>
/// </summary>
public partial class Token
{
    public long TokenId { get; set; }

    public Guid Identifier { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool? Active { get; set; }

    public long? DeviceId { get; set; }

    public virtual Device? Device { get; set; }
}
