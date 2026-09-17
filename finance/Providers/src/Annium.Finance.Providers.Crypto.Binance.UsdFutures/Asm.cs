using System.Runtime.CompilerServices;
using Annium.Core.Runtime.Types;

[assembly: AutoScanned]
[assembly: InternalsVisibleTo("Annium.Finance.Providers.Crypto.Binance.UsdFutures.Tests")]

// the benchmark resolves the registered serializer by the key production uses, rather than rebuilding an
// equivalent one - a copied key string is a copy that drifts, and a drifted one measures nothing real
[assembly: InternalsVisibleTo("Annium.Finance.Providers.Benchmark")]
