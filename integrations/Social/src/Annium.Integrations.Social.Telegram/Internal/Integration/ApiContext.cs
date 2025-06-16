using System;
using System.IO;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Integrations.Social.Telegram.Internal.Integration;

internal sealed class ApiContext
{
    public IHttpRequest Http => _httpRequestFactory.New(_server);
    public ISerializer<Stream> Serializer { get; }
    private readonly Uri _server;
    private readonly IHttpRequestFactory _httpRequestFactory;

    public ApiContext(Uri server, IHttpRequestFactory httpRequestFactory, ISerializer<Stream> serializer)
    {
        _server = server;
        _httpRequestFactory = httpRequestFactory;
        Serializer = serializer;
    }
}
