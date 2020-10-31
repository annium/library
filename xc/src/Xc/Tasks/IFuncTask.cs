namespace Xc.Tasks
{
    internal interface IFuncTask<TR> : ITask
    {
        TR Execute();
    }

    internal interface IFuncTask<TR, T1> : ITask
    {
        TR Execute(T1 x1);
    }

    internal interface IFuncTask<TR, T1, T2> : ITask
    {
        TR Execute(T1 x1, T2 x2);
    }

    internal interface IFuncTask<TR, T1, T2, T3> : ITask
    {
        TR Execute(T1 x1, T2 x2, T3 x3);
    }
}