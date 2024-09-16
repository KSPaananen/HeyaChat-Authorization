using System;
using System.Collections.Generic;

namespace HeyaChat_Authorization.Models;

public partial class DeleteRequest
{
    public long DeleteId { get; set; }

    public long UserId { get; set; }

    public DateTime? DateRequested { get; set; }

    public bool? Fulfilled { get; set; }

    public virtual User User { get; set; } = null!;
}
