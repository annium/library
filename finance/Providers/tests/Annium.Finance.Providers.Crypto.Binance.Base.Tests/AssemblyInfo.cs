using Xunit.Sdk;
using Xunit.v3;

// This assembly holds real-network tests: the stream tests spin an HttpListener-backed WebSocket server
// and a live client, and the listen key tests spin a local HTTP server beside them. Running collections in
// parallel makes them contend for CPU and OS socket/port resources, and under load a round-trip that takes
// milliseconds on an idle machine outruns the test's deadline - which reads as a hang in a different test
// on every run, rather than as a defect anyone can locate.
//
// The same conclusion was reached, and written down, by Annium.Net.WebSockets.Tests before this assembly
// grew socket tests of its own; this is that precedent applied here rather than rediscovered.
[assembly: Parallelization(Mode = ParallelMode.None)]
