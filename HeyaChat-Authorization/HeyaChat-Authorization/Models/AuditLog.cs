using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class AuditLog
{
    public long LogId { get; set; }

    public string? PerformedAction { get; set; }

    public DateTime? PerformedAt { get; set; }

    public long? DeviceId { get; set; }

    public virtual Device? Device { get; set; }
}
