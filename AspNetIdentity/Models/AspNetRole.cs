#nullable disable

using System;
using System.Collections.Generic;

namespace AspNetIdentity.Models;

public partial class AspNetRole
{
    public Guid ApplicationId { get; set; }

    public Guid RoleId { get; set; }

    public string RoleName { get; set; }

    public string LoweredRoleName { get; set; }

    public string Description { get; set; }

    public virtual AspNetApplication Application { get; set; }

    public virtual ICollection<AspNetUsersInRole> AspNetUsersInRoles { get; set; } = new List<AspNetUsersInRole>();
}