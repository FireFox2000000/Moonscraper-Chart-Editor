using System.Collections.Generic;
using MoonscraperChartEditor.Song;
using Event = MoonscraperChartEditor.Song.Event;

namespace Game.Misc
{
    public static class PhraseValidator
    {
        public static IEnumerable<IValidationError> GetValidationErrors(Song song)
        {
            Event previousEvent = null;
            foreach (var songEvent in song.events)
            {
                // Ignores all non-phrase events.
                if (!(songEvent.title is LyricHelper.PhraseStartText || songEvent.title is LyricHelper.PhraseEndText))
                    continue;

                if (songEvent.title is LyricHelper.PhraseStartText
                    && previousEvent?.title is LyricHelper.PhraseStartText)
                {
                    yield return new MismatchedPhraseEventError(previousEvent);
                }

                // previousEvent is only ever null during the first iteration.
                if (songEvent.title is LyricHelper.PhraseEndText
                    && (previousEvent is null || previousEvent.title is LyricHelper.PhraseEndText))
                {
                    yield return new MismatchedPhraseEventError(songEvent);
                }

                if (songEvent.CollidesWith(previousEvent))
                {
                    yield return new PhraseCollisionError(songEvent);
                }

                previousEvent = songEvent;
            }

            if (previousEvent?.title is LyricHelper.PhraseStartText)
            {
                yield return new MismatchedPhraseEventError(previousEvent);
            }
        }
    }
}
