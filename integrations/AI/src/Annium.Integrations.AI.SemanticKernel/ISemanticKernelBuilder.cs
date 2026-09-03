using Annium.Core.DependencyInjection;

namespace Annium.Integrations.AI.SemanticKernel;

/// <summary>
/// Accumulates the registrations that make up a Semantic Kernel setup: AI services, plugin sources and
/// MCP-backed functions are added to the same container through the <c>With*</c> extension methods.
/// </summary>
public interface ISemanticKernelBuilder
{
    /// <summary>
    /// The container every registration made through this builder is added to.
    /// </summary>
    IServiceContainer Container { get; }
}
