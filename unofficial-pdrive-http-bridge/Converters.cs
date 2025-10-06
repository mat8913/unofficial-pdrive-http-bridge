using System;
using Proton.Sdk.Drive;

namespace unofficial_pdrive_http_bridge;

public static class Converters
{
    public static DbModels.NodeMetadata ProtonNodeToDbModel(INode node)
    {
        // Note: Assumes State=Active.

        var fileNode = node as FileNode;

        return new DbModels.NodeMetadata
        {
            VolumeId = node.NodeIdentity.VolumeId.Value,
            NodeId = node.NodeIdentity.NodeId.Value,
            Name = node.Name,
            ParentNodeId = node.ParentId?.Value ?? string.Empty,
            IsFile = fileNode is not null,
            MediaType = fileNode?.MediaType,
            ActiveRevisionId = fileNode?.ActiveRevision.RevisionId.Value,
            Size = fileNode?.ActiveRevision.Size,
            ModificationTime = node.ModificationTime,
        };
    }

    public static HttpModels.NodeMetadata DbModelNodeMetadataToHttpModel(DbModels.NodeMetadata node)
    {
        var metadata = new HttpModels.NodeMetadata
        {
            Name = node.Name,
            Size = node.Size,
            Url = Uri.EscapeDataString(node.Name),
            LastModified = DateTimeOffset.FromUnixTimeSeconds(node.ModificationTime).UtcDateTime,
        };

        if (node.IsFile)
        {
            metadata.Type = HttpModels.NodeType.File;
        }
        else
        {
            metadata.Type = HttpModels.NodeType.Folder;
            metadata.Name += '/';
            metadata.Url += '/';
        }

        return metadata;
    }
}
