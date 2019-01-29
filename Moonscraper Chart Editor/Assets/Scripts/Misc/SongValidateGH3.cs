// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongValidateGH3 {

    string ErrorMessage(Song song)
    {
        string message = string.Empty;

        if (message == string.Empty)
            message = "No errors detected";

        return message;
    }

    void FixSong(Song song)
    {
        // GH3 will crash if there are more than 100 sections
        if (song.sections.Count > 100)
        {
            for (int i = 100; i < song.sections.Count; ++i)
            {
                song.Remove(song.sections[i], false);
            }

            song.UpdateCache();
        }

        SongObjectPositionFix(song, song.syncTrack);
        SongObjectPositionFix(song, song.eventsAndSections);
    }

    string SongObjectPositionFix<T>(Song song, IList<T> songObjects) where T : SongObject
    {
        string errors = string.Empty;

        for (int i = songObjects.Count; i >= 0; --i)
        {
            if (songObjects[i].time > song.length)
            {
                errors += songObjects[i].ToString() + " at position " + songObjects[i].tick.ToString() + " is beyond the length of the song. " + Globals.LINE_ENDING;

                if (songObjects[i].GetType().IsSubclassOf(typeof(SyncTrack)))
                    song.Remove(songObjects[i] as SyncTrack);
                else if (songObjects[i].GetType().IsSubclassOf(typeof(Event)))
                    song.Remove(songObjects[i] as Event);
            }
            else
                break;
        }

        return errors;
    }

    string ChartObjectPositionFix<T>(Chart chart, T[] chartObjects) where T : ChartObject
    {
        string errors = string.Empty;

        for (int i = chartObjects.Length; i >= 0; --i)
        {
            errors += chartObjects[i].ToString() + " at position " + chartObjects[i].tick.ToString() + " is beyond the length of the song. " + Globals.LINE_ENDING;

            if (chartObjects[i].time > chart.song.length)
                chart.Remove(chartObjects[i]);
            else
                break;
        }

        return errors;
    }
}
