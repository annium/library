using Annium.Core.DependencyInjection;

namespace Annium.Integrations.Ai.SemanticKernel.Internal;

internal sealed class SemanticKernelBuilder(IServiceContainer container) : ISemanticKernelBuilder
{
    public IServiceContainer Container { get; } = container;
}
