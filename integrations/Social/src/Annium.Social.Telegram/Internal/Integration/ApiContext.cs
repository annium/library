using System;
using System.IO;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Social.Telegram.Internal.Integration;

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

    /// <summary>
    /// Creates the context for one bot.
    /// </summary>
    /// <param name="server">The bot's Telegram API base URL.</param>
    /// <param name="httpRequestFactory">The factory used to create requests against that URL.</param>
    /// <param name="serializer">The serializer used for requests and responses.</param>
    public ApiContext(Uri server, IHttpRequestFactory httpRequestFactory, ISerializer<Stream> serializer)
    {
        _server = server;
        _httpRequestFactory = httpRequestFactory;
        Serializer = serializer;
    }
}
