using MoonscraperChartEditor.Song;

namespace Game.Misc
{
    public sealed class MismatchedPhraseEventError : EventValidationError
    {
        public MismatchedPhraseEventError(Event songEvent) : base(songEvent)
        {
        }
        
        public override string errorMessage => $"Unmatched global '{songEvent.title}' event!";
    }
}