#nullable disable

namespace AspNetIdentity.Models;

public partial class AspNetSchemaVersion
{
    public string Feature { get; set; }

    public string CompatibleSchemaVersion { get; set; }

    public bool IsCurrentVersion { get; set; }
}