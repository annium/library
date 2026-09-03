using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Social.Telegram.Obsolete.Api;
using Annium.Social.Telegram.Obsolete.Api.Models;
using Annium.Social.Telegram.Obsolete.Models;

namespace Annium.Social.Telegram.Obsolete.Processing;

public class TelegramUserProcessor : ITelegramUserCommandProcessor
{
    private readonly ITelegramApi _api;
    private readonly int _userId;
    private readonly TelegramConfiguration _configuration;
    private event EventHandler<TelegramMessage> Message = delegate { };

    public TelegramUserProcessor(ITelegramApi api, int userId, TelegramConfiguration configuration)
    {
        _api = api;
        _userId = userId;
        _configuration = configuration;
    }

    public void HandleMessage(TelegramMessage message)
    {
        Message.Invoke(this, message);
    }

    public Task<T> PromptAsync<T>(string question, CancellationToken token, T defaultValue, bool allowEmpty = false)
        where T : notnull
    {
        var tcs = new TaskCompletionSource<T>();
        WrapTaskCompletionSource(tcs, CompletePrompt, token);

        SendMessage();

        return tcs.Task;

        void CompletePrompt(object? sender, TelegramMessage message)
        {
            try
            {
                var result = message.Text == _configuration.SkipMessage ? defaultValue : ConvertValue<T>(message.Text);
                Message -= CompletePrompt;
                tcs.SetResult(result);
            }
            catch
            {
                SendMessage();
            }
        }

        void SendMessage()
        {
            if (allowEmpty)
                _api.SendMessageAsync(_userId, question, [_configuration.SkipMessage]).GetAwaiter();
            else
                _api.SendMessageAsync(_userId, question).GetAwaiter();
        }
    }

    public Task<T> PromptAsync<T>(
        string question,
        IReadOnlyList<T> options,
        CancellationToken token,
        T defaultValue,
        bool allowEmpty = false
    )
        where T : notnull
    {
        var tcs = new TaskCompletionSource<T>();
        var choices = options.Select(e => e.ToString()?.Trim() ?? string.Empty).ToArray();
        var defaultPresented = choices.Contains(defaultValue.ToString()?.Trim() ?? string.Empty);

        WrapTaskCompletionSource(tcs, CompletePrompt, token);

        SendMessage();

        return tcs.Task;

        void CompletePrompt(object? _, TelegramMessage message)
        {
            var text =
                message.Text == _configuration.SkipMessage && (defaultPresented || allowEmpty)
                    ? defaultValue.ToString()?.Trim() ?? string.Empty
                    : message.Text;

            try
            {
                var selected = allowEmpty
                    ? options.FirstOrDefault(option => option.ToString()?.Trim() == text) ?? defaultValue
                    : options.First(option => option.ToString()?.Trim() == text);
                Message -= CompletePrompt;
                tcs.SetResult(selected);
            }
            catch
            {
                SendMessage();
            }
        }

        void SendMessage()
        {
            if (defaultPresented || allowEmpty)
                _api.SendMessageAsync(_userId, question, new[] { _configuration.SkipMessage }.Concat(choices).ToArray())
                    .GetAwaiter();
            else
                _api.SendMessageAsync(_userId, question, choices).GetAwaiter();
        }
    }

    public async Task<T> PromptEnumAsync<T>(string question, CancellationToken token)
        where T : Enum => await PromptEnumAsync<T>(question, token, default!, false);

    public async Task<T> PromptEnumAsync<T>(string question, CancellationToken token, T defaultValue)
        where T : Enum => await PromptEnumAsync(question, token, defaultValue, true);

    private async Task<T> PromptEnumAsync<T>(string question, CancellationToken token, T defaultValue, bool defaulted)
    {
        var type = typeof(T);
        var fields = type.GetTypeInfo().GetFields().Where(e => e.FieldType == type).ToArray();
        var values = Enum.GetValues(type).OfType<T>().ToArray();

        var map = new Dictionary<string, T>();
        for (var i = 0; i < fields.Length; i++)
            map[fields[i].GetCustomAttribute<LabelAttribute>()?.Label ?? fields[i].Name] = values[i];

        var defaultKey = defaulted
            ? map.Keys.ToArray()[Array.IndexOf(map.Values.ToArray(), defaultValue)]
            : string.Empty;
        var value = await PromptAsync(question, map.Keys.ToArray(), token, defaultKey, false);

        return map[value];
    }

    public async Task<DateTime> PromptMonthAsync(
        string question,
        DateTime start,
        DateTime end,
        CancellationToken token,
        DateTime defaultValue = default,
        bool allowEmpty = false
    )
    {
        start = new DateTime(start.Year, start.Month, 1, 0, 0, 0, 0);
        end = new DateTime(end.Year, end.Month, 1, 0, 0, 0, 0);
        defaultValue = new DateTime(defaultValue.Year, defaultValue.Month, 1, 0, 0, 0, 0);

        var options = new List<string>();
        while (start <= end)
        {
            options.Add(start.ToString(_configuration.MonthFormat));
            start = start.AddMonths(1);
        }

        var choice = await PromptAsync(
            question,
            options,
            token,
            defaultValue.ToString(_configuration.MonthFormat),
            allowEmpty
        );

        return choice == null ? defaultValue : ConvertValue<DateTime>(choice);
    }

    public async Task<DateTime> PromptDateAsync(
        string question,
        DateTime start,
        DateTime end,
        CancellationToken token,
        DateTime defaultValue = default,
        bool allowEmpty = false
    )
    {
        start = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, 0);
        end = new DateTime(end.Year, end.Month, end.Day, 0, 0, 0, 0);
        defaultValue = new DateTime(defaultValue.Year, defaultValue.Month, defaultValue.Day, 0, 0, 0, 0);

        var options = new List<string>();
        while (start <= end)
        {
            options.Add(start.ToString(_configuration.DateFormat));
            start = start.AddDays(1);
        }

        var choice = await PromptAsync(
            question,
            options,
            token,
            defaultValue.ToString(_configuration.DateFormat),
            allowEmpty
        );

        return choice == null ? defaultValue : ConvertValue<DateTime>(choice);
    }

    public async Task<DateTime> PromptTimeAsync(
        string question,
        DateTime start,
        TimeSpan interval,
        DateTime end,
        CancellationToken token,
        DateTime defaultValue = default,
        bool allowEmpty = false
    )
    {
        var options = new List<string>();
        var time = start;
        while (time >= start && time < end)
        {
            options.Add(time.ToString(_configuration.TimeFormat));
            time = time.Add(interval);
        }

        var choice = await PromptAsync(
            question,
            options,
            token,
            defaultValue.ToString(_configuration.TimeFormat),
            allowEmpty
        );

        return ConvertValue<DateTime>(choice);
    }

    private TaskCompletionSource<T> WrapTaskCompletionSource<T>(
        TaskCompletionSource<T> tcs,
        EventHandler<TelegramMessage> handler,
        CancellationToken token
    )
    {
        if (Message?.GetInvocationList().Length > 0)
            throw new InvalidOperationException("Some operation already running");

        Message += handler;
        token.Register(Cancel);

        return tcs;

        void Cancel()
        {
            Message -= handler;
            tcs.TrySetCanceled();
        }
    }

    private T ConvertValue<T>(string value)
    {
        if (typeof(T) == typeof(DateTime))
            return (T)(object)DateTime.Parse(value, _configuration.DateTimeFormat);

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
