using System.ComponentModel.DataAnnotations;

namespace unofficial_pdrive_http_bridge.DbModels;

public sealed class Session
{
    [Key]
    public required string SessionId { get; set; }

    public required string Username { get; set; }

    public required string UserId { get; set; }

    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public required bool IsWaitingForSecondFactorCode { get; set; }

    public required int PasswordMode { get; set; }
}
