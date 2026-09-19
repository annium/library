using Annium.Finance.Providers.Tests.Lib;
using Xunit.Sdk;
using Xunit.v3;

// where CI looks for this project's secrets: BINANCE_SPOT_TEST_KEY and friends. Spot and USD-M futures
// keep separate credentials on purpose - they are separate venues, and a further provider will differ
// again - so the environment has to keep them apart the way the per-project test.env already does
[assembly: TestEnvScope("BINANCE_SPOT")]

// this assembly now contains a test that measures a decay over wall-clock seconds, and wall-clock
// assertions lose to CPU contention long before they lose to a real defect. The precedent is this repo's
// own: Annium.Net.WebSockets.Tests and the USD-M futures suite both run serially for the same reason,
// after a timing test that never failed locally failed twice in CI
[assembly: Parallelization(Mode = ParallelMode.None)]
