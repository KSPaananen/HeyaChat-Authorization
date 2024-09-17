namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table for storing account deletion requests. Accounts are automatically deleted after set amount of days.</para>
///     <para>Do not assign DateRequested with code, because it's handled by the database.</para>
/// </summary>
public partial class DeleteRequests
{
    public long DeleteId { get; set; }

    public long UserId { get; set; }

    public DateTime? DateRequested { get; set; }

    public bool? Fulfilled { get; set; }

    public virtual Users User { get; set; } = null!;
}
