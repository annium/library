using Annium.Finance.Providers.Abstractions.Domain.Market.Operations;
using Annium.Finance.Providers.Abstractions.Domain.User.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Finance.Providers.Abstractions.Domain.Tests.Operations;

/// <summary>
/// Pins what a result that nothing constructed reads as.
/// </summary>
/// <remarks>
/// <para>
/// These are value types, which means they have an instance no factory produced: a field nobody assigned,
/// an element of a freshly allocated array, the <c>out</c> of a <c>TryRead</c> that returned false, a
/// <c>default</c> written to satisfy definite assignment. While they were classes that instance was
/// <c>null</c> and every read of it threw; now it is a valid instance and every read of it answers.
/// </para>
/// <para>
/// Two things make the answer safe, and both are one edit away from being undone by someone tidying up.
/// <c>None</c> is the first member of each status enum, so it is the zero a default carries — put
/// <c>Ok</c> back at zero and a result nobody filled in becomes a success carrying no data, which is the
/// exact shape this codebase has repeatedly found: something did not happen, and the code carried on as
/// though it had. And <c>Message</c> reads through a null check, because the backing field of a default
/// instance is null while the declared type says it is not.
/// </para>
/// </remarks>
public class DefaultResultTests
{
    /// <summary>
    /// A user result nothing constructed is not a success, and does not claim to carry data.
    /// </summary>
    [Fact]
    public void DefaultUserResult_IsNotSuccess()
    {
        // act
        var plain = default(UserResult);
        var withData = default(UserResult<string>);

        // assert
        plain.Status.Is(UserOperationStatus.None);
        plain.IsSuccess.IsFalse("a result nobody produced must never read as one that succeeded");
        plain.IsFailure.IsTrue();

        withData.Status.Is(UserOperationStatus.None);
        withData.IsSuccess.IsFalse();
        withData.IsFailure.IsTrue();
        withData.Data.IsDefault();
    }

    /// <summary>
    /// The same on the market half of the pair.
    /// </summary>
    [Fact]
    public void DefaultMarketResult_IsNotSuccess()
    {
        // act
        var plain = default(MarketResult);
        var withData = default(MarketResult<string>);

        // assert
        plain.Status.Is(MarketOperationStatus.None);
        plain.IsSuccess.IsFalse("a result nobody produced must never read as one that succeeded");
        plain.IsFailure.IsTrue();

        withData.Status.Is(MarketOperationStatus.None);
        withData.IsSuccess.IsFalse();
        withData.IsFailure.IsTrue();
        withData.Data.IsDefault();
    }

    /// <summary>
    /// The message of a default result is an empty string rather than the null its backing field holds —
    /// the property is declared non-nullable, and a caller formatting a failure must not be the one to find
    /// out otherwise.
    /// </summary>
    [Fact]
    public void DefaultResult_MessageIsEmpty_NotNull()
    {
        // assert - all four shapes, since each carries its own backing field
        default(UserResult).Message.Is(string.Empty);
        default(UserResult<string>).Message.Is(string.Empty);
        default(MarketResult).Message.Is(string.Empty);
        default(MarketResult<string>).Message.Is(string.Empty);
    }

    /// <summary>
    /// A default result renders without throwing, which is what a log line reporting an unexpected outcome
    /// depends on.
    /// </summary>
    [Fact]
    public void DefaultResult_RendersWithoutThrowing()
    {
        // assert
        default(UserResult).ToString().Is("None ()");
        default(MarketResult).ToString().Is("None ()");
    }

    /// <summary>
    /// <c>None</c> is the zero of both status enums.
    /// </summary>
    /// <remarks>
    /// This is the invariant the rest of this file rests on, and the one an ordinary-looking edit undoes:
    /// a default struct carries zero, so whichever member sits first is what a result nobody produced
    /// reports. The tests above would all still pass if <c>None</c> merely existed somewhere in the enum;
    /// only this one fails when it stops being first.
    /// </remarks>
    [Fact]
    public void None_IsZero_WhichIsWhatMakesADefaultResultSafe()
    {
        // assert
        ((int)UserOperationStatus.None).Is(0);
        ((int)MarketOperationStatus.None).Is(0);
    }
}
