using Annium.Data.Operations;

namespace Annium.Components.State.Operations
{
    public interface IOperationState : IOperationStateBase
    {
        void Start();
        void Succeed();
        void Fail(IResultBase result);
        void Reset();
    }
}