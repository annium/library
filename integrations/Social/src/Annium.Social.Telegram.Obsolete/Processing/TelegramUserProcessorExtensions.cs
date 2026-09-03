using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Social.Telegram.Obsolete.Models;

namespace Annium.Social.Telegram.Obsolete.Processing;

public static class TelegramUserProcessorExtensions
{
    public static async Task<bool> ConfirmAsync(
        this ITelegramUserProcessor processor,
        string question,
        CancellationToken token,
        bool? defaultValue = null
    )
    {
        var choice = await processor.PromptAsync(
            question,
            ["Да", "Нет"],
            token,
            defaultValue.HasValue ? (defaultValue.Value ? "Да" : "Нет") : string.Empty
        );

        return choice == "Да";
    }

    public static async Task<T> PromptAsync<T>(
        this ITelegramUserProcessor processor,
        string question,
        IReadOnlyList<T> options,
        string format,
        CancellationToken token,
        T defaultValue,
        bool allowEmpty = false
    )
        where T : ITelegramModel
    {
        var choices = options.Select(e => e.ToString(format)).ToArray();

        var choice = await processor.PromptAsync(question, choices, token, defaultValue.ToString(format), allowEmpty);

        return choices.Contains(choice) ? options.First(e => e.ToString(format) == choice) : defaultValue;
    }
}
