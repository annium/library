using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Regression test for the semaphore-release-without-acquire bug in
/// <see cref="MessagingManagedSocket.SendAsync"/>. If <c>WaitAsync</c> throws
/// <see cref="System.OperationCanceledException"/> before acquiring the gate, the
/// <c>finally</c> block must not call <c>Release()</c>; otherwise the semaphore count
/// exceeds its maximum and every subsequent send throws
/// <see cref="System.Threading.SemaphoreFullException"/>.
/// </summary>
public class MessagingManagedSocketSemaphoreGuardTests
{
    /// <summary>
    /// A pre-cancelled cancellation token passed to <c>SendAsync</c> must NOT corrupt the
    /// internal semaphore. The call returns <see cref="SocketSendStatus.Canceled"/> and a
    /// subsequent send with a live token succeeds.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_PreCancelledToken_DoesNotCorruptSemaphore()
    {
        // arrange — a MemoryStream is enough; we exercise the semaphore path only
        using var stream = new MemoryStream();
        using var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var payload = new byte[] { 1, 2, 3 };

        // act — pre-cancelled send must not throw (the fix suppresses the spurious Release)
        var cancelled = await socket.SendAsync(payload, cts.Token);

        // assert — returns Canceled (not a propagated SemaphoreFullException)
        cancelled.Is(SocketSendStatus.Canceled);

        // act — subsequent send with a live token must succeed; semaphore is intact
        var ok = await socket.SendAsync(payload, CancellationToken.None);

        // assert — would throw SemaphoreFullException under the old buggy Release pattern
        ok.Is(SocketSendStatus.Ok);
    }

    /// <summary>
    /// A send holding the gate when the socket is disposed answers with a status rather than throwing.
    /// </summary>
    /// <remarks>
    /// The other half of the same race, and the half that was missed. Disposal was written to handle a
    /// send that is <em>about to wait</em> on the gate; this is a send that already holds one and is
    /// about to release it. The release sits in a <c>finally</c>, and a throw from a <c>finally</c>
    /// passes straight through the <c>catch</c> above it — so the one method that promises to answer
    /// with a status and never throw, threw.
    ///
    /// It reached CI as a flake, failing about one run in many and passing on the immediate re-run.
    /// Here it is deterministic: the stream blocks inside the write until the test says otherwise, so
    /// disposal is guaranteed to land while the gate is held.
    /// </remarks>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact(Timeout = 30_000)]
    public async Task SendAsync_DisposedWhileHoldingTheGate_AnswersInsteadOfThrowing()
    {
        // arrange
        var writeReached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var letWriteFinish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var stream = new BlockingStream(writeReached, letWriteFinish);
        var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

        // act — the send holds the gate and parks inside the write
        var sending = socket.SendAsync(new byte[] { 1, 2, 3 }, CancellationToken.None);
        await writeReached.Task.WaitAsync(TestContext.Current.CancellationToken);

        // VSTHRD103: MessagingManagedSocket.Dispose() is synchronous (no async variant).
#pragma warning disable VSTHRD103
        socket.Dispose();
#pragma warning restore VSTHRD103
        letWriteFinish.SetResult();

        // assert — the gate it is about to release no longer exists
        var status = await sending;
        (status is SocketSendStatus.Ok or SocketSendStatus.Closed).IsTrue(
            $"a send disposed mid-flight answered {status}"
        );
    }

    /// <summary>
    /// A stream whose write parks until the test releases it, so a disposal can be placed inside it.
    /// </summary>
    private sealed class BlockingStream : Stream
    {
        /// <summary>Signalled once a write has begun.</summary>
        private readonly TaskCompletionSource _reached;

        /// <summary>Awaited by a write before it completes.</summary>
        private readonly TaskCompletionSource _release;

        /// <summary>Set once the first write has parked, so only that one blocks.</summary>
        private bool _parked;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockingStream"/> class.
        /// </summary>
        /// <param name="reached">Signalled once a write has begun.</param>
        /// <param name="release">Awaited by the first write before it completes.</param>
        public BlockingStream(TaskCompletionSource reached, TaskCompletionSource release)
        {
            _reached = reached;
            _release = release;
        }

        /// <summary>Gets a value indicating whether the stream supports reading.</summary>
        public override bool CanRead => false;

        /// <summary>Gets a value indicating whether the stream supports seeking.</summary>
        public override bool CanSeek => false;

        /// <summary>Gets a value indicating whether the stream supports writing.</summary>
        public override bool CanWrite => true;

        /// <summary>Gets the length of the stream, which this stream does not have.</summary>
        public override long Length => throw new NotSupportedException();

        /// <summary>Gets or sets the position in the stream, which this stream does not have.</summary>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>Parks the first write until the test releases it.</summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task that completes once the write is released.</returns>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            if (_parked)
                return;

            _parked = true;
            _reached.SetResult();

            // VSTHRD003: parking here is the point - the test starts this work and releases it, and
            // that is what places the disposal inside a send that holds the gate.
#pragma warning disable VSTHRD003
            await _release.Task;
#pragma warning restore VSTHRD003
        }

        /// <summary>Does nothing; there is nothing buffered.</summary>
        public override void Flush() { }

        /// <summary>Not supported.</summary>
        /// <param name="buffer">Unused.</param>
        /// <param name="offset">Unused.</param>
        /// <param name="count">Unused.</param>
        /// <returns>Never returns.</returns>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <summary>Not supported.</summary>
        /// <param name="offset">Unused.</param>
        /// <param name="origin">Unused.</param>
        /// <returns>Never returns.</returns>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <summary>Not supported.</summary>
        /// <param name="value">Unused.</param>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>Not supported.</summary>
        /// <param name="buffer">Unused.</param>
        /// <param name="offset">Unused.</param>
        /// <param name="count">Unused.</param>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
