using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class Device
{
    public long DeviceId { get; set; }

    public string DeviceName { get; set; } = null!;

    public Guid? DeviceIdentifier { get; set; }

    public string CountryTag { get; set; } = null!;

    public DateTime? UsedAt { get; set; } // Automatically updated by database

    public long? UserId { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public virtual User? User { get; set; }
}
