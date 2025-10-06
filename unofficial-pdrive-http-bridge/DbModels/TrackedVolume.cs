using System.ComponentModel.DataAnnotations;

namespace unofficial_pdrive_http_bridge.DbModels;

public sealed class TrackedVolume
{
    [Key]
    public required string VolumeId { get; set; }
    public required string LatestEventId { get; set; }
}
