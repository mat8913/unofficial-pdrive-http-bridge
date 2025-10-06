using Microsoft.Extensions.Logging;

namespace unofficial_pdrive_http_bridge;

public sealed class PersistenceManager
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _dbPath;

    public PersistenceManager(ILoggerFactory loggerFactory, string dbPath)
    {
        _loggerFactory = loggerFactory;
        _dbPath = dbPath;
    }

    public ProgramDbContext GetProgramDbContext()
    {
        return new ProgramDbContext(_loggerFactory, _dbPath);
    }
}
