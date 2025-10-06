using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proton.Sdk.Drive;

namespace unofficial_pdrive_http_bridge;

public sealed class NodeMetadataCacher(NodeMetadataCache cache, ProtonDriveClient client)
{
    private readonly NodeMetadataCache _cache = cache;
    private readonly ProtonDriveClient _client = client;
    private readonly Dictionary<string, VolumeEventHandler> _volumeEventHandlers = new();
    private readonly SemaphoreSlim _sync = new(1, 1);
    private bool _started = false;

    public async Task StartAsync(CancellationToken ct)
    {
        await _sync.WaitAsync(ct);
        try
        {
            var volumes = await _cache.GetVolumesAsync(ct);
            foreach (var volume in volumes)
            {
                if (_volumeEventHandlers.ContainsKey(volume.VolumeId))
                    continue;

                var channel = new VolumeEventChannel(_client, new(volume.VolumeId));
                channel.BaselineEventId = new(volume.LatestEventId);
                _volumeEventHandlers[volume.VolumeId] = new VolumeEventHandler(channel, _cache);
            }

            foreach (var handler in _volumeEventHandlers.Values)
            {
                handler.Start();
            }

            _started = true;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<List<DbModels.NodeMetadata>> GetChildrenAsync(string volumeId, string nodeId, string shareId, CancellationToken ct)
    {
        if (!_started)
            throw new InvalidOperationException("NodeMetadataCacher not started");

        var children = await _cache.TryGetChildrenAsync(volumeId, nodeId, ct);
        if (children is not null)
            return children;

        await _sync.WaitAsync(ct);
        try
        {
            var handler = await EnsureVolumeEventHandlerAsync(volumeId, ct);
            await handler.StopAsync();

            try
            {
                children = await _client.GetFolderChildrenAsync(new NodeIdentity(new(shareId), new(volumeId), new(nodeId)), ct)
                    .Select(Converters.ProtonNodeToDbModel)
                    .OrderBy(x => x.Name, StringComparer.Ordinal)
                    .ToListAsync(ct);

                await _cache.SetChildrenAsync(handler.EventId, volumeId, nodeId, children, ct);

                return children;
            }
            finally
            {
                handler.Start();
            }
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<DbModels.NodeMetadata> GetNodeMetadataAsync(string volumeId, string nodeId, string shareId, CancellationToken ct)
    {
        var nodeMetadata = await _cache.TryGetNodeMetadataAsync(volumeId, nodeId, ct);
        if (nodeMetadata is not null)
            return nodeMetadata;

        var node = await _client.GetNodeAsync(new(shareId), new(nodeId), ct);
        nodeMetadata = Converters.ProtonNodeToDbModel(node);

        await _sync.WaitAsync();
        try
        {
            var handler = await EnsureVolumeEventHandlerAsync(volumeId, ct);

            await handler.StopAsync();
            try
            {
                _cache.OnNodeUpdate(handler.EventId, nodeMetadata);
            }
            finally
            {
                handler.Start();
            }
        }
        finally
        {
            _sync.Release();
        }

        return nodeMetadata;
    }

    // assumes lock is already taken
    private async Task<VolumeEventHandler> EnsureVolumeEventHandlerAsync(string volumeId, CancellationToken ct)
    {
        if (_volumeEventHandlers.TryGetValue(volumeId, out var handler))
            return handler;

        var channel = new VolumeEventChannel(_client, new(volumeId));
        channel.BaselineEventId = await channel.GetLatestEventIdAsync(ct);
        handler = new VolumeEventHandler(channel, _cache);
        _volumeEventHandlers[volumeId] = handler;
        return handler;
    }
}
