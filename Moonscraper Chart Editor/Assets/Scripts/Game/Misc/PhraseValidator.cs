using System.Collections.Generic;
using MoonscraperChartEditor.Song;
using Event = MoonscraperChartEditor.Song.Event;

namespace Game.Misc
{
    public static class PhraseValidator
    {
        private const string PhraseStart = "phrase_start";
        private const string PhraseEnd = "phrase_end";
        
        public static IEnumerable<IValidationError> GetValidationErrors(Song song)
        {
            Event previousEvent = null;
            foreach (var songEvent in song.events)
            {
                // Ignores all non-phrase events.
                if (!(songEvent.title is PhraseStart || songEvent.title is PhraseEnd)) continue;

                if (songEvent.title is PhraseStart
                    && previousEvent?.title is PhraseStart)
                {
                    yield return new MismatchedPhraseEventError(previousEvent);
                }

                // previousEvent is only ever null during the first iteration.
                if (songEvent.title is PhraseEnd
                    && (previousEvent is null || previousEvent.title is PhraseEnd))
                {
                    yield return new MismatchedPhraseEventError(songEvent);
                }

                if (songEvent.CollidesWith(previousEvent))
                {
                    yield return new PhraseCollisionError(songEvent);
                }

                previousEvent = songEvent;
            }

            if (previousEvent?.title is PhraseStart)
            {
                yield return new MismatchedPhraseEventError(previousEvent);
            }
        }
    }
}
