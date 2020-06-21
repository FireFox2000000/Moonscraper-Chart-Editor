// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public abstract class SyncTrack : SongObject
{
    public SyncTrack(uint _position) : base(_position) {}

    public override void Delete(bool update = true)
    {
        if (tick != 0)
        {
            base.Delete(update);
            if (song != null)
                song.Remove(this, update);
        }
    }
}
