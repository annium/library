// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Annium.Blazor.Routing.Internal.Implementations;
// using Annium.Blazor.Routing.Internal.Implementations.Locations.Segments;
// using Annium.Blazor.Routing.Internal.Locations;
// using Annium.Core.Mapper;
//
// namespace Annium.Blazor.Routing.Internal.LocationsOld
// {
//     internal class LocationTemplate : LocationTemplateBase
//     {
//         public static LocationTemplate Parse(string template, IMapper mapper)
//         {
//             var segments = Helper.ParseTemplateParts(template)
//                 .Select<string, ILocationSegment>(x => new FixedLocationSegment(x))
//                 .ToArray();
//
//             return new LocationTemplate(segments, mapper);
//         }
//
//         private readonly IReadOnlyList<ILocationSegment> _segments;
//
//         private LocationTemplate(
//             IReadOnlyList<ILocationSegment> segments,
//             IMapper mapper
//         ) : base(segments, mapper)
//         {
//             _segments = segments;
//         }
//
//         public override LocationMatch Match(RawLocation rawLocation) => MatchBase(rawLocation);
//
//         public string GetPath() => string.Join(Constants.SEPARATOR, _segments.Select(x =>
//         {
//             if (x is FixedLocationSegment fs)
//                 return fs.Part;
//
//             throw new NotImplementedException($"Segment {x} is not supported");
//         }));
//     }
// }
