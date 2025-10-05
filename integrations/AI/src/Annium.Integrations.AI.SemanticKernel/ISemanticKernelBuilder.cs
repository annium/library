using Annium.Core.DependencyInjection;

namespace Annium.Integrations.AI.SemanticKernel;

public interface ISemanticKernelBuilder
{
    IServiceContainer Container { get; }
}
