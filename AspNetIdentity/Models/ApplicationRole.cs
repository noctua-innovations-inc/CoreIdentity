#nullable disable

using Microsoft.AspNetCore.Identity;
using System;

namespace AspNetIdentity.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public Guid ApplicationId { get; set; }
    public string Description { get; set; }

    private string _NormalizedName = null;

    public override string NormalizedName
    {
        get => _NormalizedName ?? Name?.ToLowerInvariant();
        set => _NormalizedName = value?.ToLowerInvariant();
    }
}