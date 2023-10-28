namespace Annium.Components.State.Forms;

public static class StatusFactory
{
    public static readonly StateStatus Default = new() { Value = Status.None, Message = string.Empty };

    public static StateStatus None(string message = "") => new() { Value = Status.None, Message = message };

    public static StateStatus Loading(string message = "") => new() { Value = Status.Loading, Message = message };

    public static StateStatus Validating(string message = "") => new() { Value = Status.Validating, Message = message };

    public static StateStatus Success(string message = "") => new() { Value = Status.Success, Message = message };

    public static StateStatus Error(string message = "") => new() { Value = Status.Error, Message = message };
}
