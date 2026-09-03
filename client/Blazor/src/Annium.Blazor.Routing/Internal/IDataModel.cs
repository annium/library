using System.Collections.Generic;
using Annium.Net.Base;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Defines the contract for data model operations in routing, including conversions between objects, parameters, and URI queries.
/// </summary>
internal interface IDataModel
{
    /// <summary>
    /// Converts an object instance to a dictionary of parameter names and values.
    /// </summary>
    /// <param name="data">The object to convert to parameters.</param>
    /// <returns>A dictionary containing parameter names and their corresponding values.</returns>
    IReadOnlyDictionary<string, object?> ToParams(object data);

    /// <summary>
    /// Converts a dictionary of parameters to an object instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create and populate.</typeparam>
    /// <param name="parameters">The parameters to use for populating the object.</param>
    /// <returns>A new instance of type T populated with the parameter values.</returns>
    T ToData<T>(IReadOnlyDictionary<string, object?> parameters)
        where T : new();

    /// <summary>
    /// Converts a URI query to a dictionary of parameter names and values.
    /// </summary>
    /// <param name="query">The URI query to convert.</param>
    /// <returns>A dictionary containing parameter names and their corresponding values.</returns>
    IReadOnlyDictionary<string, object?> ToParams(UriQuery query);

    /// <summary>
    /// Converts a dictionary of parameters to a URI query.
    /// </summary>
    /// <param name="parameters">The parameters to convert to a URI query.</param>
    /// <returns>A URI query containing the parameter values.</returns>
    UriQuery ToQuery(IReadOnlyDictionary<string, object?> parameters);
}
