namespace mid2chart {
    public class Sync {
        public long tick, num;
        public bool isBPM;

        public Sync(long tick, long num, bool isBPM) {
            this.tick = tick;
            this.num = num;
            this.isBPM = isBPM;
        }
    }
}