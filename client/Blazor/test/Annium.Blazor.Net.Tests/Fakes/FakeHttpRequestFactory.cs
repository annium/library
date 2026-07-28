using System;
using Annium.Net.Http;

namespace Annium.Blazor.Net.Tests.Fakes;

/// <summary>
/// A fake <see cref="IHttpRequestFactory"/> that records the base <see cref="Uri"/> handed to
/// <see cref="New(Uri)"/>, so the host-base-address wiring of <c>HostHttpRequestFactory</c> can be asserted
/// without a real HTTP stack. The other overloads are unsupported — the factory under test uses only <see cref="New(Uri)"/>.
/// </summary>
internal sealed class FakeHttpRequestFactory : IHttpRequestFactory
{
    /// <summary>
    /// Gets the base <see cref="Uri"/> passed to the most recent <see cref="New(Uri)"/> call, or null if never called.
    /// </summary>
    public Uri? LastBaseUri { get; private set; }

    /// <summary>
    /// Not supported — the factory under test always supplies a base address.
    /// </summary>
    /// <returns>Never returns; always throws.</returns>
    public IHttpRequest New() => throw new NotSupportedException("base-less New() is not exercised by these tests");

    /// <summary>
    /// Not supported — the factory under test passes a <see cref="Uri"/>, not a string.
    /// </summary>
    /// <param name="baseUri">The base URI string.</param>
    /// <returns>Never returns; always throws.</returns>
    public IHttpRequest New(string baseUri) =>
        throw new NotSupportedException("string-based New() is not exercised by these tests");

    /// <summary>
    /// Records the base <see cref="Uri"/> and returns null (the returned request is not exercised by these tests).
    /// </summary>
    /// <param name="baseUri">The base URI passed by the factory under test.</param>
    /// <returns>Null — the caller under test forwards it, and the tests only assert the recorded base URI.</returns>
    public IHttpRequest New(Uri baseUri)
    {
        LastBaseUri = baseUri;
        return null!;
    }
}
