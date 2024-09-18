namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table for storing multifactorauth codes associated with users.</para>
///     <para>ExpiresAt can be assigned through code. Read duration from appsettings</para>
/// </summary>
public partial class Codes
{
    public long CodeId { get; set; }

    public string Code { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool? Used { get; set; }

    public long? UserId { get; set; }

    public virtual User? User { get; set; }
}
