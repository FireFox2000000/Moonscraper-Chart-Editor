using MoonscraperChartEditor.Song;

namespace Game.Misc
{
    public class PhraseCollisionError : EventValidationError
    {
        public PhraseCollisionError(Event songEvent) : base(songEvent)
        {
        }

        public override string errorMessage => $"Global '{songEvent.title}' event collides with another global phrase event!";
    }
}