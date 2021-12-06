namespace Xc.Tasks;

internal interface IActionTask : ITask
{
    void Execute();
}

internal interface IActionTask<T> : ITask
{
    void Execute(T x1);
}

internal interface IActionTask<T1, T2> : ITask
{
    void Execute(T1 x1, T2 x2);
}

internal interface IActionTask<T1, T2, T3> : ITask
{
    void Execute(T1 x1, T2 x2, T3 x3);
}