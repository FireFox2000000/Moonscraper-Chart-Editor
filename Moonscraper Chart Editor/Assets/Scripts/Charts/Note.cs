using UnityEngine;
using System;
using System.Collections;

public class Note : ChartObject 
{
    public uint sustain_length;
    public Fret_Type fret_type;
    public Note_Type note_type;
    public Special_Type special_type;
    public Flags flags;

    public NoteController controller = null;

    public Note(Song song, Chart chart, uint _position, 
                Fret_Type _fret_type, 
                uint _sustain = 0, 
                Flags _flags = Flags.NONE,
                Note_Type _note_type = Note_Type.STRUM, 
                Special_Type _special_type = Special_Type.NONE) : base(song, chart, _position)
    {
        sustain_length = _sustain;
        flags = _flags;
        fret_type = _fret_type;
        note_type = _note_type;
        special_type = _special_type;
    }

    public Note (Note note) : base(note.song, note.chart, note.position)
    {
        position = note.position;
        sustain_length = note.sustain_length;
        flags = note.flags;
        fret_type = note.fret_type;
        note_type = note.note_type;
        special_type = note.special_type;
    }

    public enum Fret_Type
    {
        GREEN = 0, RED = 1, YELLOW = 2, BLUE = 3, ORANGE = 4
    }

    public enum Note_Type
    {
        STRUM, HOPO, TAP
    }

    public enum Special_Type
    {
        NONE, STAR_POW, BATTLE
    }

    [Flags]
    public enum Flags
    {
        NONE = 0,
        FORCED = 1,
        TAP = 2
    }

    public bool forced
    {
        get
        {
            return (flags & Flags.FORCED) == Flags.FORCED;
        }
        set
        {
            if (value)
                flags = flags | Flags.FORCED;
            else
                flags = flags & ~Flags.FORCED;
        }
    }

    public override string GetSaveString()
    {    
        return Globals.TABSPACE + position + " = N " + (int)fret_type + " " + sustain_length + "\n";          // 48 = N 2 0
    }

    public string GetFlagsSaveString()
    {
        string saveString = string.Empty;

        if ((flags & Flags.FORCED) == Flags.FORCED)
            saveString += Globals.TABSPACE + position + " = N 5 0 \n";

        if ((flags & Flags.TAP) == Flags.TAP)
            saveString += Globals.TABSPACE + position + " = N 6 0 \n";

        return saveString;
    }

    public static bool operator == (Note a, Note b)
    {
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            else if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null))
                return true;
            else
                return false;
        }
        else
        {
            if (a.position == b.position && a.fret_type == b.fret_type)
                return true;
            else
                return false;
        }
    }

    public static bool operator !=(Note a, Note b)
    {
        return !(a == b);
    }

    public static bool operator < (Note a, Note b)
    {
        if (a.position < b.position)
            return true;
        else if (a.position == b.position)
        {
            if (a.fret_type < b.fret_type)
                return true;
        }

        return false;
    }

    public static bool operator > (Note a, Note b)
    {
        if (a != b)
            return !(a < b);
        else
            return false;
    }

    public static void groupAddFlags (Note[] notes, Flags flag)
    {
        for (int i = 0; i < notes.Length; ++i)
        {
            notes[i].flags = notes[i].flags | flag;
        }
    }

}
