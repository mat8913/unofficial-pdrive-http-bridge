using System;

namespace unofficial_pdrive_http_bridge.HttpModels;

public sealed class NodeMetadata
{
    public string? Name { get; set; }

    public long? Size { get; set; }

    public NodeType Type { get; set; }

    public string? Url { get; set; }

    public DateTime? LastModified { get; set; }
}
