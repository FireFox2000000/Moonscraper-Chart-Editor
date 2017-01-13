using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActionHistory {
    int historyPoint;
    List<Action[]> actionList;

    public ActionHistory()
    {
        actionList = new List<Action[]>();
        historyPoint = -1;
    }

    public void Insert (Action[] action)
    {
        // Clear all actions above the history point
        actionList.RemoveRange(historyPoint + 1, actionList.Count - (historyPoint + 1));

        // Add the action in
        actionList.Add(action);
        ++historyPoint;
    }

    public void Insert(Action action)
    {
        Insert(new Action[] { action });
    }

    public void Undo(ChartEditor editor)
    {
        //Debug.Log(historyPoint);
        if (historyPoint >= 0)
        {
            for (int i = actionList[historyPoint].Length - 1; i >= 0; --i)
                actionList[historyPoint][i].Revoke(editor);

            --historyPoint;
        }
    }

    public void Redo(ChartEditor editor)
    {
        if (historyPoint + 1 < actionList.Count)
        {
            ++historyPoint;
            for (int i = 0; i < actionList[historyPoint].Length; ++i)
                actionList[historyPoint][i].Invoke(editor);

            Debug.Log(historyPoint);
        }
    }

    public abstract class Action
    {
        protected SongObject[] songObjects;

        protected Action(SongObject[] _songObjects)
        {
            songObjects = new SongObject[_songObjects.Length];
            
            for (int i = 0; i < _songObjects.Length; ++i)
            {
                songObjects[i] = _songObjects[i].Clone();
            }
        }

        public abstract void Revoke(ChartEditor editor);
        public abstract void Invoke(ChartEditor editor);
    }

    public class Add : Action
    {
        public Add(SongObject[] songObjects) : base(songObjects){}

        public override void Invoke(ChartEditor editor)
        {
            foreach (SongObject songObject in songObjects)
            {
                PlaceSongObject.AddObjectToCurrentEditor(songObject.Clone(), editor, false);
            }
            editor.currentChart.updateArrays();
            editor.currentSong.updateArrays();
        }

        public override void Revoke(ChartEditor editor)
        {
            new Delete(songObjects).Invoke(editor);
        }
    }

    public class Delete : Action
    {
        public Delete(SongObject[] songObjects) : base(songObjects){}

        public override void Invoke(ChartEditor editor)
        {
            foreach(SongObject songObject in songObjects)
            {
                SongObject foundSongObject;
                SongObject[] arrayToSearch;
                int arrayPos;

                // Find each item
                if (songObject.GetType().IsSubclassOf(typeof(ChartObject)) || songObject.GetType() == typeof(ChartObject))
                {
                    arrayToSearch = editor.currentChart.chartObjects;
                }
                else
                {
                    if (songObject.GetType().IsSubclassOf(typeof(Event)) || songObject.GetType() == typeof(Event))
                        arrayToSearch = editor.currentSong.events;
                    
                    else
                        arrayToSearch = editor.currentSong.syncTrack;
                }

                arrayPos = SongObject.FindObjectPosition(songObject, arrayToSearch);

                if (arrayPos == Globals.NOTFOUND)
                    continue;
                else
                    foundSongObject = arrayToSearch[arrayPos];

                if (foundSongObject.controller)
                    foundSongObject.controller.Delete(false);
            }

            editor.currentChart.updateArrays();
            editor.currentSong.updateArrays();
        }

        public override void Revoke(ChartEditor editor)
        {
            new Add(songObjects).Invoke(editor);
        }
    }

    public class Modify : Action
    {
        public SongObject before { get { return songObjects[0]; } set { songObjects[0] = value.Clone(); } }
        public SongObject after { get { return songObjects[1]; } set { songObjects[1] = value.Clone(); } }

        public Modify(SongObject before, SongObject after) : base(new SongObject[]{ before, after }) {}

        public override void Invoke(ChartEditor editor)
        {
            throw new NotImplementedException();
        }

        public override void Revoke(ChartEditor editor)
        {
            throw new NotImplementedException();
        }
    }
}


