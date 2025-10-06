using System;
using System.Threading.Tasks;
using Proton.Sdk.Drive;

namespace unofficial_pdrive_http_bridge;

public sealed class VolumeEventHandler
{
    private readonly VolumeEventChannel _channel;
    private readonly NodeMetadataCache _cache;

    public VolumeEventHandler(VolumeEventChannel channel, NodeMetadataCache cache)
    {
        if (!channel.BaselineEventId.HasValue)
        {
            throw new ArgumentNullException("BaselineEventId");
        }

        _channel = channel;
        _cache = cache;

        _channel.NodeCreated += Wrap1<INode>(OnNodeChanged);
        _channel.NodeMetadataChanged += Wrap1<INode>(OnNodeChanged);
        _channel.FileContentsChanged += Wrap1<FileNode>(OnNodeChanged);
        _channel.NodeDeleted += Wrap2<VolumeId, LinkId>(OnNodeDeleted);
    }

    public string VolumeId => _channel.VolumeId.Value;

    public string EventId => _channel.BaselineEventId!.Value.Value;

    public void Start()
    {
        _channel.Start();
    }

    public async Task StopAsync()
    {
        await _channel.StopAsync();
    }

    private void OnNodeChanged(VolumeEventId eventId, INode node)
    {
        if (VolumeId != node.NodeIdentity.VolumeId.Value)
        {
            throw new InvalidOperationException($"Wrong volume ID. Expected: {VolumeId}. Got: {node.NodeIdentity.VolumeId.Value}.");
        }

        if (node.State != NodeState.Active)
        {
            OnNodeDeleted(eventId, node.NodeIdentity.VolumeId, node.NodeIdentity.NodeId);
            return;
        }

        var nodeMetadata = Converters.ProtonNodeToDbModel(node);

        _cache.OnNodeUpdate(eventId.Value, nodeMetadata);
    }

    private void OnNodeDeleted(VolumeEventId eventId, VolumeId volumeId, LinkId nodeId)
    {
        if (VolumeId != volumeId.Value)
        {
            throw new InvalidOperationException($"Wrong volume ID. Expected: {VolumeId}. Got: {volumeId.Value}.");
        }

        _cache.OnNodeDelete(eventId.Value, volumeId.Value, nodeId.Value);
    }

    private Action<VolumeEventId, T> Wrap1<T>(Action<VolumeEventId, T> f)
    {
        return (eventId, x) =>
        {
            try
            {
                f(eventId, x);
            }
            catch (Exception ex)
            {
                Environment.FailFast($"Error handling VolumeEvent {eventId.Value}", ex);
            }
        };
    }

    private Action<VolumeEventId, T, U> Wrap2<T, U>(Action<VolumeEventId, T, U> f)
    {
        return (eventId, x, y) =>
        {
            try
            {
                f(eventId, x, y);
            }
            catch (Exception ex)
            {
                Environment.FailFast($"Error handling VolumeEvent {eventId.Value}", ex);
            }
        };
    }
}
