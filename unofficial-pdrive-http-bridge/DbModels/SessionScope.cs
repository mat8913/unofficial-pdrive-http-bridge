using System.ComponentModel.DataAnnotations;

namespace unofficial_pdrive_http_bridge.DbModels;

public sealed class SessionScope
{
    [Key]
    public required string Scope { get; set; }
}
