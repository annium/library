using System;
using System.Collections.Generic;
using System.Text;
using Annium.Components.State.Core;
using Annium.Data.Operations;
using Annium.Linq;

namespace Annium.Components.State.Operations.Internal;

internal abstract class OperationStateBase : ObservableState, IOperationStateBase
{
    public string PlainError => PlainErrors.Join("; ");
    public IReadOnlyCollection<string> PlainErrors { get; private set; } = [];
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors { get; private set; } =
        new Dictionary<string, IReadOnlyCollection<string>>();
    public bool IsOk => PlainErrors.Count == 0 && LabeledErrors.Count == 0;
    public bool HasErrors => PlainErrors.Count > 0 || LabeledErrors.Count > 0;
    public bool IsLoading { get; private set; }
    public bool IsLoaded => HasSucceed || HasFailed;
    public bool HasSucceed { get; private set; }
    public bool HasFailed { get; private set; }

    public void Start()
    {
        PlainErrors = [];
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = true;
        HasSucceed = false;
        HasFailed = false;
        NotifyChanged();
    }

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

    protected void SucceedInternal()
    {
        PlainErrors = [];
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = false;
        HasSucceed = true;
        HasFailed = false;
        NotifyChanged();
    }

    protected void FailInternal(IResultBase result)
    {
        PlainErrors = result.PlainErrors;
        LabeledErrors = result.LabeledErrors;
        IsLoading = false;
        HasSucceed = false;
        HasFailed = true;
        NotifyChanged();
    }

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
