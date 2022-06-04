using MoonscraperChartEditor.Song;

namespace Game.Misc
{
    public abstract class EventValidationError : IValidationError
    {
        protected EventValidationError(Event songEvent)
        {
            this.songEvent = songEvent;
        }
        
        public Event songEvent { get; }
        
        public abstract string errorMessage { get; }
    }
}