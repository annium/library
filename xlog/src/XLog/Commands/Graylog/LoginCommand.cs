using System;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net.Http;
using XLog.Components;

namespace XLog.Commands.Graylog;

internal class LoginCommand : AsyncCommand<LoginCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "login";
    public static string Description => "login to given graylog instance";
    public ILogger Logger { get; }
    private readonly IConfigurationManager _configurationManager;
    private readonly IHttpRequestFactory _httpRequestFactory;

    public LoginCommand(
        IConfigurationManager configurationManager,
        IHttpRequestFactory httpRequestFactory,
        ILogger logger
    )
    {
        _configurationManager = configurationManager;
        _httpRequestFactory = httpRequestFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(LoginCommandConfiguration cfg, CancellationToken ct)
    {
        cfg.Server.EnsureAbsolute();
        var sessionId = await LogInAsync(cfg);
        if (sessionId is null)
            return;

        _configurationManager.GraylogLogin(cfg.Name, cfg.Server.ToString(), cfg.Login, cfg.Password);
        this.Info("Login succeed");
    }

    private async Task<string?> LogInAsync(LoginCommandConfiguration cfg)
    {
        var response = await _httpRequestFactory
            .New(cfg.Server)
            .Post("api/system/sessions")
            .Header("X-Requested-By", "cli")
            .JsonContent(
                new
                {
                    host = cfg.Server,
                    username = cfg.Login,
                    password = cfg.Password
                }
            )
            .AsResponseAsync(new { session_id = string.Empty });
        var sessionId = response.Data.session_id;
        if (!sessionId.IsNullOrWhiteSpace())
            return sessionId;

        this.Error("Login failed: {response}", response.Content.ReadAsStringAsync());
        return null;
    }
}

public class LoginCommandConfiguration
{
    [Position(1, isRequired: true)]
    [Help("Name")]
    public string Name { get; set; } = string.Empty;

    [Position(2, isRequired: true)]
    [Help("Server address")]
    public Uri Server { get; set; } = default!;

    [Position(3, isRequired: true)]
    [Help("Login")]
    public string Login { get; set; } = string.Empty;

    [Position(4, isRequired: true)]
    [Help("Password")]
    public string Password { get; set; } = string.Empty;
}
