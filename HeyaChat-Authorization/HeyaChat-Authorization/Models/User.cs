using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Username { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public byte[]? BiometricsKey { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual ICollection<DeleteRequest> DeleteRequests { get; set; } = new List<DeleteRequest>();

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<MfaCode> MfaCodes { get; set; } = new List<MfaCode>();

    public virtual ICollection<Suspension> Suspensions { get; set; } = new List<Suspension>();
}
