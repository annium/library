using Annium.Data.Operations;

namespace Annium.Components.State.Operations;

public interface IOperationState<T> : IOperationStateBase, IDataResultBase<T>
{
    void Start();
    void Succeed(T data);
    void Fail(IDataResultBase<T> result);
    void Fail(IResultBase result);
    void Reset();
}
