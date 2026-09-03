using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Integrations.Social.Telegram.Obsolete.Operations;

namespace Annium.Integrations.Social.Telegram.Obsolete.Processing;

public class TelegramMenu : ITelegramMenu
{
    private readonly IDictionary<IList<string>, Type> _menu = new Dictionary<IList<string>, Type>();
    private readonly IList<string> _path = new List<string>();
    private readonly IServiceProvider _provider;

    public TelegramMenu(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ITelegramMenu BeginCategory(string name)
    {
        _path.Add(name);

        return this;
    }

    public ITelegramMenu EndCategory()
    {
        if (_path.Count == 0)
            throw new InvalidOperationException("There's no open menu category to be closed");

        _path.RemoveAt(_path.Count - 1);

        return this;
    }

    public ITelegramMenu AddOperation<TOperation>()
        where TOperation : ITelegramOperation
    {
        var path = new List<string>(_path);
        var instance = _provider.Resolve<TOperation>();
        path.Add(instance.Description);

        // check for uniqueness
        if (_menu.Any(e => e.Key.Intersect(path).Count() == path.Count))
            throw new InvalidOperationException($"Menu path {string.Join(" -> ", path)} is already used");

        _menu.Add(path, instance.GetType());

        return this;
    }

    public async Task<ITelegramOperation> GetOperationAsync(
        int userId,
        ITelegramUserProcessor processor,
        CancellationToken token
    )
    {
        // while there are more than one option - prompt for them
        var options = _menu;
        do
        {
            var choices = GetChoices(options);
            var choice = await processor.PromptAsync("Выберите действие", choices, token, choices.First());
            options = FilterOptions(options, choice);
        } while (options.Count > 1);

        var type = options.First().Value;

        return (ITelegramOperation)_provider.Resolve(type);
    }

    private IReadOnlyList<string> GetChoices(IDictionary<IList<string>, Type> options) =>
        options.Select(e => e.Key.First()).Distinct().ToArray();

    private IDictionary<IList<string>, Type> FilterOptions(IDictionary<IList<string>, Type> source, string choice)
    {
        // select menu options, starting with given one path entry
        // if any option after filtration has empty path - operation found, return it as single entry
        return source
            .Where(e => e.Key.First() == choice)
            .ToDictionary(e => e.Key.Skip(1).ToList() as IList<string>, e => e.Value);
    }
}
