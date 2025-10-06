using Microsoft.EntityFrameworkCore;

namespace unofficial_pdrive_http_bridge.DbModels;

[PrimaryKey(
    nameof(VolumeId),
    nameof(NodeId)
)]
public sealed class NodeMetadata
{
    public required string VolumeId { get; set; }
    public required string NodeId { get; set; }
    public required string Name { get; set; }
    public required string ParentNodeId { get; set; }
    public required bool IsFile { get; set; }
    public string? MediaType { get; set; }
    public string? ActiveRevisionId { get; set; }
    public long? Size { get; set; }
    public required long ModificationTime { get; set; }
}
