using System.Threading;
using System.Threading.Tasks;
using Annium.Social.Telegram.Integration.Messages;
using Annium.Social.Telegram.Integration.Messages.Requests;
using Annium.Social.Telegram.Integration.Shared.Domain;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Social.Telegram.Internal.Integration.Messages;

/// <summary>
/// Default <see cref="IMessageApi"/> implementation that sends messages through the Telegram Bot API's
/// <c>sendMessage</c> endpoint.
/// </summary>
internal class MessageApi : IMessageApi, ILogSubject
{
    /// <summary>
    /// The logger used to record request failures and non-OK responses.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The API context used to issue the <c>sendMessage</c> request.
    /// </summary>
    private readonly ApiContext _context;

    /// <summary>
    /// Creates the message API for one bot.
    /// </summary>
    /// <param name="context">The API context used to issue requests.</param>
    /// <param name="logger">The logger used to trace calls and report rejections.</param>
    public MessageApi(ApiContext context, ILogger logger)
    {
        Logger = logger;
        _context = context;
    }

    /// <summary>
    /// Sends a text message via the Telegram <c>sendMessage</c> endpoint, logging and returning
    /// <see langword="false"/> if the response could not be parsed or Telegram reported a failure.
    /// </summary>
    /// <param name="request">The chat and text to send.</param>
    /// <param name="ct">The token used to cancel the request.</param>
    /// <returns><see langword="true"/> if Telegram accepted the message; otherwise, <see langword="false"/>.</returns>
    public async ValueTask<bool> SendMessageAsync(SendMessageRequest request, CancellationToken ct = default)
    {
        // Telegram answers a rejected send with an ok:false body carrying Description — reading only
        // the HTTP status discarded the single diagnostic the API gives for the failure
        var response = await _context
            .Http.Post("sendMessage")
            .JsonContent(request)
            .WithRedactedLogFrom(this)
            .AsAsync<Response<Message>>(ct);

        if (response is null)
        {
            this.Error("failed to parse sendMessage response");

            return false;
        }

        if (!response.Ok)
            this.Error<string>("sendMessage failed: {description}", response.Description);

        return response.Ok;
    }
}
