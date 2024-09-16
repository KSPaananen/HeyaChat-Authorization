using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class MfaCode
{
    public long CodeId { get; set; }

    public string Code { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool? Used { get; set; }

    public long? UserId { get; set; }

    public virtual User? User { get; set; }
}
