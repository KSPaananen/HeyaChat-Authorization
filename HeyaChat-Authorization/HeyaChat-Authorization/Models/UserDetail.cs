namespace HeyaChat_Authorization.Models;

/// <summary>
///     <para>Table for storing additional details of users. UserId is auto incrementing, instead its direct connection to users table userId.</para>
///     <para>Do not assign CreatedAt and UpdatedAt with code, because these are handled by the database.</para>
/// </summary>
public partial class UserDetail
{
    public long DetailId { get; set; }

    public bool? EmailVerified { get; set; }

    public bool? PhoneVerified { get; set; }

    public bool? MfaEnabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long UserId { get; set; }

    public virtual User? User { get; set; }
}
