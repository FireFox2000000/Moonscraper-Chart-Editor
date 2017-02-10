namespace mid2chart {
    public class Section {
        public string name;
        public long tick;

        public Section(long tick, string name) {
            this.name = name;
            this.tick = tick;
        }
    }
}