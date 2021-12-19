using System.Collections.Generic;
using MoonscraperChartEditor.Song;
using Event = MoonscraperChartEditor.Song.Event;

namespace Game.Misc
{
    public static class PhraseValidator
    {
        public static List<IValidationError> ValidateIntegrity(Song song)
        {
            var validationErrors = new List<IValidationError>();
            Event previousEvent = null;
            foreach (var songEvent in song.eventsAndSections)
            {
                if (songEvent is Section) continue;

                switch (songEvent.title)
                {
                    case PhraseEvent.PhraseStart: {
                        if (PhraseEvent.PhraseStart.Equals(previousEvent?.title))
                        {
                            validationErrors.Add(new MismatchedPhraseEventError(previousEvent));
                        }

                        break;
                    }
                    case PhraseEvent.PhraseEnd: {
                        if (!PhraseEvent.PhraseStart.Equals(previousEvent?.title))
                        {
                            validationErrors.Add(new MismatchedPhraseEventError(songEvent));
                        }

                        break;
                    }
                    default:
                        continue;
                }
                
                if (songEvent.tick == previousEvent?.tick)
                {
                    validationErrors.Add(new PhraseCollisionError(songEvent));
                }

                previousEvent = songEvent;
            }

            if (PhraseEvent.PhraseStart.Equals(previousEvent?.title))
            {
                validationErrors.Add(new MismatchedPhraseEventError(previousEvent));
            }

            return validationErrors;
        }
    }
}
