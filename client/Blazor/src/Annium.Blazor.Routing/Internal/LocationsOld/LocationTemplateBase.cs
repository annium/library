// using System;
// using System.Collections.Generic;
// using Annium.Blazor.Routing.Internal.Implementations.Locations.Segments;
// using Annium.Blazor.Routing.Internal.Locations;
// using Annium.Core.Mapper;
//
// namespace Annium.Blazor.Routing.Internal.LocationsOld
// {
//     internal abstract class LocationTemplateBase : ILocationTemplate
//     {
//         private readonly IMapper _mapper;
//         private readonly IReadOnlyList<ILocationSegment> _segments;
//
//         protected LocationTemplateBase(
//             IReadOnlyList<ILocationSegment> segments,
//             IMapper mapper
//         )
//         {
//             _segments = segments;
//             _mapper = mapper;
//         }
//
//         public abstract LocationMatch Match(RawLocation rawLocation);
//
//         protected LocationMatch MatchBase(RawLocation rawLocation)
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
//             return new LocationMatch(true, routeValues);
//         }
//
//         public override string ToString() => string.Join(Constants.SEPARATOR, _segments);
//     }
// }
