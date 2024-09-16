using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class UserDetail
{
    public long? UserId { get; set; }

    public bool? EmailVerified { get; set; }

    public bool? PhoneVerified { get; set; }

    public bool? MfaEnabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
