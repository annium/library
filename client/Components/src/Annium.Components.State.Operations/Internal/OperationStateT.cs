using Annium.Data.Operations;

namespace Annium.Components.State.Operations.Internal;

internal class OperationState<T> : OperationStateBase, IOperationState<T>
{
    public T Data { get; private set; } = default!;

    public void Succeed(T data)
    {
        Data = data;
        SucceedInternal();
    }

    public void Fail(IDataResultBase<T> result)
    {
        Data = result.Data;
        FailInternal(result);
    }

    public void Fail(IResultBase result)
    {
        Data = default!;
        FailInternal(result);
    }

    public void Reset()
    {
        Data = default!;
        ResetInternal();
    }
}
