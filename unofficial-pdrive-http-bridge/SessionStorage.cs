using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace unofficial_pdrive_http_bridge;

public sealed class SessionStorage
{
    private readonly PersistenceManager _persistenceManager;

    public SessionStorage(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager;
    }

    public async Task StoreSessionAsync(StoredSession session, CancellationToken ct)
    {
        await using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        await db.Sessions.ExecuteDeleteAsync(ct);
        await db.SessionScopes.ExecuteDeleteAsync(ct);

        var modelSession = new DbModels.Session
        {
            SessionId = session.SessionId,
            Username = session.Username,
            UserId = session.UserId,
            AccessToken = session.AccessToken,
            RefreshToken = session.RefreshToken,
            IsWaitingForSecondFactorCode = session.IsWaitingForSecondFactorCode,
            PasswordMode = session.PasswordMode,
        };
        await db.AddAsync(modelSession, ct);

        var modelScopes = session.Scopes.Select(scope => new DbModels.SessionScope { Scope = scope });
        await db.AddRangeAsync(modelScopes, ct);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<StoredSession?> TryLoadSessionAsync(CancellationToken ct)
    {
        await using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var modelSession = await db.Sessions.SingleOrDefaultAsync(ct);
        if (modelSession is null)
            return null;

        var scopes = await db.SessionScopes.Select(x => x.Scope).ToArrayAsync(ct);

        return new StoredSession(
            SessionId: modelSession.SessionId,
            Username: modelSession.Username,
            UserId: modelSession.UserId,
            AccessToken: modelSession.AccessToken,
            RefreshToken: modelSession.RefreshToken,
            Scopes: scopes,
            IsWaitingForSecondFactorCode: modelSession.IsWaitingForSecondFactorCode,
            PasswordMode: modelSession.PasswordMode
        );
    }

    public void UpdateTokens(string accessToken, string refreshToken)
    {
        using var db = _persistenceManager.GetProgramDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

        db.Sessions.ExecuteUpdate(setters => setters
            .SetProperty(x => x.AccessToken, accessToken)
            .SetProperty(x => x.RefreshToken, refreshToken));

        db.SaveChanges();
        transaction.Commit();
    }
}

public readonly record struct StoredSession
(
    string SessionId,
    string Username,
    string UserId,
    string AccessToken,
    string RefreshToken,
    string[] Scopes,
    bool IsWaitingForSecondFactorCode,
    int PasswordMode
);
