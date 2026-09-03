using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Social.Telegram.Obsolete.Api.Models;

namespace Annium.Social.Telegram.Obsolete.Api;

public class TelegramApi : ITelegramApi
{
    private static readonly IDictionary<string, string> _emptyQuery = new Dictionary<string, string>();
    private const uint Timeout = 3600;
    private const uint MaxMessageLength = 4096;
    private readonly TelegramConfiguration _configuration;

    public TelegramApi(TelegramConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<TelegramApiResult<TelegramUser>> GetMeAsync() =>
        await GetAsync("getMe", _emptyQuery, new TelegramUser());

    public async Task<TelegramApiResult<TelegramUpdate[]>> GetUpdatesAsync(int offset = 0)
    {
        var query = new Dictionary<string, string> { ["offset"] = offset.ToString(), ["timeout"] = Timeout.ToString() };

        return await GetAsync<TelegramUpdate[]>("getUpdates", query, []);
    }

    public async Task<TelegramApiResult<TelegramMessage>> SendMessageAsync(
        int chatId,
        string text,
        IReadOnlyList<string>? buttons = null
    )
    {
        var replyMarkup =
            buttons != null && buttons.Any()
                ? new { keyboard = GetKeyboard(buttons) }
                : new { removeKeyboard = true } as object;

        while (text.Length > MaxMessageLength)
        {
            var chunkLength = GetChunkLength(text);
            await PostAsync(
                "sendMessage",
                new
                {
                    chatId,
                    text = text[..chunkLength].TrimEnd(),
                    replyMarkup,
                },
                new TelegramMessage()
            );
            text = text[chunkLength..].TrimStart();
        }

        return await PostAsync(
            "sendMessage",
            new
            {
                chatId,
                text,
                replyMarkup,
            },
            new TelegramMessage()
        );

        int GetChunkLength(string message)
        {
            var length = 0;
            var index = 0;

            while (
                (index = message.IndexOf(Environment.NewLine, index + 1, StringComparison.Ordinal)) < MaxMessageLength
            )
                length = index;

            return length;
        }
    }

    public Task<TelegramApiResult<TelegramMessage>> SendMessageAsync(
        int chatId,
        IReadOnlyList<string> text,
        IReadOnlyList<string>? buttons = null
    ) => SendMessageAsync(chatId, string.Join(Environment.NewLine, text), buttons);

    private async Task<TelegramApiResult<TResult>> GetAsync<TResult>(
        string method,
        IDictionary<string, string> query,
        TResult defaultValue
    ) => await SendAsync(GetMessage(HttpMethod.Get, method, query), defaultValue);

    private async Task<TelegramApiResult<TResult>> PostAsync<TResult>(string method, object body, TResult defaultValue)
    {
        var message = GetMessage(HttpMethod.Post, method, new Dictionary<string, string>());
        message.Content = CreateJsonContent(body);

        return await SendAsync(message, defaultValue);
    }

    private static StringContent CreateJsonContent(object body)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(
                body,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
            )
        );
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return content;
    }

    private HttpRequestMessage GetMessage(HttpMethod httpMethod, string method, IDictionary<string, string>? query)
    {
        var message = new HttpRequestMessage();
        message.Method = httpMethod;

        var uri = $"{_configuration.ApiUrl}{method}";
        if (query != null && query.Count > 0)
            uri += $"?{string.Join("&", query.Select(pair => $"{pair.Key}={pair.Value}"))}";

        message.RequestUri = new Uri(uri);

        return message;
    }

    private async Task<TelegramApiResult<TResult>> SendAsync<TResult>(HttpRequestMessage message, TResult defaultValue)
    {
        using var client = new HttpClient();

        client.Timeout = TimeSpan.FromSeconds(Timeout);
        if (_configuration.Debug)
        {
            Console.WriteLine($"{DateTimeOffset.Now:dd.MM.yyyy HH:mm:ss.fff} {message.Method} {message.RequestUri}");
            if (message.Content != null)
                Console.WriteLine(await message.Content.ReadAsStringAsync());
        }

        //send and wait for result
        var response = await client.SendAsync(message);

        //read response content
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrEmpty(responseContent))
            return new TelegramApiResult<TResult>
            {
                Ok = false,
                Result = defaultValue,
                Description = string.Empty,
            };

        //if no content, return based on status code
        var result = new TelegramApiResult<TResult>
        {
            Ok = response.IsSuccessStatusCode,
            Result = defaultValue,
            Description = response.ReasonPhrase ?? string.Empty,
        };

        return result;

        //
        // //deserialize
        // return JsonConvert.DeserializeObject<TelegramApiResult<TResult>>(responseContent, new JsonSerializerSettings()
        // {
        //     ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }
        // });
    }

    private object[][] GetKeyboard(IReadOnlyList<string> labels)
    {
        var keyboard = new List<List<object>>();
        var i = 0;
        var width = GetKeyboardWidth(labels);
        var row = new List<object>();
        foreach (var label in labels)
        {
            if (i % width == 0)
            {
                row = new List<object>();
                keyboard.Add(row);
            }

            row.Add(new { text = label });
            i++;
        }

        return keyboard.Select(r => r.ToArray()).ToArray();
    }

    private uint GetKeyboardWidth(IReadOnlyList<string> labels)
    {
        double maxWidth = 40;
        double max = Math.Max(1, labels.Select(e => e.Length).Max());

        return (uint)Math.Ceiling(maxWidth / max);
    }
}
