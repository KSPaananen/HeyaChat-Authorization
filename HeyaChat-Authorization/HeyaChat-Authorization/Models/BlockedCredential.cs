using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class BlockedCredential
{
    public long BlockId { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }
}
