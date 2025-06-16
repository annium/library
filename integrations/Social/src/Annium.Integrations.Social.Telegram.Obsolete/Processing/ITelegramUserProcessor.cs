using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public interface ITelegramUserProcessor
{
    Task<T> PromptAsync<T>(string question, CancellationToken token, T defaultValue, bool allowEmpty = false)
        where T : notnull;

    Task<T> PromptAsync<T>(
        string question,
        IReadOnlyList<T> options,
        CancellationToken token,
        T defaultValue,
        bool allowEmpty = false
    )
        where T : notnull;

    Task<T> PromptEnumAsync<T>(string question, CancellationToken token)
        where T : Enum;

    Task<T> PromptEnumAsync<T>(string question, CancellationToken token, T defaultValue)
        where T : Enum;

    Task<DateTime> PromptMonthAsync(
        string question,
        DateTime start,
        DateTime end,
        CancellationToken token,
        DateTime defaultValue,
        bool allowEmpty = false
    );

    Task<DateTime> PromptDateAsync(
        string question,
        DateTime start,
        DateTime end,
        CancellationToken token,
        DateTime defaultValue,
        bool allowEmpty = false
    );

    Task<DateTime> PromptTimeAsync(
        string question,
        DateTime start,
        TimeSpan interval,
        DateTime end,
        CancellationToken token,
        DateTime defaultValue,
        bool allowEmpty = false
    );
}
