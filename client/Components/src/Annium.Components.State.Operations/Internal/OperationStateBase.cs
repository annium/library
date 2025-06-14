using System.Collections.Generic;
using System.Text;
using Annium.Components.State.Core;
using Annium.Data.Operations;
using Annium.Linq;

namespace Annium.Components.State.Operations.Internal;

/// <summary>
/// Base class for operation state implementations providing common functionality
/// </summary>
internal abstract class OperationStateBase : ObservableState, IOperationStateBase
{
    /// <summary>
    /// Gets a plain error message by joining all plain errors with semicolon separator
    /// </summary>
    public string PlainError => PlainErrors.Join("; ");

    /// <summary>
    /// Gets the collection of plain error messages
    /// </summary>
    public IReadOnlyCollection<string> PlainErrors { get; private set; } = [];

    /// <summary>
    /// Gets the dictionary of labeled error messages
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors { get; private set; } =
        new Dictionary<string, IReadOnlyCollection<string>>();

    /// <summary>
    /// Gets a value indicating whether the operation is in a valid state with no errors
    /// </summary>
    public bool IsOk => PlainErrors.Count == 0 && LabeledErrors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the operation has any errors
    /// </summary>
    public bool HasErrors => PlainErrors.Count > 0 || LabeledErrors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the operation is currently loading
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the operation has completed (either succeeded or failed)
    /// </summary>
    public bool IsLoaded => HasSucceed || HasFailed;

    /// <summary>
    /// Gets a value indicating whether the operation has succeeded
    /// </summary>
    public bool HasSucceed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the operation has failed
    /// </summary>
    public bool HasFailed { get; private set; }

    /// <summary>
    /// Starts the operation by setting it to loading state
    /// </summary>
    public void Start()
    {
        PlainErrors = [];
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = true;
        HasSucceed = false;
        HasFailed = false;
        NotifyChanged();
    }

    /// <summary>
    /// Gets a detailed string representation of the current error state
    /// </summary>
    /// <returns>A formatted string containing error information</returns>
    public string ErrorState()
    {
        var sb = new StringBuilder();

        if (PlainErrors.Count > 0)
        {
            sb.AppendLine($"{PlainErrors.Count} plain errors:");
            foreach (var error in PlainErrors)
                sb.AppendLine($"- {error}");
        }
        else
            sb.AppendLine("no plain errors");

        if (LabeledErrors.Count > 0)
        {
            sb.AppendLine($"{LabeledErrors.Count} labeled errors:");
            foreach (var (label, errors) in LabeledErrors)
            {
                sb.AppendLine($"- {label}:");
                foreach (var error in errors)
                    sb.AppendLine($"-- {error}");
            }
        }
        else
            sb.AppendLine("no labeled errors");

        return sb.ToString();
    }

    /// <summary>
    /// Internal method to mark the operation as successfully completed
    /// </summary>
    protected void SucceedInternal()
    {
        PlainErrors = [];
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = false;
        HasSucceed = true;
        HasFailed = false;
        NotifyChanged();
    }

    /// <summary>
    /// Internal method to mark the operation as failed with the provided result
    /// </summary>
    /// <param name="result">The result containing error information</param>
    protected void FailInternal(IResultBase result)
    {
        PlainErrors = result.PlainErrors;
        LabeledErrors = result.LabeledErrors;
        IsLoading = false;
        HasSucceed = false;
        HasFailed = true;
        NotifyChanged();
    }

    /// <summary>
    /// Internal method to reset the operation state to its initial state
    /// </summary>
    protected void ResetInternal()
    {
        PlainErrors = [];
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = false;
        HasSucceed = false;
        HasFailed = false;
        NotifyChanged();
    }
}
