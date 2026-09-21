using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Finance.Providers.Abstractions.Connectors.User;
using Annium.Finance.Providers.Abstractions.Domain.Shared;
using Annium.Finance.Providers.Abstractions.Domain.Shared.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User;
using Annium.Finance.Providers.Core.Shared;
using Annium.Finance.Providers.Core.Shared.RateLimits;
using Annium.Finance.Providers.Core.Shared.Status;
using Annium.Finance.Providers.Core.User;
using Annium.Finance.Providers.Crypto.Binance.Base.User;
using Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User.Services;
using Annium.Logging;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using static Annium.Finance.Providers.Crypto.Binance.Spot.Constants;

namespace Annium.Finance.Providers.Crypto.Binance.Spot.Internal.User;

/// <summary>
/// Builds fully-wired <see cref="UserConnector"/> instances for the Binance spot provider: resolves
/// configuration, the underlying provider, request signing, the order management request factories, the
/// account stream, and the context/orders/trades loaders.
/// </summary>
/// <param name="sp">The service provider used to resolve dependencies.</param>
internal class UserConnectorFactory(IServiceProvider sp) : IUserConnectorInstanceFactory
{
    /// <summary>Creates a new Binance spot user connector for the given settings.</summary>
    /// <param name="settings">The account connection settings to configure the connector with.</param>
    /// <param name="monitor">The monitor the connector and its components report their status into.</param>
    /// <param name="disposable">The disposable box the connector will register its cleanup actions on.</param>
    /// <returns>The created user connector.</returns>
    public IUserConnector Create(UserSettings settings, IStatusMonitor monitor, AsyncDisposableBox disposable)
    {
        var config = sp.Resolve<IMapper>().Map<UserConfig>(settings);
        var providerKey = settings.GetProviderKey();

        var timeProvider = sp.Resolve<ITimeProvider>();
        var provider = sp.CreateUserProvider(settings);
        var queryProcessor = sp.Resolve<QueryProcessor>();
        var signatureService = sp.CreateSignatureService(settings, providerKey);

        // the stream signs its subscription with the venue's own clock, so the clock has to be tracked
        // before anything is sent - a subscription signed against an unsynchronised one is refused, and
        // refused for a reason the refusal does not name
        sp.TrackServerTime(providerKey, monitor, ref disposable);

        var initOrderRequestFactory = sp.ResolveHttpRequestFactory(InitOrderKey);
        var modifyOrderRequestFactory = sp.ResolveHttpRequestFactory(ModifyOrderKey);
        var cancelOrderRequestFactory = sp.ResolveHttpRequestFactory(CancelOrderKey);
        var cancelAllOrdersRequestFactory = sp.ResolveHttpRequestFactory(CancelAllOrdersKey);

        var userStream = sp.CreateWsApiUserStream(
            config.WsApi,
            config.SubscribeRetryInterval,
            signatureService,
            monitor
        );

        var orderUpdateEventSerializer = sp.ResolveSerializer<ReadOnlyMemory<byte>>(
            OrderUpdateKey,
            MediaTypeNames.Application.Json
        );
        var accountUpdateEventSerializer = sp.ResolveSerializer<ReadOnlyMemory<byte>>(
            AccountUpdateKey,
            MediaTypeNames.Application.Json
        );

        var contextLoader = sp.CreateUserContextLoader(config.ReloadContext, monitor, provider, ref disposable);
        var ordersLoader = sp.CreateCompositeLoader(config.ReloadOrders, monitor, LoadOrdersAsync);
        var tradesLoader = sp.CreateKeyedLoader<string, long, IReadOnlyCollection<TradeModel>>(
            config.ReloadTrades,
            monitor,
            timeProvider.Now.ToUnixTimeMilliseconds(),
            LoadTradesAsync,
            GetTradesContext
        );

        // the context loader is added to the box by the helper that builds it; these two have no such
        // helper, and the connector only unhooks their events, so without this they outlive it - timers
        // still fetching, status reporters still bound, and the keyed one still holding an entry per symbol
        disposable += ordersLoader;
        disposable += tradesLoader;

        // and the same for the stream: the connector unhooks its events and nothing disposed it, so a
        // torn-down connector left a socket reconnecting, a subscription retrying, and a target bound to a
        // monitor that could then never read connected again
        disposable += userStream;

        var rateLimiter = sp.Resolve<IRateLimiter>();
        var reporter = monitor.CreateReporter();
        var logger = sp.Resolve<ILogger>();

        return new UserConnector(
            config,
            provider,
            queryProcessor,
            signatureService,
            initOrderRequestFactory,
            modifyOrderRequestFactory,
            cancelOrderRequestFactory,
            cancelAllOrdersRequestFactory,
            rateLimiter,
            contextLoader,
            ordersLoader,
            tradesLoader,
            userStream,
            orderUpdateEventSerializer,
            accountUpdateEventSerializer,
            reporter,
            monitor,
            disposable,
            logger
        );

        async Task<IBaseResult<IReadOnlyCollection<OrderModel>?>> LoadOrdersAsync(CancellationToken ct)
        {
            var result = await provider.LoadOpenOrdersAsync();

            return result;
        }

        async Task<IBaseResult<IReadOnlyCollection<TradeModel>?>> LoadTradesAsync(
            string symbol,
            long since,
            CancellationToken ct
        )
        {
            var result = await provider.LoadTradesAsync(symbol, since);

            return result;
        }

        long GetTradesContext(string symbol, long since, IReadOnlyCollection<TradeModel> trades)
        {
            var result = trades.Select(x => x.Moment).MaxBy(x => x);

            return result;
        }
    }
}
