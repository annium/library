using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Validation;

namespace Annium.Components.State.Forms.Extensions;

public static class ObjectContainerValidationExtensions
{
    public static IObjectContainer<T> UseValidator<T>(this IObjectContainer<T> state, IValidator<T> validator)
        where T : notnull, new()
    {
        return state.Changed.SubscribeValidator(state, validator);
    }

    public static IObjectContainer<T> UseValidator<T>(
        this IObjectContainer<T> state,
        IValidator<T> validator,
        TimeSpan dueTime
    )
        where T : notnull, new()
    {
        return state.Changed.Throttle(dueTime).SubscribeValidator(state, validator);
    }

    private static IObjectContainer<T> SubscribeValidator<T>(
        this IObservable<Unit> observable,
        IObjectContainer<T> state,
        IValidator<T> validator
    )
        where T : notnull, new()
    {
        var cts = new CancellationTokenSource();
        observable.Subscribe(_ =>
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            state.Validate(validator, cts.Token);
        });

        return state;
    }

    private static void Validate<T>(this IObjectContainer<T> state, IValidator<T> validator, CancellationToken ct)
        where T : notnull, new()
    {
        var children = state
            .Children.Where(x => x.Value is IStatusContainer)
            .ToDictionary(x => x.Key, x => (IStatusContainer)x.Value);

        using (state.Mute())
        {
            foreach (var child in children.Values)
                child.SetStatus(Status.Validating);
        }

#pragma warning disable VSTHRD002
        var result = validator.GetValidationResultAsync(state).Result;
#pragma warning restore VSTHRD002
        if (ct.IsCancellationRequested)
            return;

        using (state.Mute())
        {
            foreach (var (name, child) in children)
                if (result.LabeledErrors.TryGetValue(name, out var errors))
                    child.SetStatus(Status.Error, string.Join("; ", errors));
                else
                    child.SetStatus(Status.None);
        }
    }

    private static async Task<IResult> GetValidationResultAsync<T>(
        this IValidator<T> validator,
        IObjectContainer<T> state
    )
        where T : notnull, new()
    {
        try
        {
            return await validator.ValidateAsync(state.Value);
        }
        catch (Exception exception)
        {
            return Result.New().Error(exception.Message);
        }
    }
}
