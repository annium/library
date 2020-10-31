using System.Threading.Tasks;

namespace Xc.Tasks
{
    internal interface ITask
    {
        void Execute();
    }

    internal interface ITask<T>
    {
        T Execute();
    }
}