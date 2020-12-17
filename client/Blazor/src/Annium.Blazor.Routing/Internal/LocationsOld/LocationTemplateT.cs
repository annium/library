// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using System.Text.RegularExpressions;
// using Annium.Blazor.Routing.Internal.Implementations;
// using Annium.Blazor.Routing.Internal.Implementations.Locations.Segments;
// using Annium.Blazor.Routing.Internal.Locations;
// using Annium.Core.Mapper;
// using Annium.Core.Primitives;
// using Annium.Net.Base;
// using Microsoft.AspNetCore.Http.Extensions;
//
// namespace Annium.Blazor.Routing.Internal.LocationsOld
// {
//     internal class LocationTemplate<TPageData> : ILocationTemplate
//     {
//         private static readonly Regex ParamRe = new Regex(@"^\{([A-z0-9]+)\}$", RegexOptions.Compiled | RegexOptions.Singleline);
//
//         private static readonly IReadOnlyDictionary<string, PropertyInfo> Params = typeof(TPageData)
//             .GetProperties()
//             .Where(x => x.CanWrite)
//             .ToPropertiesDictionary();
//
//         public static LocationTemplate<TPageData> Parse(string template, IMapper mapper)
//         {
//             var pathParams = new Dictionary<string, PropertyInfo>();
//             var queryParams = Params.ToDictionary(x => x.Key, x => x.Value);
//             var segments = Helper.ParseTemplateParts(template)
//                 .Select<string, ILocationSegment>(x =>
//                 {
//                     var match = ParamRe.Match(x);
//                     if (!match.Success)
//                         return new FixedLocationSegment(x);
//
//                     var name = match.Groups[1].Value;
//                     if (!queryParams.Remove(name, out var property))
//                         return new FixedLocationSegment(x);
//
//                     pathParams[name] = property;
//                     return new ParamLocationSegment(name, property.PropertyType);
//                 })
//                 .ToArray();
//
//             return new LocationTemplate<TPageData>(mapper, segments, pathParams, queryParams);
//         }
//
//         private readonly IMapper _mapper;
//         private readonly IReadOnlyList<ILocationSegment> _segments;
//         private readonly IReadOnlyDictionary<string, PropertyInfo> _pathParams;
//         private readonly IReadOnlyDictionary<string, PropertyInfo> _queryParams;
//
//         public LocationTemplate(
//             IMapper mapper,
//             IReadOnlyList<ILocationSegment> segments,
//             IReadOnlyDictionary<string, PropertyInfo> pathParams,
//             IReadOnlyDictionary<string, PropertyInfo> queryParams
//         )
//         {
//             _mapper = mapper;
//             _segments = segments;
//             _pathParams = pathParams;
//             _queryParams = queryParams;
//         }
//
//         public LocationMatch Match(RawLocation rawLocation)
//         {
//             if (rawLocation.Segments.Count != _segments.Count)
//                 return LocationMatch.Empty;
//
//             var routeValues = new Dictionary<string, object>();
//             for (var i = 0; i < _segments.Count; i++)
//             {
//                 var segment = _segments[i];
//                 var raw = rawLocation.Segments[i];
//
//                 switch (segment)
//                 {
//                     case FixedLocationSegment fixedSegment:
//                         if (!fixedSegment.Match(raw))
//                             return LocationMatch.Empty;
//                         break;
//                     case ParamLocationSegment paramSegment:
//                         var value = paramSegment.Match(raw, _mapper);
//                         if (value is null)
//                             return LocationMatch.Empty;
//                         routeValues[paramSegment.Name] = value;
//                         break;
//                     default:
//                         throw new NotImplementedException();
//                 }
//             }
//
//             foreach (var (key, values) in rawLocation.Parameters.Where(x => Params.ContainsKey(x.Key) && !routeValues.ContainsKey(x.Key)))
//             {
//                 var type = Params[key].PropertyType;
//
//                 try
//                 {
//                     var value = type.IsEnumerable() ? _mapper.Map(values.ToArray(), type) : _mapper.Map(values.FirstOrDefault()!, type);
//                     routeValues[key] = value;
//                 }
//                 catch
//                 {
//                     // ignored
//                 }
//             }
//
//             return new LocationMatch(true, routeValues);
//         }
//
//         public string GetPath(TPageData data)
//         {
//             var path = string.Join(Constants.SEPARATOR, _segments.Select(x =>
//             {
//                 if (x is FixedLocationSegment fs)
//                     return fs.Part;
//                 if (x is ParamLocationSegment ps)
//                     return _mapper.Map<string>(_pathParams[ps.Name].GetValue(data) ?? _pathParams[ps.Name].PropertyType.DefaultValue()!);
//
//                 throw new NotImplementedException($"Segment {x} is not supported");
//             }));
//
//             var qb = new QueryBuilder();
//             foreach (var (name, property) in _queryParams)
//                 qb.Add(name, property.GetValue(data)?.ToString() ?? string.Empty);
//             throw new NotImplementedException();
//             // qb.Param(name, property.GetValue(data));
//             //
//             // return qb.Build().PathAndQuery;
//         }
//
//         public string GetQuery(TPageData data)
//         {
//             var path = string.Join(Constants.SEPARATOR, _segments.Select(x =>
//             {
//                 if (x is FixedLocationSegment fs)
//                     return fs.Part;
//                 if (x is ParamLocationSegment ps)
//                     return _mapper.Map<string>(_pathParams[ps.Name].GetValue(data) ?? _pathParams[ps.Name].PropertyType.DefaultValue()!);
//
//                 throw new NotImplementedException($"Segment {x} is not supported");
//             }));
//
//             var qb = UriFactory.Base("http://localhost").Path(path);
//             foreach (var (name, property) in _queryParams)
//                 qb.Param(name, property.GetValue(data));
//
//             return qb.Build().PathAndQuery;
//         }
//
//         public string GetLink(TPageData data)
//         {
//             var path = GetPath(data);
//
//             var qb = UriFactory.Base("http://localhost").Path(path);
//             foreach (var (name, property) in _queryParams)
//                 qb.Param(name, property.GetValue(data));
//
//             return qb.Build().PathAndQuery;
//         }
//
//         public override string ToString() => string.Join("/", _segments);
//     }
// }