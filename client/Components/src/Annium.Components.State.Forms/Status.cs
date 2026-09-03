namespace Annium.Components.State.Forms;

/// <summary>
/// Enumeration representing different status states for form components.
/// </summary>
public enum Status
{
    /// <summary>
    /// No specific status - the default state.
    /// </summary>
    None,

    /// <summary>
    /// Loading state - indicates an operation is in progress.
    /// </summary>
    Loading,

    /// <summary>
    /// Validating state - indicates validation is being performed.
    /// </summary>
    Validating,

    /// <summary>
    /// Success state - indicates a successful operation.
    /// </summary>
    Success,

    /// <summary>
    /// Error state - indicates an error has occurred.
    /// </summary>
    Error,
}
