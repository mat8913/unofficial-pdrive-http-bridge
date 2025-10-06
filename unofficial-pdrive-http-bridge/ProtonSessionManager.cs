using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proton.Sdk;
using Proton.Sdk.Drive;

namespace unofficial_pdrive_http_bridge;

public sealed class ProtonSessionManager(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    PersistenceManager persistenceManager,
    SessionStorage sessionStorage)
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly PersistenceManager _persistenceManager = persistenceManager;
    private readonly SessionStorage _sessionStorage = sessionStorage;
    private readonly SemaphoreSlim _sync = new(1, 1);

    public ProtonSession? ProtonSession { get; private set; }

    public async Task StartAsync(CancellationToken ct)
    {
        if (ProtonSession is not null)
            return;

        await TryResumeSessionAsync(ct);
    }

    public async Task LoginAsync(string username, string password, string otpCode, CancellationToken ct)
    {
        await _sync.WaitAsync();
        try
        {
            if (ProtonSession is not null)
                throw new InvalidOperationException("Already logged in");

            var options = GetProtonClientOptions();

            var sessionBeginRequest = new SessionBeginRequest
            {
                Username = username,
                Password = password,
                Options = options,
            };

            var session = await ProtonApiSession.BeginAsync(sessionBeginRequest, ct);

            if (session.IsWaitingForSecondFactorCode)
            {
                await session.ApplySecondFactorCodeAsync(otpCode, ct);

                await session.ApplyDataPasswordAsync(Encoding.UTF8.GetBytes(password!), ct);
            }

            var tokens = await session.TokenCredential.GetAccessTokenAsync(ct);

            var storedSession = new StoredSession
            (
                SessionId: session.SessionId.Value,
                Username: session.Username,
                UserId: session.UserId.Value,
                AccessToken: tokens.AccessToken,
                RefreshToken: tokens.RefreshToken,
                Scopes: session.Scopes.ToArray(),
                IsWaitingForSecondFactorCode: session.IsWaitingForSecondFactorCode,
                PasswordMode: (int)session.PasswordMode
            );

            await _sessionStorage.StoreSessionAsync(storedSession, ct);

            await SetSessionAsync(session, ct);
        }
        finally
        {
            _sync.Release();
        }
    }

    private async Task SetSessionAsync(ProtonApiSession apiSession, CancellationToken ct)
    {
        apiSession.TokenCredential.TokensRefreshed += (accessToken, refreshToken) =>
        {
            _sessionStorage.UpdateTokens(accessToken, refreshToken);
        };

        var client = new ProtonDriveClient(apiSession);
        var nodeMetadataCacher = ActivatorUtilities.CreateInstance<NodeMetadataCacher>(_serviceProvider, client);
        var session = new ProtonSession(apiSession, client, nodeMetadataCacher);
        await session.StartAsync(ct);
        ProtonSession = session;
    }

    private async Task TryResumeSessionAsync(CancellationToken ct)
    {
        await _sync.WaitAsync();
        try
        {
            // Shouldn't be possible
            if (ProtonSession is not null)
                return;

            var savedSessionN = await _sessionStorage.TryLoadSessionAsync(ct);

            if (savedSessionN is null)
                return;

            var savedSession = savedSessionN.Value;

            var options = GetProtonClientOptions();

            var sessionResumeRequest = new SessionResumeRequest
            {
                SessionId = new() { Value = savedSession.SessionId },
                Username = savedSession.Username,
                UserId = new() { Value = savedSession.UserId },
                AccessToken = savedSession.AccessToken,
                RefreshToken = savedSession.RefreshToken,
                IsWaitingForSecondFactorCode = savedSession.IsWaitingForSecondFactorCode,
                PasswordMode = (PasswordMode)savedSession.PasswordMode,
                Options = options,
            };
            sessionResumeRequest.Scopes.AddRange(savedSession.Scopes);

            var session = ProtonApiSession.Resume(sessionResumeRequest);

            await SetSessionAsync(session, ct);
        }
        finally
        {
            _sync.Release();
        }
    }

    private ProtonClientOptions GetProtonClientOptions()
    {
        var secretsCache = new DbSecretsCache(_persistenceManager);

        var options = new ProtonClientOptions
        {
            AppVersion = Constants.APP_NAME,
            SecretsCache = secretsCache,
            LoggerFactory = new WarnLoggerFactory(_loggerFactory)
        };

        return options;
    }
}
