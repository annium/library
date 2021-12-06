using System;
using System.Collections.Generic;
using Annium.Components.State.Core;
using Annium.Core.Primitives.Collections.Generic;
using Annium.Data.Operations;

namespace Annium.Components.State.Operations.Internal;

internal abstract class OperationStateBase : ObservableState, IOperationStateBase
{
    public string PlainError => PlainErrors.Join("; ");
    public IReadOnlyCollection<string> PlainErrors { get; private set; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors { get; private set; } = new Dictionary<string, IReadOnlyCollection<string>>();
    public bool IsOk => PlainErrors.Count == 0 && LabeledErrors.Count == 0;
    public bool HasErrors => PlainErrors.Count > 0 || LabeledErrors.Count > 0;
    public bool IsLoading { get; private set; }
    public bool IsLoaded => HasSucceed || HasFailed;
    public bool HasSucceed { get; private set; }
    public bool HasFailed { get; private set; }

    public void Start()
    {
        PlainErrors = Array.Empty<string>();
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = true;
        HasSucceed = false;
        HasFailed = false;
        NotifyChanged();
    }

    protected void SucceedInternal()
    {
        PlainErrors = Array.Empty<string>();
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
        PlainErrors = Array.Empty<string>();
        LabeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();
        IsLoading = false;
        HasSucceed = false;
        HasFailed = false;
        NotifyChanged();
    }
}