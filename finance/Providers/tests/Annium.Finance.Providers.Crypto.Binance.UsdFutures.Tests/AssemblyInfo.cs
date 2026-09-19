using Annium.Finance.Providers.Tests.Lib;
using Xunit.Sdk;
using Xunit.v3;

// where CI looks for this project's secrets: BINANCE_USDFUTURES_TEST_KEY and friends. Spot and USD-M
// futures keep separate credentials on purpose - they are separate venues, and a further provider will
// differ again - so the environment has to keep them apart the way the per-project test.env already does
[assembly: TestEnvScope("BINANCE_USDFUTURES")]

// the write block trades on one real account, and xUnit's default parallelism is per collection: the
// eight trading tests happen to share a class, so they happen to run in order, and nothing said so. A
// second write class - a probe was one, until it was moved out of the block - then trades beside them,
// and one test cancelling every open order on the symbol is another test's order disappearing for no
// reason it can see. The neighbouring spot assembly already claimed this suite ran serially; it did not.
[assembly: Parallelization(Mode = ParallelMode.None)]
