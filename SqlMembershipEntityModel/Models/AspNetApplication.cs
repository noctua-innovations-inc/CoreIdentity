#nullable disable

using System;
using System.Collections.Generic;

namespace SqlMembershipEntityModel.Models;

public partial class AspNetApplication
{
    public string ApplicationName { get; set; }

    public string LoweredApplicationName { get; set; }

    public Guid ApplicationId { get; set; }

    public string Description { get; set; }

    public virtual ICollection<AspNetMembership> AspNetMemberships { get; set; } = new List<AspNetMembership>();

    public virtual ICollection<AspNetRole> AspNetRoles { get; set; } = new List<AspNetRole>();

    public virtual ICollection<AspNetUser> AspNetUsers { get; set; } = new List<AspNetUser>();
}