using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

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
