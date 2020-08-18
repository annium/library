namespace Annium.Components.State.Form
{
    public interface IStatusContainer
    {
        Status Status { get; }
        string Message { get; }
        void SetStatus(Status status);
        void SetStatus(Status status, string message);
    }
}