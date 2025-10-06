using Microsoft.EntityFrameworkCore;

namespace unofficial_pdrive_http_bridge.DbModels;

[PrimaryKey(
    nameof(VolumeId),
    nameof(NodeId)
)]
public sealed class TrackedFolder
{
    public required string VolumeId { get; set; }
    public required string NodeId { get; set; }
}
