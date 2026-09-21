using System;
using Annium.Core.DependencyInjection;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Core.Shared.TimeSync;
using Annium.Finance.Providers.Crypto.Binance.Base.Internal.User.Services;
using Annium.Finance.Providers.Crypto.Binance.Base.User.Services;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Finance.Providers.Crypto.Binance.Base.User;

/// <summary>Factory extension methods for constructing Binance account/trading services from an <see cref="IServiceProvider"/>.</summary>
public static class ServiceProviderExtensions
{
    /// <summary>Creates a <see cref="SignatureService"/> that signs requests with the given account's key and secret, using the keyed server time source.</summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="settings">The user settings providing the API key and secret.</param>
    /// <param name="providerKey">The key identifying the registered <see cref="IServerTimeSource"/> to resolve.</param>
    /// <returns>The created signature service.</returns>
    public static ISignatureService CreateSignatureService(
        this IServiceProvider sp,
        UserSettings settings,
        ProviderKey providerKey
    )
    {
        var serverTimeSource = sp.ResolveKeyed<IServerTimeSource>(providerKey);

        return new SignatureService(settings, serverTimeSource);
    }

    /// <summary>Creates a <see cref="UserStream"/> that connects to the user data stream WebSocket using keys supplied by the given listen key resolver.</summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="wsApi">The base URI of the user data stream WebSocket API.</param>
    /// <param name="listenKeyUriPath">The path appended to <paramref name="wsApi"/>, followed by the listen key.</param>
    /// <param name="listenKeyResolver">The resolver supplying and refreshing the listen key the stream connects with.</param>
    /// <param name="monitor">The monitor the stream reports its connection status into.</param>
    /// <returns>The created user stream.</returns>
    public static IUserStream CreateListenKeyUserStream(
        this IServiceProvider sp,
        Uri wsApi,
        string listenKeyUriPath,
        IListenKeyResolver listenKeyResolver,
        IStatusMonitor monitor
    )
    {
        var statusReporter = monitor.CreateReporter();
        var logger = sp.Resolve<ILogger>();

        return new UserStream(wsApi, listenKeyUriPath, listenKeyResolver, statusReporter, logger);
    }

    /// <summary>
    /// Creates a user stream that subscribes the account's events onto a WebSocket API connection.
    /// </summary>
    /// <remarks>
    /// The alternative to <see cref="CreateListenKeyUserStream"/>, for a venue whose account stream is
    /// reached by a signed method call rather than by a key spent in a URL. Which of the two a venue takes
    /// is its own fact and belongs in that venue's factory, not behind a flag here.
    /// </remarks>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="wsApi">The endpoint of the WebSocket API.</param>
    /// <param name="subscribeRetryInterval">How often a refused subscription is attempted again, in milliseconds.</param>
    /// <param name="signatureService">Signs the subscription request.</param>
    /// <param name="monitor">The monitor the stream reports its connection status into.</param>
    /// <returns>The created user stream.</returns>
    public static IUserStream CreateWsApiUserStream(
        this IServiceProvider sp,
        Uri wsApi,
        int subscribeRetryInterval,
        ISignatureService signatureService,
        IStatusMonitor monitor
    )
    {
        var rateLimiter = sp.Resolve<IRateLimiter>();
        var statusReporter = monitor.CreateReporter();
        var logger = sp.Resolve<ILogger>();

        return new WsApiUserStream(
            wsApi,
            subscribeRetryInterval,
            signatureService,
            rateLimiter,
            statusReporter,
            logger
        );
    }

    /// <summary>Creates a <see cref="ListenKeyResolver"/> that fetches and keeps alive a listen key from the given endpoint.</summary>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="httpApi">The base URI of the account HTTP API the listen key is fetched from.</param>
    /// <param name="listenKeyConfig">The fetch and confirm intervals driving the resolver's timer.</param>
    /// <param name="endpoint">The relative path of the listen key endpoint.</param>
    /// <param name="listenKeyKey">The keyed HTTP request factory registration key to resolve the request factory with.</param>
    /// <param name="signatureService">The service used to sign the listen key request.</param>
    /// <param name="monitor">The monitor the resolver reports its connection status into.</param>
    /// <returns>The created listen key resolver.</returns>
    public static IListenKeyResolver CreateListenKeyResolver(
        this IServiceProvider sp,
        Uri httpApi,
        ListenKeyConfiguration listenKeyConfig,
        string endpoint,
        string listenKeyKey,
        ISignatureService signatureService,
        IStatusMonitor monitor
    )
    {
        var httpRequestFactory = sp.ResolveHttpRequestFactory(listenKeyKey);
        var rateLimiter = sp.Resolve<IRateLimiter>();
        var statusReporter = monitor.CreateReporter();
        var logger = sp.Resolve<ILogger>();

        return new ListenKeyResolver(
            httpApi,
            listenKeyConfig,
            endpoint,
            httpRequestFactory,
            signatureService,
            rateLimiter,
            statusReporter,
            logger
        );
    }
}
