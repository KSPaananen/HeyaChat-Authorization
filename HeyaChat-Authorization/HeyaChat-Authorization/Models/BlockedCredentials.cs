namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table which stores details of permanently suspended users.</para>
///     <para>This is to prevent circumventing suspensions by creating new accounts with banned emails.</para>
/// </summary>
public partial class BlockedCredentials
{
    public long BlockId { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }
}
