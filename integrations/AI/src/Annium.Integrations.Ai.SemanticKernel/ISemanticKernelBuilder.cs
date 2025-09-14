using Annium.Core.DependencyInjection;

namespace Annium.Integrations.Ai.SemanticKernel;

public interface ISemanticKernelBuilder
{
    IServiceContainer Container { get; }
}
