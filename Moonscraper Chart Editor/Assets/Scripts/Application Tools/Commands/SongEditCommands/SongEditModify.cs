using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEditModify<T> : SongEditCommand where T : SongObject
{
    T before { get { return songObjects[0] as T; } }
    T after { get { return songObjects[1] as T; } }

    public SongEditModify(T before, T after) 
    {
        Debug.Assert(after.song == null, "Must add a new song object!");

        songObjects.Add(before.Clone());
        songObjects.Add(after);             // After should be a new object, take ownership to save allocation
    }

    public override void Invoke()
    {
        SongEditDelete.ApplyAction(before);
        SongEditAdd.ApplyAction(after);

        PostExecuteUpdate();
    }

    public override void Revoke()
    {
        SongEditDelete.ApplyAction(after);
        SongEditAdd.ApplyAction(before);

        PostExecuteUpdate();
    }
}
