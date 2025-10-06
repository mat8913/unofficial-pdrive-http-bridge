using System.ComponentModel.DataAnnotations;

namespace unofficial_pdrive_http_bridge.DbModels;

public sealed class WebUiPassword
{
    [Key]
    public required int Id { get; set; }
    public required string Password { get; set; }
}
