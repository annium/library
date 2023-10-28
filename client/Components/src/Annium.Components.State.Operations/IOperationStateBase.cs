using Annium.Components.State.Core;
using Annium.Data.Operations;

namespace Annium.Components.State.Operations;

public interface IOperationStateBase : IObservableState, IResultBase
{
    bool IsLoading { get; }
    bool IsLoaded { get; }
    bool HasSucceed { get; }
    bool HasFailed { get; }
}
