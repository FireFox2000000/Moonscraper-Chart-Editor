using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActionHistory
{
    int historyPoint;
    List<Action[]> actionList;
    List<int> timestamps;

    public bool canUndo { get { return historyPoint >= 0; } }
    public bool canRedo { get { return historyPoint + 1 < actionList.Count; } }

    public ActionHistory()
    {
        actionList = new List<Action[]>();
        timestamps = new List<int>();
        historyPoint = -1;
    }

    public void Insert(Action[] action)
    {
        // Clear all actions above the history point
        actionList.RemoveRange(historyPoint + 1, actionList.Count - (historyPoint + 1));
        timestamps.RemoveRange(historyPoint + 1, actionList.Count - (historyPoint + 1));

        // Add the action in
        actionList.Add(action);
        timestamps.Add(Time.frameCount);
        ++historyPoint;
    }

    public void Insert(Action action)
    {
        Insert(new Action[] { action });
    }

    public bool Undo(ChartEditor editor)
    {
        if (canUndo)
        {
            ChartEditor.editOccurred = true;
            int frame = timestamps[historyPoint];

            while (historyPoint >= 0 && timestamps[historyPoint] == frame)
            {
                for (int i = actionList[historyPoint].Length - 1; i >= 0; --i)
                    actionList[historyPoint][i].Revoke(editor);

                --historyPoint;
            }

            editor.currentChart.updateArrays();
            editor.currentSong.updateArrays();
            editor.currentSelectedObject = null;

            return true;
        }

        return false;
    }

    public bool Redo(ChartEditor editor)
    {
        if (canRedo)
        {
            ChartEditor.editOccurred = true;
            int frame = timestamps[historyPoint + 1];

            while (historyPoint + 1 < actionList.Count && timestamps[historyPoint + 1] == frame)
            {
                ++historyPoint;
                for (int i = 0; i < actionList[historyPoint].Length; ++i)
                    actionList[historyPoint][i].Invoke(editor);
            }

            editor.currentChart.updateArrays();
            editor.currentSong.updateArrays();
            editor.currentSelectedObject = null;

            return true;
        }

        return false;
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
        public Add(SongObject[] songObjects) : base(songObjects) { }
        public Add(SongObject songObjects) : base(new SongObject[] { songObjects }) { }

        public override void Invoke(ChartEditor editor)
        {
            foreach (SongObject songObject in songObjects)
            {
                PlaceSongObject.AddObjectToCurrentEditor(songObject.Clone(), editor, false);
            }
        }

        public override void Revoke(ChartEditor editor)
        {
            new Delete(songObjects).Invoke(editor);
        }
    }

    public class Delete : Action
    {
        public Delete(SongObject[] songObjects) : base(songObjects) { }
        public Delete(SongObject songObjects) : base(new SongObject[] { songObjects }) { }

        public override void Invoke(ChartEditor editor)
        {
            foreach (SongObject songObject in songObjects)
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

                foundSongObject.Delete(false);
            }
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

        public Modify(SongObject before, SongObject after) : base(new SongObject[] { before, after }) { }

        public override void Invoke(ChartEditor editor)
        {
            new Delete(before).Invoke(editor);
            new Add(after).Invoke(editor);
        }

        public override void Revoke(ChartEditor editor)
        {
            new Delete(after).Invoke(editor);
            new Add(before).Invoke(editor);
        }
    }
}


