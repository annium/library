using System.Threading.Tasks;
using Annium.Integrations.Social.Telegram.Integration.Messages;
using Annium.Integrations.Social.Telegram.Integration.Messages.Requests;
using Annium.Logging;
using Annium.Net.Http;

namespace Annium.Integrations.Social.Telegram.Internal.Integration.Messages;

internal class MessageApi : IMessageApi, ILogSubject
{
    public ILogger Logger { get; }
    private readonly ApiContext _context;

    public MessageApi(ApiContext context, ILogger logger)
    {
        Logger = logger;
        _context = context;
    }

    public async ValueTask<bool> SendMessageAsync(SendMessageRequest request)
    {
        var response = await _context.Http.Post("sendMessage").JsonContent(request).WithLogFrom(this).RunAsync();

        return response.IsSuccess;
    }
}
