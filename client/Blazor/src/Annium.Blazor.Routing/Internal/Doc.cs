// LocationTemplate.Match:
// - match string segments array to given array of IPathSegment
//   => LocationPath/ILocationPath, builds path segments list, can parse template, build path from params, extract params from path
//   => LocationParams/ILocationParams, builds path segments list, can parse template, build path from params, extract params from path
// Route<T>.Link:
// - split type properties by path / query
//   => ???
// - build path from data
//   => through ILocationPath
// - build query string from data
//   => LocationQuery/ILocationQuery, is created from list of properties
// Route<T>.GetParams:
// - extract path properties from segments array
//   => LocationPath/ILocationPath
// - extract query properties from query string
//   => LocationQuery/ILocationQuery
// - combine path and query properties into T
//   => Location/ILocation (built from Path and Query)
// Route<T>.IsAt:
// - validate path is matching given template
//   => LocationPath/ILocationPath -> returns params dictionary
// - validate path is matched
//   => LocationPath/ILocationPath
// - build path from data
// - build query dictionary

// Definitions: D (TData), P (params dict), Q (UriQuery); RL (RawLocation); LM (LocationMatch)
// THUS, tasks:
// - Swap D->P
// - Swap P->D
// - Swap P->Q
// - Swap Q->P
// - Parse template: ILocationPath
// - Parse TData into dictionary: ???
// - Split TData dictionary into Path/Query: ???
// - Convert TData D to params dict P: ???
// - Build path from template and params dict P: ???
// - Match path to template, returning params dict P: ???
// - Map route values to params dict: ???
// - Merge params P

// Workflow:
// IRouteBase.Match(RL raw):
// - LM plm = IRouteBase.Path.Match(raw.Segments)
// - if !plm.IsSuccess return LM.Empty
// - LM qlm = IRouteT.Query.Match(Q rl.Parameters) // LMq(true, P)
// - return plm.Merge(qlm)
// Route<T>.Link(T data):
// - P pathParams = IRouteT.PathModel.ToParams(data);
// - string path = IRouteBase.Path.Link(pathParams);
// - P queryParams = ILocation.QueryModel.ToParams(data);
// - Q query = ILocation.Query.Create(queryParams)
// - return path + query.ToString()
// Route.Link():
// - string path = ILocation.Path.Link(new P);
// - return path
// Route<T>.TryGetParams():
// - RL raw = RL.Parse(NavigationManager.Uri)
// - LM plm = ILocation.Path.Match(raw.Segments)
// - if !plm.IsSuccess return false
// - LM qlm = ILocation.Query.Match(Q rl.Parameters) // LMq(true, P)
// - LM lm =  plm.Merge(qlm)
// - return plm.Merge(qlm)
