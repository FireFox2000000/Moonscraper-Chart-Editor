using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActionHistory {
    int historyPoint = 0;

    public abstract class Action
    {
        SongObject[] songObjects;

        protected Action(SongObject[] _songObjects)
        {
            List<SongObject> objectsList = new List<SongObject>();

            foreach (SongObject songObject in _songObjects)
            {
                objectsList.Add(songObject.Clone());
            }

            songObjects = objectsList.ToArray();
        }

        public abstract void Revoke(Song song, Chart chart);
        public abstract void Invoke(Song song, Chart chart);
    }

    public class Add : Action
    {
        public Add(SongObject[] songObjects) : base(songObjects){}

        public override void Invoke(Song song, Chart chart)
        {
            throw new NotImplementedException();
        }

        public override void Revoke(Song song, Chart chart)
        {
            throw new NotImplementedException();
        }
    }

    public class Delete : Action
    {
        public Delete(SongObject[] songObjects) : base(songObjects){}

        public override void Invoke(Song song, Chart chart)
        {
            throw new NotImplementedException();
        }

        public override void Revoke(Song song, Chart chart)
        {
            throw new NotImplementedException();
        }
    }

    public class Modify : Action
    {
        public Modify(SongObject before, SongObject after) : base(new SongObject[]{ before, after }) {}

        public override void Invoke(Song song, Chart chart)
        {
            throw new NotImplementedException();
        }

        public override void Revoke(Song song, Chart chart)
        {
            throw new NotImplementedException();
        }
    }
}


