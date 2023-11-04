#nullable disable

using System;

namespace SqlMembershipEntityModel.Models;

public partial class AspNetUsersInRole
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public virtual AspNetRole Role { get; set; }

    public virtual AspNetUser User { get; set; }
}