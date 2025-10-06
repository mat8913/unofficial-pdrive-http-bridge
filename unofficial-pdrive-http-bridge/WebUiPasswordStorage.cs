using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace unofficial_pdrive_http_bridge;

public sealed class WebUiPasswordStorage(PersistenceManager persistenceManager)
{
    private readonly PersistenceManager _persistenceManager = persistenceManager;

    public async Task<(bool, string)> GetPasswordAsync(CancellationToken ct)
    {
        await using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var modelPassword = await db.WebUiPasswords.SingleOrDefaultAsync();
        if (modelPassword is not null)
            return (true, modelPassword.Password);
        modelPassword = new()
        {
            Id = 1,
            Password = GeneratePassword(),
        };

        await db.AddAsync(modelPassword, ct);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return (false, modelPassword.Password);
    }

    public async Task<string> ResetPasswordAsync(CancellationToken ct)
    {
        await using var db = _persistenceManager.GetProgramDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        await db.WebUiPasswords.ExecuteDeleteAsync(ct);

        var modelPassword = new DbModels.WebUiPassword
        {
            Id = 1,
            Password = GeneratePassword(),
        };
        await db.AddAsync(modelPassword, ct);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return modelPassword.Password;
    }

    private string GeneratePassword()
    {
        var charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return RandomNumberGenerator.GetString(charset, 22);
    }
}
