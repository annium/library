namespace Annium.Components.State.Forms;

/// <summary>
/// Represents a state status with an associated message.
/// </summary>
public struct StateStatus
{
    /// <summary>
    /// Gets or sets the status value.
    /// </summary>
    public Status Value { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the status.
    /// </summary>
    public string Message { get; set; }
}
