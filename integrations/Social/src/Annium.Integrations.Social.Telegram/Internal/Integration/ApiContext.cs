using System;
using System.IO;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Integrations.Social.Telegram.Internal.Integration;

/// <summary>
/// Per-bot context bundling the HTTP request factory pointed at the bot's Telegram API base URL and the serializer
/// used for requests and responses.
/// </summary>
internal sealed class ApiContext
{
    /// <summary>
    /// Gets a new HTTP request pre-configured against this bot's Telegram API base URL.
    /// </summary>
    public IHttpRequest Http => _httpRequestFactory.New(_server);

    /// <summary>
    /// Gets the serializer used to encode requests and decode responses for this bot's API calls.
    /// </summary>
    public ISerializer<Stream> Serializer { get; }

    /// <summary>
    /// The bot's Telegram API base URL (<c>https://api.telegram.org/bot{token}</c>).
    /// </summary>
    private readonly Uri _server;

    /// <summary>
    /// The factory used to create HTTP requests against the bot's Telegram API base URL.
    /// </summary>
    private readonly IHttpRequestFactory _httpRequestFactory;

    public ApiContext(Uri server, IHttpRequestFactory httpRequestFactory, ISerializer<Stream> serializer)
    {
        _server = server;
        _httpRequestFactory = httpRequestFactory;
        Serializer = serializer;
    }
}
