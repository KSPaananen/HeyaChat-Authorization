using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class UserDetail
{
    public long DetailId { get; set; }

    public bool EmailVerified { get; set; }

    public bool PhoneVerified { get; set; }

    public bool MfaEnabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UserId { get; set; }
}
