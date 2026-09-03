using Annium.Core.DependencyInjection;

namespace Annium.Integrations.AI.SemanticKernel.Internal;

/// <summary>
/// Default <see cref="ISemanticKernelBuilder"/> implementation, holding the container the registrations
/// are written to.
/// </summary>
/// <param name="container">The container to register into.</param>
internal sealed class SemanticKernelBuilder(IServiceContainer container) : ISemanticKernelBuilder
{
    /// <summary>
    /// The container every registration made through this builder is added to.
    /// </summary>
    public IServiceContainer Container { get; } = container;
}
