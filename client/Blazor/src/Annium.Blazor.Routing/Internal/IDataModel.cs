using System.Collections.Generic;
using Annium.Net.Base;

namespace Annium.Blazor.Routing.Internal;

internal interface IDataModel
{
    IReadOnlyDictionary<string, object> ToParams(object data);
    T ToData<T>(IReadOnlyDictionary<string, object> parameters) where T : new();
    IReadOnlyDictionary<string, object> ToParams(UriQuery query);
    UriQuery ToQuery(IReadOnlyDictionary<string, object> parameters);
}