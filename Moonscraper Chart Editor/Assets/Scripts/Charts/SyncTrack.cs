public abstract class SyncTrack : SongObject
{
    public SyncTrack(uint _position) : base(_position) {}

    public override void Delete(bool update = true)
    {
        if (position != 0)
        {
            base.Delete(update);
            if (song != null)
                song.Remove(this, update);
        }
    }
}
