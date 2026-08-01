namespace Annium.Integrations.AI.SemanticKernel;

/// <summary>
/// Marks a type as a kernel plugin. Implementations are discovered by <c>WithPluginInstances</c> and exposed
/// to the kernel under their friendly type name, with their [KernelFunction] methods as functions.
/// </summary>
/// <remarks>
/// Discovery goes through Annium's type manager, so the assembly declaring the plugin must be scanned
/// (<c>AddRuntime</c> plus an <c>[assembly: AutoScanned]</c> attribute); otherwise no plugin is found.
/// </remarks>
public interface ISemanticKernelPlugin;
