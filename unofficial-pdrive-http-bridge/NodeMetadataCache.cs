using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace unofficial_pdrive_http_bridge;

public sealed class NodeMetadataCache(ILogger<NodeMetadataCache> logger, PersistenceManager persistenceManager)
{
    private readonly ILogger<NodeMetadataCache> _logger = logger;
    private readonly PersistenceManager _persistenceManager = persistenceManager;

    public void OnNodeUpdate(string eventId, DbModels.NodeMetadata nodeMetadata)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        var tracked = db.TrackedFolders.Any(x =>
            x.VolumeId == nodeMetadata.VolumeId &&
            x.NodeId == nodeMetadata.ParentNodeId);

        // always track root node
        if (!tracked && string.IsNullOrEmpty(nodeMetadata.ParentNodeId))
        {
            db.TrackedVolumes.Upsert(new DbModels.TrackedVolume
            {
                VolumeId = nodeMetadata.VolumeId,
                LatestEventId = eventId,
            }).Run();
            db.TrackedFolders.Upsert(new DbModels.TrackedFolder
            {
                VolumeId = nodeMetadata.VolumeId,
                NodeId = nodeMetadata.ParentNodeId,
            }).Run();
            tracked = true;
        }

        db.TrackedVolumes
            .Where(x => x.VolumeId == nodeMetadata.VolumeId)
            .ExecuteUpdate(s => s.SetProperty(x => x.LatestEventId, eventId));

        if (tracked)
        {
            db.NodeMetadata.Upsert(nodeMetadata).Run();
        }
        else
        {
            db.NodeMetadata
                .Where(x =>
                    x.VolumeId == nodeMetadata.VolumeId &&
                    x.NodeId == nodeMetadata.NodeId)
                .ExecuteDelete();
        }

        db.SaveChanges();
        transaction.Commit();
    }

    public void OnNodeDelete(string eventId, string volumeId, string nodeId)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        db.TrackedVolumes
            .Where(x => x.VolumeId == volumeId)
            .ExecuteUpdate(s => s.SetProperty(x => x.LatestEventId, eventId));

        db.NodeMetadata
            .Where(x =>
                x.VolumeId == volumeId &&
                x.NodeId == nodeId)
            .ExecuteDelete();

        db.TrackedFolders
            .Where(x =>
                x.VolumeId == volumeId &&
                x.NodeId == nodeId)
            .ExecuteDelete();

        db.SaveChanges();
        transaction.Commit();
    }

    public async Task<List<DbModels.NodeMetadata>?> TryGetChildrenAsync(string volumeId, string nodeId, CancellationToken ct)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var trackedFolder = await db.TrackedFolders
            .AsNoTracking()
            .Where(x =>
                x.VolumeId == volumeId &&
                x.NodeId == nodeId)
            .SingleOrDefaultAsync(ct);

        if (trackedFolder is null)
        {
            _logger.LogInformation("TryGetChildren cache miss {volumeId} {nodeId}", volumeId, nodeId);
            return null;
        }

        return await db.NodeMetadata
            .Where(x =>
                x.VolumeId == volumeId &&
                x.ParentNodeId == nodeId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<List<DbModels.TrackedVolume>> GetVolumesAsync(CancellationToken ct)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        return await db.TrackedVolumes
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<DbModels.NodeMetadata?> TryGetNodeMetadataAsync(string volumeId, string nodeId, CancellationToken ct)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var nodeMetadata = await db.NodeMetadata
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.VolumeId == volumeId && x.NodeId == nodeId, ct);

        if (nodeMetadata is null)
            _logger.LogInformation("TryGetNodeMetadata cache miss {volumeId} {nodeId}", volumeId, nodeId);

        return nodeMetadata;
    }

    public async Task SetChildrenAsync(string eventId, string volumeId, string nodeId, IReadOnlyList<DbModels.NodeMetadata> children, CancellationToken ct)
    {
        foreach (var child in children)
        {
            if (child.VolumeId != volumeId)
            {
                throw new InvalidOperationException($"Wrong volume id. Got {child.VolumeId}. Expected {volumeId}");
            }
            if (child.ParentNodeId != nodeId)
            {
                throw new InvalidOperationException($"Wrong parent node id. Got {child.ParentNodeId}. Expected {nodeId}");
            }
        }

        using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        await db.TrackedVolumes.Upsert(new() { VolumeId = volumeId, LatestEventId = eventId }).RunAsync(ct);

        await db.TrackedFolders.Upsert(new() { VolumeId = volumeId, NodeId = nodeId }).RunAsync(ct);

        await db.NodeMetadata.Where(x => x.VolumeId == volumeId && x.ParentNodeId == nodeId).ExecuteDeleteAsync(ct);

        await db.NodeMetadata.AddRangeAsync(children, ct);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task ResetAsync(CancellationToken ct)
    {
        _logger.LogWarning("Resetting cache");

        using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        await db.NodeMetadata.ExecuteDeleteAsync(ct);
        await db.TrackedFolders.ExecuteDeleteAsync(ct);
        await db.TrackedVolumes.ExecuteDeleteAsync(ct);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
