using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Proton.Sdk;
using Proton.Sdk.Cryptography;

namespace unofficial_pdrive_http_bridge;

public sealed class DbSecretsCache : ISecretsCache
{
    private readonly PersistenceManager _persistenceManager;

    public DbSecretsCache(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager;
    }

    public void Set(CacheKey cacheKey, ReadOnlySpan<byte> secretBytes, byte flags, TimeSpan expiration)
    {
        if (expiration != Timeout.InfiniteTimeSpan)
        {
            throw new NotImplementedException("Timeout not implemented");
        }

        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        db.SecretsCacheSecrets.Upsert(new()
        {
            Context_HasValue = cacheKey.Context.HasValue,
            Context_Name = cacheKey.Context?.Name ?? "",
            Context_Id = cacheKey.Context?.Id ?? "",
            ValueHolderName = cacheKey.ValueHolderName,
            ValueHolderId = cacheKey.ValueHolderId,
            ValueName = cacheKey.ValueName,
            SecretBytes = secretBytes.ToArray(),
            Flags = flags,
        }).Run();

        db.SaveChanges();
        transaction.Commit();
    }

    public void IncludeInGroup(CacheKey groupCacheKey, ReadOnlySpan<CacheKey> memberCacheKeys)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        var context_name = groupCacheKey.Context?.Name ?? "";
        var context_id = groupCacheKey.Context?.Id ?? "";
        db.SecretsCacheGroups.Where(x =>
            x.Context_HasValue == groupCacheKey.Context.HasValue &&
            x.Context_Name == context_name &&
            x.Context_Id == context_id &&
            x.ValueHolderName == groupCacheKey.ValueHolderName &&
            x.ValueHolderId == groupCacheKey.ValueHolderId &&
            x.ValueName == groupCacheKey.ValueName
            ).ExecuteDelete();

        foreach (var cacheKey in memberCacheKeys)
        {
            db.SecretsCacheGroups.Add(new()
            {
                Context_HasValue = groupCacheKey.Context.HasValue,
                Context_Name = groupCacheKey.Context?.Name ?? "",
                Context_Id = groupCacheKey.Context?.Id ?? "",
                ValueHolderName = groupCacheKey.ValueHolderName,
                ValueHolderId = groupCacheKey.ValueHolderId,
                ValueName = groupCacheKey.ValueName,
                Secret_Context_HasValue = cacheKey.Context.HasValue,
                Secret_Context_Name = cacheKey.Context?.Name ?? "",
                Secret_Context_Id = cacheKey.Context?.Id ?? "",
                Secret_ValueHolderName = cacheKey.ValueHolderName,
                Secret_ValueHolderId = cacheKey.ValueHolderId,
                Secret_ValueName = cacheKey.ValueName,
            });
        }

        db.SaveChanges();
        transaction.Commit();
    }

    public bool TryUse<TState, TResult>(CacheKey cacheKey, TState state, SecretTransform<TState, TResult> transform, [MaybeNullWhen(false)] out TResult result) where TResult : notnull
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction();

        var context_name = cacheKey.Context?.Name ?? "";
        var context_id = cacheKey.Context?.Id ?? "";
        var modelSecret = db.SecretsCacheSecrets
            .Where(x =>
                x.Context_HasValue == cacheKey.Context.HasValue &&
                x.Context_Name == context_name &&
                x.Context_Id == context_id &&
                x.ValueHolderName == cacheKey.ValueHolderName &&
                x.ValueHolderId == cacheKey.ValueHolderId &&
                x.ValueName == cacheKey.ValueName)
            .Select(x => new { x.SecretBytes, x.Flags })
            .SingleOrDefault();

        if (modelSecret is null)
        {
            result = default;
            return false;
        }

        result = transform(state, modelSecret.SecretBytes, modelSecret.Flags);
        return true;
    }

    public bool TryUseGroup<TState, TResult>(CacheKey groupCacheKey, TState state, SecretTransform<TState, TResult> transform, [MaybeNullWhen(false)] out List<TResult> result) where TResult : notnull
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction();

        var context_name = groupCacheKey.Context?.Name ?? "";
        var context_id = groupCacheKey.Context?.Id ?? "";
        var modelSecrets = db.SecretsCacheGroups
            .Where(x =>
                x.Context_HasValue == groupCacheKey.Context.HasValue &&
                x.Context_Name == context_name &&
                x.Context_Id == context_id &&
                x.ValueHolderName == groupCacheKey.ValueHolderName &&
                x.ValueHolderId == groupCacheKey.ValueHolderId &&
                x.ValueName == groupCacheKey.ValueName)
            .Join(
                db.SecretsCacheSecrets,
                g => new
                {
                    g.Secret_Context_HasValue,
                    g.Secret_Context_Name,
                    g.Secret_Context_Id,
                    g.Secret_ValueHolderName,
                    g.Secret_ValueHolderId,
                    g.Secret_ValueName,
                },
                s => new
                {
                    Secret_Context_HasValue = s.Context_HasValue,
                    Secret_Context_Name = s.Context_Name,
                    Secret_Context_Id = s.Context_Id,
                    Secret_ValueHolderName = s.ValueHolderName,
                    Secret_ValueHolderId = s.ValueHolderId,
                    Secret_ValueName = s.ValueName,
                },
                (g, s) => new { s.SecretBytes, s.Flags })
            .ToArray();

        if (modelSecrets.Length == 0)
        {
            result = default;
            return false;
        }

        var ret = new List<TResult>(modelSecrets.Length);
        foreach (var modelSecret in modelSecrets)
        {
            ret.Add(transform(state, modelSecret.SecretBytes, modelSecret.Flags));
        }

        result = ret;
        return true;
    }

    public void Remove(CacheKey cacheKey)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        var context_name = cacheKey.Context?.Name ?? "";
        var context_id = cacheKey.Context?.Id ?? "";

        db.SecretsCacheSecrets
            .Where(x =>
                x.Context_HasValue == cacheKey.Context.HasValue &&
                x.Context_Name == context_name &&
                x.Context_Id == context_id)
            .ExecuteDelete();

        db.SecretsCacheGroups
            .Where(x =>
                x.Context_HasValue == cacheKey.Context.HasValue &&
                x.Context_Name == context_name &&
                x.Context_Id == context_id)
            .ExecuteDelete();

        db.SaveChanges();
        transaction.Commit();
    }
}
