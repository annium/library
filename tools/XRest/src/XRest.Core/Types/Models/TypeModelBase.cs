using XRest.Core.Models;

namespace XRest.Core.Types.Models;

public abstract record TypeModelBase(Namespace Namespace, string Name, bool IsGeneric) : ITypeModel;