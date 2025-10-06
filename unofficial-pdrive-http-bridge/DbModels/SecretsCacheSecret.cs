using Microsoft.EntityFrameworkCore;

namespace unofficial_pdrive_http_bridge.DbModels;

[PrimaryKey(
    nameof(Context_HasValue),
    nameof(Context_Name),
    nameof(Context_Id),
    nameof(ValueHolderName),
    nameof(ValueHolderId),
    nameof(ValueName)
)]
public sealed class SecretsCacheSecret
{
    public required bool Context_HasValue { get; set; }
    public required string Context_Name { get; set; }
    public required string Context_Id { get; set; }
    public required string ValueHolderName { get; set; }
    public required string ValueHolderId { get; set; }
    public required string ValueName { get; set; }
    public required byte[] SecretBytes { get; set; }
    public required byte Flags { get; set; }
}
