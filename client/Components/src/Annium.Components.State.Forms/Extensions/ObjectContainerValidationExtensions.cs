using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Validation;

namespace Annium.Components.State.Forms.Extensions;

/// <summary>
/// Provides extension methods for adding validation support to object containers.
/// </summary>
public static class ObjectContainerValidationExtensions
{
    /// <summary>
    /// Adds validation to an object container using the specified validator.
    /// Validation is triggered immediately when the container value changes.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="state">The object container to add validation to.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <returns>The same object container instance with validation enabled.</returns>
    public static IObjectContainer<T> UseValidator<T>(this IObjectContainer<T> state, IValidator<T> validator)
        where T : notnull, new()
    {
        return state.Changed.SubscribeValidator(state, validator);
    }

    /// <summary>
    /// Adds validation to an object container using the specified validator with throttling.
    /// Validation is triggered after the specified delay when the container value changes.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="state">The object container to add validation to.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <param name="dueTime">The delay before validation is triggered after value changes.</param>
    /// <returns>The same object container instance with throttled validation enabled.</returns>
    public static IObjectContainer<T> UseValidator<T>(
        this IObjectContainer<T> state,
        IValidator<T> validator,
        TimeSpan dueTime
    )
        where T : notnull, new()
    {
        return state.Changed.Throttle(dueTime).SubscribeValidator(state, validator);
    }

    /// <summary>
    /// Subscribes a validator to an observable stream to perform validation when events occur.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="observable">The observable stream to subscribe to.</param>
    /// <param name="state">The object container to validate.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <returns>The same object container instance.</returns>
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

    /// <summary>
    /// Performs validation on an object container and updates the status of child containers based on validation results.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="state">The object container to validate.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <param name="ct">The cancellation token to check for cancellation requests.</param>
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

    /// <summary>
    /// Gets the validation result for an object container's value, handling any exceptions that occur during validation.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="validator">The validator to use for validation.</param>
    /// <param name="state">The object container containing the value to validate.</param>
    /// <returns>A task representing the validation result.</returns>
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
