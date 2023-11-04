#nullable disable

using System;

namespace SqlMembershipEntityModel.Models;

public partial class AspNetProfile
{
    public Guid UserId { get; set; }

    public string PropertyNames { get; set; }

    public string PropertyValuesString { get; set; }

    public byte[] PropertyValuesBinary { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public virtual AspNetUser User { get; set; }
}