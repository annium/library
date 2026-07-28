using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Annium.Components.State.Forms.Extensions;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for the validation extension branches of object container state (throttled registration,
/// exception handling and in-flight validation cancellation).
/// </summary>
public class ValidationTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ValidationTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ValidationTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that the throttled UseValidator overload collapses a burst of rapid changes into a single
    /// validation run, executed against the last value set once the due time elapses.
    /// </summary>
    /// <remarks>
    /// The throttled overload uses Rx's default scheduler and cannot have a virtual/test scheduler injected
    /// through the public API, so this test relies on a small due time plus a deterministic
    /// <see cref="TaskCompletionSource{T}"/> signal for the primary wait, with a short settle delay
    /// afterwards to confirm no further validation runs follow. The settle delay is a best-effort guard,
    /// not the detection mechanism itself.
    /// </remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task UseValidator_WithDueTime_ThrottlesRapidChangesIntoSingleRun()
    {
        // arrange
        var factory = GetFactory();
        var validator = new TrackingValidator();
        var state = factory.CreateObject(Arrange());
        state.UseValidator(validator, TimeSpan.FromMilliseconds(20));

        // act
        for (var age = 1; age <= 5; age++)
            state.Set(new User { Name = "Max", Age = age });
        var validatedValue = await validator.FirstCallAsync;
        await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);

        // assert
        validator.CallCount.Is(1);
        validatedValue.Age.Is(5);
    }

    /// <summary>
    /// Tests that when the validator's ValidateAsync throws, GetValidationResultAsync's catch branch converts
    /// the exception into a plain (unlabeled) error, and Validate surfaces that plain error to EVERY child as
    /// Status.Error with the exception message (a throwing/failing validator must be visible, not silently
    /// swallowed as Status.None). No unhandled exception escapes the Changed subscription.
    /// </summary>
    [Fact]
    public void UseValidator_ValidatorThrows_SurfacesErrorToAllChildren()
    {
        // arrange
        var factory = GetFactory();
        var validator = new ThrowingValidator();
        var state = factory.CreateObject(Arrange());
        state.UseValidator(validator);

        // act
        state.Set(new User { Name = "Other", Age = 99 });

        // assert: the thrown validator's message is surfaced to every child as Status.Error
        state.HasStatus(Status.Error).IsTrue();
        state.IsStatus(Status.Error).IsTrue();
        state.AtAtomic(x => x.Name).Status.Is(Status.Error);
        state.AtAtomic(x => x.Name).Message.Is(ThrowingValidator.ErrorMessage);
        state.AtAtomic(x => x.Age).Status.Is(Status.Error);
        state.AtAtomic(x => x.Age).Message.Is(ThrowingValidator.ErrorMessage);

        // container remains usable (no unhandled exception escapes the Changed subscription)
        state.Set(new User { Name = "Third", Age = 12 });
        state.IsStatus(Status.Error).IsTrue();
    }

    /// <summary>
    /// Tests that when a new Changed notification arrives while a previous validation run is still
    /// in flight, the previous run's cancellation token is cancelled and, once it eventually completes,
    /// its result is discarded rather than overwriting the statuses applied by the newer run.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task UseValidator_NewChangeArrivesDuringValidation_CancelsAndDiscardsStaleResult()
    {
        // arrange
        var factory = GetFactory();
        var validator = new GatedValidator();
        var state = factory.CreateObject(Arrange());
        state.UseValidator(validator);

        // act
        // VSTHRD003: awaiting Task.Run-started tasks on purpose, to run two Set calls on separate
        // threads and deterministically race the extension's cancellation branch — test-only.
#pragma warning disable VSTHRD003
        var firstSet = Task.Run(() => state.Set(new User { Name = "First", Age = 30 }));
        await validator.FirstStartedAsync;

        var secondSet = Task.Run(() => state.Set(new User { Name = "Second", Age = 40 }));
        await secondSet;

        validator.ReleaseFirst();
        await firstSet;
#pragma warning restore VSTHRD003

        // assert - only the second (fresh) run's result was applied; the stale, cancelled first
        // run's result never overwrote it
        validator.CallCount.Is(2);
        state.AtAtomic(x => x.Name).Status.Is(Status.Error);
        state.AtAtomic(x => x.Name).Message.Is(GatedValidator.FreshError);
        state.AtAtomic(x => x.Age).Status.Is(Status.None);
    }

    /// <summary>
    /// Tests that a validator error keyed to a nested property path (e.g. "Address.City") is routed into the
    /// matching nested child container's atomic state, rather than being silently dropped.
    /// </summary>
    [Fact]
    public void UseValidator_NestedPropertyError_RoutedToNestedChild()
    {
        // arrange
        var factory = GetFactory();
        var validator = new NestedKeyValidator();
        var state = factory.CreateObject(
            new Person
            {
                Name = "Max",
                Address = new Address { City = "" },
            }
        );
        state.UseValidator(validator);

        // act: a real change triggers validation, which emits a labeled error keyed "Address.City"
        state.Set(
            new Person
            {
                Name = "Lex",
                Address = new Address { City = "NYC" },
            }
        );

        // assert: the nested error reaches the Address container's City atomic child
        var city = state.AtObject(x => x.Address).AtAtomic(x => x.City);
        city.Status.Is(Status.Error);
        city.Message.Is(NestedKeyValidator.CityError);

        // and: the unrelated top-level atomic (Name) is cleared to None
        state.AtAtomic(x => x.Name).Status.Is(Status.None);
    }

    /// <summary>
    /// Tests that a validator error keyed through a nested MAP value (e.g. "Depts.hr.Name") is routed into the
    /// map value's nested atomic child — exercising per-key routing into a non-object composite.
    /// </summary>
    [Fact]
    public void UseValidator_NestedMapValueError_RoutedToNestedChild()
    {
        // arrange
        var factory = GetFactory();
        var validator = new NestedMapKeyValidator();
        var state = factory.CreateObject(new Org { Depts = new Dictionary<string, Dept> { ["hr"] = new Dept() } });
        state.UseValidator(validator);

        // act: change a value to trigger validation, which emits a labeled error keyed "Depts.hr.Name"
        state.Set(new Org { Depts = new Dictionary<string, Dept> { ["hr"] = new Dept { Name = "HR" } } });

        // assert: the error routed through the map value's key ("hr") to its Name atomic child
        var name = state.AtMap(x => x.Depts).AtObject(x => x["hr"]).AtAtomic(x => x.Name);
        name.Status.Is(Status.Error);
        name.Message.Is(NestedMapKeyValidator.DeptError);
    }

    /// <summary>
    /// Tests that recursive nested validation does not storm a nested composite container's aggregate Changed:
    /// each intermediate composite is muted while its atomic descendants' statuses are set, so a component bound
    /// to a nested container is not notified once per descendant during a validation cycle.
    /// </summary>
    [Fact]
    public void UseValidator_NestedValidation_DoesNotStormNestedContainerNotifications()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var validator = new NestedKeyValidator();
        var state = factory.CreateObject(
            new Person
            {
                Name = "Max",
                Address = new Address { City = "x" },
            }
        );
        state.UseValidator(validator);
        var address = state.AtObject(x => x.Address);
        address.Changed.Subscribe(log.Add);

        // act: change ONLY the top-level Name (Address value unchanged) so any Address notification would come
        // solely from validation setting the nested City status
        state.Set(
            new Person
            {
                Name = "Lex",
                Address = new Address { City = "x" },
            }
        );

        // assert: the nested Address container is muted during validation → no per-descendant notification storm
        log.IsEmpty();
    }

    /// <summary>
    /// Creates a sample user object for testing
    /// </summary>
    /// <returns>A user object with sample data</returns>
    private User Arrange() => new() { Name = "Max", Age = 20 };

    /// <summary>
    /// Represents a user entity for testing purposes
    /// </summary>
    private class User
    {
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user age
        /// </summary>
        public int Age { get; set; }
    }

    /// <summary>
    /// Validator stub that records how many times it was invoked and completes a signal on the first call,
    /// used to observe when a throttled validation run actually executes.
    /// </summary>
    private sealed class TrackingValidator : IValidator<User>
    {
        /// <summary>
        /// Signal completed with the value passed to the first ValidateAsync invocation.
        /// </summary>
        private readonly TaskCompletionSource<User> _firstCall = new();

        /// <summary>
        /// Gets the total number of times ValidateAsync was invoked.
        /// </summary>
        public int CallCount => _callCount;

        /// <summary>
        /// The backing field for the invocation counter.
        /// </summary>
        private int _callCount;

        /// <summary>
        /// Gets a task that completes with the value used in the first ValidateAsync invocation.
        /// </summary>
        public Task<User> FirstCallAsync => _firstCall.Task;

        /// <summary>
        /// Records the invocation and immediately returns a successful result.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="label">The label for the validation context.</param>
        /// <returns>A successful validation result.</returns>
        public Task<IResult> ValidateAsync(User value, string label = "")
        {
            Interlocked.Increment(ref _callCount);
            _firstCall.TrySetResult(value);
            return Task.FromResult(Result.Create());
        }
    }

    /// <summary>
    /// Validator stub whose ValidateAsync always throws, used to exercise the catch branch of
    /// GetValidationResultAsync.
    /// </summary>
    private sealed class ThrowingValidator : IValidator<User>
    {
        /// <summary>
        /// The exception message surfaced by ValidateAsync.
        /// </summary>
        public const string ErrorMessage = "validator exploded";

        /// <summary>
        /// Always throws to simulate a failing validator.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="label">The label for the validation context.</param>
        /// <returns>Never returns; always throws.</returns>
        public Task<IResult> ValidateAsync(User value, string label = "") =>
            throw new InvalidOperationException(ErrorMessage);
    }

    /// <summary>
    /// Validator stub whose first invocation blocks until released by the test (simulating a slow,
    /// in-flight validation) while returning a stale, distinguishable error; subsequent invocations
    /// complete immediately with a different, distinguishable error. Used to exercise the cancellation
    /// branch that discards a stale, superseded validation run.
    /// </summary>
    private sealed class GatedValidator : IValidator<User>
    {
        /// <summary>
        /// The labeled error applied by the first (stale, expected-to-be-cancelled) invocation.
        /// </summary>
        public const string StaleError = "stale-should-not-apply";

        /// <summary>
        /// The labeled error applied by the second (fresh) invocation.
        /// </summary>
        public const string FreshError = "fresh-applied";

        /// <summary>
        /// Signal completed once the first invocation has started and is blocking.
        /// </summary>
        private readonly TaskCompletionSource _firstStarted = new();

        /// <summary>
        /// Signal used by the test to release the blocked first invocation.
        /// </summary>
        private readonly TaskCompletionSource _release = new();

        /// <summary>
        /// The backing field for the invocation counter.
        /// </summary>
        private int _callCount;

        /// <summary>
        /// Gets the total number of times ValidateAsync was invoked.
        /// </summary>
        public int CallCount => _callCount;

        /// <summary>
        /// Gets a task that completes once the first invocation has started blocking.
        /// </summary>
        public Task FirstStartedAsync => _firstStarted.Task;

        /// <summary>
        /// Releases the blocked first invocation, letting it complete with the stale error.
        /// </summary>
        public void ReleaseFirst() => _release.TrySetResult();

        /// <summary>
        /// Blocks on the first invocation until released, returning a stale error; returns a fresh,
        /// distinguishable error immediately on subsequent invocations.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="label">The label for the validation context.</param>
        /// <returns>A labeled error result, distinguishable per invocation.</returns>
        public async Task<IResult> ValidateAsync(User value, string label = "")
        {
            var call = Interlocked.Increment(ref _callCount);
            if (call == 1)
            {
                _firstStarted.TrySetResult();

                // VSTHRD003: awaiting a TaskCompletionSource signal released by the test, to simulate a
                // slow, in-flight validation that the test then supersedes and cancels — test-only.
#pragma warning disable VSTHRD003
                await _release.Task;
#pragma warning restore VSTHRD003

                return Result.Create().Error(nameof(User.Age), StaleError);
            }

            return Result.Create().Error(nameof(User.Name), FreshError);
        }
    }

    /// <summary>
    /// Test model with a nested composite (Address) child, used for nested-validation routing.
    /// </summary>
    private class Person
    {
        /// <summary>
        /// Gets or sets the person name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nested address.
        /// </summary>
        public Address Address { get; set; } = new();
    }

    /// <summary>
    /// Nested test model used as a composite child of <see cref="Person"/>.
    /// </summary>
    private class Address
    {
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validator stub that emits a single labeled error keyed to a nested property path ("Address.City"),
    /// simulating a nested/sub-validator's dotted-path output.
    /// </summary>
    private sealed class NestedKeyValidator : IValidator<Person>
    {
        /// <summary>
        /// The message emitted for the nested Address.City error.
        /// </summary>
        public const string CityError = "city required";

        /// <summary>
        /// Returns a result carrying a single labeled error keyed "Address.City".
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="label">The label for the validation context.</param>
        /// <returns>A result with the nested labeled error.</returns>
        public Task<IResult> ValidateAsync(Person value, string label = "") =>
            Task.FromResult<IResult>(Result.Create().Error("Address.City", CityError));
    }

    /// <summary>
    /// Test model with a nested map of composite values, used for nested-map validation routing.
    /// </summary>
    private class Org
    {
        /// <summary>
        /// Gets or sets the departments keyed by code.
        /// </summary>
        public Dictionary<string, Dept> Depts { get; set; } = new();
    }

    /// <summary>
    /// Nested test model used as a map value of <see cref="Org"/>.
    /// </summary>
    private class Dept
    {
        /// <summary>
        /// Gets or sets the department name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validator stub that emits a single labeled error keyed through a nested map value ("Depts.hr.Name").
    /// </summary>
    private sealed class NestedMapKeyValidator : IValidator<Org>
    {
        /// <summary>
        /// The message emitted for the nested Depts.hr.Name error.
        /// </summary>
        public const string DeptError = "dept name required";

        /// <summary>
        /// Returns a result carrying a single labeled error keyed "Depts.hr.Name".
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="label">The label for the validation context.</param>
        /// <returns>A result with the nested labeled error.</returns>
        public Task<IResult> ValidateAsync(Org value, string label = "") =>
            Task.FromResult<IResult>(Result.Create().Error("Depts.hr.Name", DeptError));
    }
}
