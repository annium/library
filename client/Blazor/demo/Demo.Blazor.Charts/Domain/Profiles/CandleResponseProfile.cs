using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Models;
using Annium.Core.Mapper;
using NodaTime;

namespace Demo.Blazor.Charts.Domain.Profiles;

public class CandleResponseProfile : Profile
{
    public CandleResponseProfile()
    {
        Map<CandleResponse, IReadOnlyList<Candle>>(x => MapCandleResponse(x));
    }

    private IReadOnlyList<Candle> MapCandleResponse(CandleResponse x)
    {
        var candles = new List<Candle>(x.Moments.Length);

        for (var i = 0; i < x.Moments.Length; i++)
            candles.Add(
                new Candle(Instant.FromUnixTimeSeconds(x.Moments[i]), x.Opens[i], x.Highs[i], x.Lows[i], x.Closes[i])
            );

        return candles;
    }
}
