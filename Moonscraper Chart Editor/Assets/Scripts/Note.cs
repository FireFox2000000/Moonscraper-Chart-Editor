using System;
using System.Collections;

public class Note 
{
    public int position, sustain;
    public Fret_Type fret_type;
    public Note_Type note_type;
    public Special_Type special_type;
    public bool forced;

    public NoteController controller = null;

    public Note(int _position, 
                Fret_Type _fret_type, 
                int _sustain = 0, 
                bool _forced = false,
                Note_Type _note_type = Note_Type.NORMAL, 
                Special_Type _special_type = Special_Type.NONE)
    {
        position = _position;
        sustain = _sustain;
        forced = _forced;
        fret_type = _fret_type;
        note_type = _note_type;
        special_type = _special_type;
    }

    public Note (Note note)
    {
        position = note.position;
        sustain = note.sustain;
        forced = note.forced;
        fret_type = note.fret_type;
        note_type = note.note_type;
        special_type = note.special_type;
    }

    public enum Fret_Type
    {
        GREEN, RED, YELLOW, BLUE, ORANGE
    }

    public enum Note_Type
    {
        NORMAL, HOPO, TAP
    }

    public enum Special_Type
    {
        NONE, STAR_POW, BATTLE
    }

    public string GetSaveString()
    {
        string saveString = "";
        const string TABSPACE = "  ";
        
        saveString += TABSPACE + position + " = N " + fret_type + " " + sustain + "\n";          // 48 = N 2 0

        if (forced)
            saveString += TABSPACE + position + " = N 5 0 \n";

        if (note_type == Note_Type.TAP)
            saveString += TABSPACE + position + " = N 6 0 \n";

        // Still need to do star power, will probably do it independant of the note
        // 10752 = S 2 3072

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
}
