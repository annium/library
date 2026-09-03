namespace Annium.Components.State.Forms;

/// <summary>
/// Factory class for creating StateStatus instances with predefined status values.
/// </summary>
public static class StatusFactory
{
    /// <summary>
    /// Gets the default state status with None status and empty message.
    /// </summary>
    public static readonly StateStatus Default = new() { Value = Status.None, Message = string.Empty };

    /// <summary>
    /// Creates a StateStatus with None status.
    /// </summary>
    /// <param name="message">The optional message to associate with the status.</param>
    /// <returns>A StateStatus with None status and the specified message.</returns>
    public static StateStatus None(string message = "") => new() { Value = Status.None, Message = message };

    /// <summary>
    /// Creates a StateStatus with Loading status.
    /// </summary>
    /// <param name="message">The optional message to associate with the status.</param>
    /// <returns>A StateStatus with Loading status and the specified message.</returns>
    public static StateStatus Loading(string message = "") => new() { Value = Status.Loading, Message = message };

    /// <summary>
    /// Creates a StateStatus with Validating status.
    /// </summary>
    /// <param name="message">The optional message to associate with the status.</param>
    /// <returns>A StateStatus with Validating status and the specified message.</returns>
    public static StateStatus Validating(string message = "") => new() { Value = Status.Validating, Message = message };

    /// <summary>
    /// Creates a StateStatus with Success status.
    /// </summary>
    /// <param name="message">The optional message to associate with the status.</param>
    /// <returns>A StateStatus with Success status and the specified message.</returns>
    public static StateStatus Success(string message = "") => new() { Value = Status.Success, Message = message };

    /// <summary>
    /// Creates a StateStatus with Error status.
    /// </summary>
    /// <param name="message">The optional message to associate with the status.</param>
    /// <returns>A StateStatus with Error status and the specified message.</returns>
    public static StateStatus Error(string message = "") => new() { Value = Status.Error, Message = message };
}
