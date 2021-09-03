using Annium.Core.Primitives;

namespace Backuper.Api.Tools
{
    public class Namer
    {
        private readonly ITimeProvider _timeProvider;

        public Namer(
            ITimeProvider timeProvider
        )
        {
            _timeProvider = timeProvider;
        }

        public string GetName()
        {
            var ((year, month, day), (hour, min, _)) = _timeProvider.Now.InUtc().LocalDateTime;

            return $"{year:0000}.{month:00}.{day:00}_{hour:00}.{min:00}.dump";
        }
    }
}