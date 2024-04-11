using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net.Http;
using Humanizer;
using MongoDB.Bson;
using OneOf;
using XLog.Components;

namespace XLog.Commands.Graylog;

internal class DumpCommand : AsyncCommand<DumpCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "dump";
    public static string Description => "get dump for given query";
    public ILogger Logger { get; }
    private readonly IConfigurationManager _configurationManager;
    private readonly IHttpRequestFactory _httpRequestFactory;

    public DumpCommand(
        IConfigurationManager configurationManager,
        IHttpRequestFactory httpRequestFactory,
        ILogger logger
    )
    {
        _configurationManager = configurationManager;
        _httpRequestFactory = httpRequestFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(DumpCommandConfiguration cfg, CancellationToken ct)
    {
        var credentials = _configurationManager.GraylogGetCredentials(cfg.Server);
        if (credentials is null)
        {
            this.Warn($"No credentials found for server {cfg.Server}. Login first");
            return;
        }

        var (server, login, pass) = credentials.Value;

        var sessionId = await LogInAsync(server, login, pass);
        if (sessionId is null)
            return;

        var isValid = await ValidateAsync(server, sessionId, cfg.Query);
        if (!isValid)
            return;

        var time = ResolveTime(cfg.Time);
        if (time.IsT2)
        {
            this.Error(time.AsT2);
            return;
        }

        var opts = await SearchAsync(server, sessionId, cfg.Query, time);
        if (opts is null)
            return;

        var (searchId, subId) = opts.Value;
        var jobId = await ExportAsync(server, sessionId, searchId, subId);
        if (jobId is null)
            return;

        await DownLoadAsync(server, sessionId, jobId, cfg.File, ct);
    }

    private async Task DownLoadAsync(string server, string sessionId, string jobId, string file, CancellationToken ct)
    {
        var response = await _httpRequestFactory
            .New(server)
            .Get($"api/views/search/messages/job/{jobId}/raw.csv")
            .Header("X-Requested-By", "cli")
            .Cookie("authentication", sessionId)
            .RunAsync(HttpCompletionOption.ResponseHeadersRead, ct);

        if (response.IsFailure)
        {
            this.Error<string>("Download failed: {response}", await response.Content.ReadAsStringAsync(ct));
            return;
        }

        // Create a file stream to store the downloaded data.
        // This really can be any type of writeable stream.
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = ArrayPool<byte>.Shared.Rent(1_048_576);
        try
        {
            var progress = 0L;
            int bytesRead;
            Console.Write("downloaded: ");
            var pos = Console.CursorLeft;
            var strlen = Console.WindowWidth - pos - 1;

            while ((bytesRead = await stream.ReadAsync(buffer, ct).ConfigureAwait(false)) != 0)
            {
                // believe that we'll never less than bytes in last chunk
                for (var i = 3; i < bytesRead; i++)
                    Cleanup(buffer.AsSpan(i - 3, 4));

                await fs.WriteAsync(buffer.AsMemory(0, bytesRead), ct).ConfigureAwait(false);
                progress += bytesRead;
                Console.CursorLeft = pos;
                Console.Write(progress.Bytes().Humanize().PadRight(strlen));
            }

            Console.WriteLine();
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw;
        }
    }

    private static void Cleanup(Span<byte> span)
    {
        // start line "
        if (span[0] == '\n' && span[1] == '"')
            span[1] = (byte)'[';
        // end line "
        else if (span[2] == '"' && span[3] == '\n')
            span[2] = (byte)' ';
        // timestamp delimiter Z",
        else if (span[0] == 'Z' && span[1] == '"' && span[2] == ',' && span[3] == '"')
        {
            span[1] = (byte)']';
            span[2] = (byte)' ';
            span[3] = (byte)' ';
        }
        // field delimiter ","
        else if (span[1] == '"' && span[2] == ',' && span[3] == '"')
        {
            span[1] = (byte)' ';
            span[2] = (byte)' ';
            span[3] = (byte)' ';
        }
    }

    private async Task<string?> ExportAsync(string server, string sessionId, string searchId, string subId)
    {
        var response = await _httpRequestFactory
            .New(server)
            .Post($"api/views/export/{searchId}/{subId}")
            .Header("X-Requested-By", "cli")
            .Cookie("authentication", sessionId)
            .JsonContent(
                new
                {
                    execution_state = new { parameter_bindinds = new { } },
                    fields_in_order = new[] { "timestamp", "source", "log_level", "message" }
                }
            )
            .RunAsync();
        if (response.IsSuccess)
            return await response.Content.ReadAsStringAsync();

        this.Error<string>("Export failed: {response}", await response.Content.ReadAsStringAsync());
        return null;
    }

    private async Task<(string searchId, string subId)?> SearchAsync(
        string server,
        string sessionId,
        string query,
        OneOf<AbsoluteTime, RelativeTime, Exception> time
    )
    {
        var searchId = ObjectId.GenerateNewId().ToString();
        var queryId = Guid.NewGuid().ToString();
        var subId = Guid.NewGuid().ToString();

        var response = await _httpRequestFactory
            .New(server)
            .Post("api/views/search")
            .Header("X-Requested-By", "cli")
            .Cookie("authentication", sessionId)
            .JsonContent(
                new
                {
                    id = searchId,
                    queries = new[]
                    {
                        new
                        {
                            id = queryId,
                            query = new { type = "elasticsearch", query_string = query },
                            timerange = time.Value,
                            filter = null as object,
                            search_types = new[]
                            {
                                new
                                {
                                    offset = 0,
                                    decorators = Array.Empty<string>(),
                                    type = "messages",
                                    id = subId,
                                    limit = 150,
                                    filters = Array.Empty<string>(),
                                    sort = new[] { new { field = "timestamp", order = "ASC" } }
                                }
                            }
                        }
                    },
                    parameters = Array.Empty<string>()
                }
            )
            .RunAsync();
        if (response.IsSuccess)
            return (searchId, subId);

        this.Error<string>("Search failed: {response}", await response.Content.ReadAsStringAsync());
        return null;
    }

    private async Task<bool> ValidateAsync(string server, string sessionId, string query)
    {
        var response = await _httpRequestFactory
            .New(server)
            .Post("api/search/validate")
            .Header("X-Requested-By", "cli")
            .Cookie("authentication", sessionId)
            .JsonContent(
                new
                {
                    query,
                    timerange = new { type = "relative", from = 14400 },
                    streams = Array.Empty<string>()
                }
            )
            .AsResponseAsync(new { status = string.Empty });
        var status = response.Data.status;
        if (status == "OK")
            return true;

        this.Error<string>("Query validation failed: {response}", await response.Content.ReadAsStringAsync());
        return false;
    }

    private async Task<string?> LogInAsync(string server, string login, string pass)
    {
        var response = await _httpRequestFactory
            .New(server)
            .Post("api/system/sessions")
            .Header("X-Requested-By", "cli")
            .JsonContent(
                new
                {
                    host = server,
                    username = login,
                    password = pass
                }
            )
            .AsResponseAsync(new { session_id = string.Empty });
        var sessionId = response.Data.session_id;
        if (!sessionId.IsNullOrWhiteSpace())
            return sessionId;

        this.Error<string>("Auth failed: {response}", await response.Content.ReadAsStringAsync());
        return null;
    }

    private OneOf<AbsoluteTime, RelativeTime, Exception> ResolveTime(string time)
    {
        time = time.ToLowerInvariant();

        if (TryParseRelative(time, 'd', 86400, out var days))
            return days;

        if (TryParseRelative(time, 'h', 3600, out var hours))
            return hours;

        if (TryParseRelative(time, 'm', 60, out var minutes))
            return minutes;

        if (TryParseAbsolute(time, out var result))
            return result;

        return new Exception($"Failed to parse time: {time}");

        static bool TryParseRelative(
            string value,
            char unit,
            uint multiplier,
            [NotNullWhen(true)] out RelativeTime? result
        )
        {
            if (value.EndsWith(unit) && uint.TryParse(value[..^1], out var val) && val > 0)
            {
                result = new RelativeTime(val * multiplier);
                return true;
            }

            result = null;
            return false;
        }

        static bool TryParseAbsolute(string value, [NotNullWhen(true)] out AbsoluteTime? result)
        {
            result = null;

            var parts = value
                .Split('_', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();
            if (parts.Length != 2)
                return false;

            if (!DateTime.TryParse(parts[0], out var from) || !DateTime.TryParse(parts[1], out var to))
                return false;

            result = new AbsoluteTime(from.InUtc(), to.InUtc());

            return true;
        }
    }

    private record RelativeTime
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "relative";

        [JsonPropertyName("from")]
        public uint From { get; }

        public RelativeTime(uint from)
        {
            From = from;
        }
    }

    private record AbsoluteTime
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "absolute";

        [JsonPropertyName("from")]
        public string From { get; }

        [JsonPropertyName("to")]
        public string To { get; }

        public AbsoluteTime(DateTime from, DateTime to)
        {
            From = from.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            To = to.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        }
    }
}

public class DumpCommandConfiguration
{
    [Position(1, isRequired: true)]
    [Help("Server name")]
    public string Server { get; set; } = string.Empty;

    [Position(2, isRequired: true)]
    [Help("time range")]
    public string Time { get; set; } = string.Empty;

    [Position(3, isRequired: true)]
    [Help("output file")]
    public string File { get; set; } = string.Empty;

    [Raw]
    [Help("Query")]
    public string Query { get; set; } = string.Empty;
}
