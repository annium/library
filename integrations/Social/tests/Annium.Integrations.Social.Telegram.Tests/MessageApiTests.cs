using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration.Messages.Requests;
using Annium.Integrations.Social.Telegram.Internal.Integration.Messages;
using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.Telegram.Tests;

/// <summary>
/// Tests for the outbound message API: how a Telegram-reported failure surfaces to the caller.
/// </summary>
public class MessageApiTests : TestBase
{
    public MessageApiTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A send Telegram accepts is reported as success.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendMessage_Ok_ReturnsTrue()
    {
        // arrange
        var (server, context) = RunApi(
            (_, _) =>
                new ApiReply(
                    """
                    {"ok":true,"result":{"message_id":1,"chat":{"id":42,"type":"private"},"date":1,"text":"hi"}}
                    """
                )
        );
        await using var _ = server;
        var api = new MessageApi(context, Logger);

        // act
        var result = await api.SendMessageAsync(
            new SendMessageRequest { ChatId = 42, Text = "hi" },
            TestContext.Current.CancellationToken
        );

        // assert
        result.IsTrue();
    }

    /// <summary>
    /// A send Telegram rejects is reported as failure, with its description logged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendMessage_Rejected_ReturnsFalseAndLogsDescription()
    {
        // arrange — regression: only the HTTP status was inspected, so Description (the sole diagnostic
        // Telegram gives for a rejected send) was dropped
        var (server, context) = RunApi(
            (_, _) =>
                new ApiReply("""{"ok":false,"description":"Bad Request: chat not found"}""", HttpStatusCode.BadRequest)
        );
        await using var _ = server;
        var api = new MessageApi(context, Logger);

        // act
        var result = await api.SendMessageAsync(
            new SendMessageRequest { ChatId = 42, Text = "hi" },
            TestContext.Current.CancellationToken
        );

        // assert
        result.IsFalse();
        Logs.Any(x => x.Message.Contains("chat not found")).IsTrue("failure description must be logged");
    }

    /// <summary>
    /// An unparseable response is reported as failure rather than as success.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendMessage_UnparseableResponse_ReturnsFalse()
    {
        // arrange
        var (server, context) = RunApi((_, _) => new ApiReply("not json"));
        await using var _ = server;
        var api = new MessageApi(context, Logger);

        // act
        var result = await api.SendMessageAsync(
            new SendMessageRequest { ChatId = 42, Text = "hi" },
            TestContext.Current.CancellationToken
        );

        // assert
        result.IsFalse();
    }
}
