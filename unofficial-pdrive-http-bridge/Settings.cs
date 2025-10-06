namespace unofficial_pdrive_http_bridge;

public sealed class Settings
{
    public string? Hostname { get; set; }
    public int? Port { get; set; }
    public bool ResetPassword { get; set; } = false;
    public bool ResetCache { get; set; } = false;
}
