using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Proton.Sdk;
using Proton.Sdk.Drive;

namespace unofficial_pdrive_http_bridge;

public sealed class ProtonSession(
    ProtonApiSession protonApiSession,
    ProtonDriveClient protonDriveClient,
    NodeMetadataCacher nodeMetadataCacher)
{
    private NodeIdentity? _rootNodeIdentity;

    public ProtonApiSession ProtonApiSession { get; } = protonApiSession;
    public ProtonDriveClient ProtonDriveClient { get; } = protonDriveClient;
    public NodeMetadataCacher NodeMetadataCacher { get; } = nodeMetadataCacher;
    public NodeIdentity RootNodeIdentity => _rootNodeIdentity! ?? throw new InvalidOperationException("not initialized");

    public async Task StartAsync(CancellationToken ct)
    {
        await NodeMetadataCacher.StartAsync(ct);

        var volumes = await ProtonDriveClient.GetVolumesAsync(ct);
        var rootVolume = volumes[0];
        var rootShareId = rootVolume.RootShareId;
        var rootShare = await ProtonDriveClient.GetShareAsync(rootShareId, ct);
        _rootNodeIdentity = new NodeIdentity(rootShareId, rootVolume.Id, rootShare.RootNodeId);
    }

    public async Task<DbModels.NodeMetadata?> GetNodeMetadataByPathAsync(IEnumerable<string> path, NodeIdentity? start, CancellationToken ct)
    {
        start ??= RootNodeIdentity;

        var node = await NodeMetadataCacher.GetNodeMetadataAsync(
            start.VolumeId.Value,
            start.NodeId.Value,
            start.ShareId.Value,
            ct);

        foreach (var pathElem in path)
        {
            var children = await NodeMetadataCacher.GetChildrenAsync(node.VolumeId, node.NodeId, start.ShareId.Value, ct);
            node = children.SingleOrDefault(x => x.Name == pathElem);
            if (node is null)
            {
                return null;
            }
        }

        return node;
    }
}
