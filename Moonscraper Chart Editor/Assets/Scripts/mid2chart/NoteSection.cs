namespace mid2chart {
    public class NoteSection : Event {

        public NoteSection(long tick, long sus) {
            this.tick = tick;
            this.sus = sus;
        }
    }
}