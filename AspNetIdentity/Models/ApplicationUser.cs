#nullable disable

using Microsoft.AspNetCore.Identity;
using System;
using System.Data.SqlTypes;
using System.Security.Principal;

namespace AspNetIdentity.Models;

public class ApplicationUser : IdentityUser<Guid>, IIdentity
{
    public ApplicationUser()
    {
        CreateDate = DateTime.UtcNow;
        LastLoginDate = SqlDateTime.MinValue.Value;
        LastPasswordChangedDate = SqlDateTime.MinValue.Value;
        LastLockoutDate = SqlDateTime.MinValue.Value;
        LastActivityDate = DateTime.UtcNow;
    }

    public ApplicationUser(string userName) : this()
    {
        UserName = userName;
    }

    // All fields relate to the aspnet_Membership table, except
    // LastActivityDate, which relates to the aspnet_Users table.

    public Guid ApplicationId { get; set; }

    public override string UserName
    {
        get => base.UserName;
        set
        {
            NormalizedUserName = value?.ToLower();
            base.UserName = value;
        }
    }

    private string _NormalizedUserName = null;

    public override string NormalizedUserName
    {
        get => _NormalizedUserName ?? UserName?.ToLowerInvariant();
        set => _NormalizedUserName = value?.ToLowerInvariant();
    }

    public override string Email
    {
        get => base.Email;
        set
        {
            NormalizedEmail = value?.ToLower();
            base.Email = value;
        }
    }

    private string _NormalizedEmail = null;

    public override string NormalizedEmail
    {
        get => _NormalizedEmail ?? Email?.ToLowerInvariant();
        set => _NormalizedEmail = value?.ToLowerInvariant();
    }

    public int PasswordFormat { get; set; } = 1;
    public bool IsApproved { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    public DateTime LastPasswordChangedDate { get; set; }
    public DateTime LastLockoutDate { get; set; }
    public DateTime LastActivityDate { get; set; }
    public string Comment { get; set; }
    public bool MustChangePassword { get; set; }

    public string AuthenticationType => "Noctua";

    public bool IsAuthenticated { get; set; } = false;

    public string Name => UserName;
}