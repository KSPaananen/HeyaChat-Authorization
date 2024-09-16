using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class Token
{
    public long TokenId { get; set; }

    public Guid Identifier { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool? Active { get; set; }

    public long? DeviceId { get; set; }

    public virtual Device? Device { get; set; }
}
