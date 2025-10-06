using Microsoft.EntityFrameworkCore;

namespace unofficial_pdrive_http_bridge.DbModels;

[PrimaryKey(
    nameof(Context_HasValue),
    nameof(Context_Name),
    nameof(Context_Id),
    nameof(ValueHolderName),
    nameof(ValueHolderId),
    nameof(ValueName),
    nameof(Secret_Context_HasValue),
    nameof(Secret_Context_Name),
    nameof(Secret_Context_Id),
    nameof(Secret_ValueHolderName),
    nameof(Secret_ValueHolderId),
    nameof(Secret_ValueName)
)]
public sealed class SecretsCacheGroup
{
    public required bool Context_HasValue { get; set; }
    public required string Context_Name { get; set; }
    public required string Context_Id { get; set; }
    public required string ValueHolderName { get; set; }
    public required string ValueHolderId { get; set; }
    public required string ValueName { get; set; }
    public required bool Secret_Context_HasValue { get; set; }
    public required string Secret_Context_Name { get; set; }
    public required string Secret_Context_Id { get; set; }
    public required string Secret_ValueHolderName { get; set; }
    public required string Secret_ValueHolderId { get; set; }
    public required string Secret_ValueName { get; set; }
}
