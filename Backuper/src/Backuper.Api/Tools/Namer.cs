using System;
using NodaTime;

namespace Backuper.Api.Tools
{
    public class Namer
    {
        private readonly Func<Instant> getInstant;

        public Namer(
            Func<Instant> getInstant
        )
        {
            this.getInstant = getInstant;
        }

        public string GetName()
        {
            var((year, month, day), (hour, min, _)) = getInstant().InUtc().LocalDateTime;

            return $"{year}.{month}.{day}_{hour}.{min}.dump";
        }
    }
}