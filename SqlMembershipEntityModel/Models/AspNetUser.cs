#nullable disable

using System;
using System.Collections.Generic;

namespace AspNetIdentity.Models;

public partial class AspNetUser
{
    public Guid ApplicationId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; }

    public string LoweredUserName { get; set; }

    public string MobileAlias { get; set; }

    public bool IsAnonymous { get; set; }

    public DateTime LastActivityDate { get; set; }

    public virtual AspNetApplication Application { get; set; }

    public virtual AspNetMembership AspNetMembership { get; set; }

    public virtual AspNetProfile AspNetProfile { get; set; }

    public virtual ICollection<AspNetUsersInRole> AspNetUsersInRoles { get; set; } = new List<AspNetUsersInRole>();
}