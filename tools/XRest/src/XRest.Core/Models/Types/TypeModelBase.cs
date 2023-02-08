namespace XRest.Core.Models.Types;

public abstract record TypeModelBase(Namespace Namespace, string Name, bool IsGeneric) : ITypeModel;