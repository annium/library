namespace Annium.Components.State
{
    public static class StatusFactory
    {
        public static readonly StateStatus Default = new StateStatus { Value = Status.None, Message = string.Empty };
        public static StateStatus None(string message = "") => new StateStatus { Value = Status.None, Message = message };
        public static StateStatus Loading(string message = "") => new StateStatus { Value = Status.Loading, Message = message };
        public static StateStatus Validating(string message = "") => new StateStatus { Value = Status.Validating, Message = message };
        public static StateStatus Success(string message = "") => new StateStatus { Value = Status.Success, Message = message };
        public static StateStatus Error(string message = "") => new StateStatus { Value = Status.Error, Message = message };
    }
}