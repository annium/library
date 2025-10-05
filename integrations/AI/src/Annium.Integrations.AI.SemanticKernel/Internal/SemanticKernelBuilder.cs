using Annium.Core.DependencyInjection;

namespace Annium.Integrations.AI.SemanticKernel.Internal;

internal sealed class SemanticKernelBuilder(IServiceContainer container) : ISemanticKernelBuilder
{
    public IServiceContainer Container { get; } = container;
}
