namespace Annium.AI.SemanticKernel;

/// <summary>
/// Marks a type as a kernel plugin. Implementations are discovered by <c>WithPluginInstances</c> and exposed
/// to the kernel under their friendly type name, with their [KernelFunction] methods as functions.
/// </summary>
/// <remarks>
/// Discovery goes through Annium's type manager, so the assembly declaring the plugin must be scanned:
/// <c>AddRuntime</c> plus an <c>[assembly: AutoScanned]</c> attribute. The two halves fail differently —
/// without <c>AddRuntime</c> there is no type manager and <c>WithPluginInstances</c> throws
/// <see cref="System.InvalidOperationException"/> at the registration call; with it, but with the
/// declaring assembly unscanned, nothing is found and the kernel simply resolves with no plugins.
/// </remarks>
public interface ISemanticKernelPlugin;
