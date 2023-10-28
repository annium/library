using Annium.Data.Operations;

namespace Annium.Components.State.Operations.Internal;

internal class OperationState : OperationStateBase, IOperationState
{
    public void Succeed()
    {
        SucceedInternal();
    }

    public void Fail(IResultBase result)
    {
        FailInternal(result);
    }

    public void Reset()
    {
        ResetInternal();
    }
}
