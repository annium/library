namespace Annium.Components.State.Forms;

/// <summary>
/// Interface for managing status and messages in form components.
/// </summary>
public interface IStatusContainer
{
    /// <summary>
    /// Gets the current status of the container.
    /// </summary>
    Status Status { get; }

    /// <summary>
    /// Gets the current message associated with the status.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Sets the status of the container.
    /// </summary>
    /// <param name="status">The status to set.</param>
    void SetStatus(Status status);

    /// <summary>
    /// Sets the status and message of the container.
    /// </summary>
    /// <param name="status">The status to set.</param>
    /// <param name="message">The message to associate with the status.</param>
    void SetStatus(Status status, string message);
}
