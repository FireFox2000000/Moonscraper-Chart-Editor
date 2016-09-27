using System;
using System.Collections;

public class Note : IComparer
{
    public int position, sustain;
    public Fret_Type fret_type;
    public Note_Type note_type;
    public Special_Type special_type;
    public bool forced;

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

    public static Fret_Type NoteNumberToFretType (int number)
    {
        switch (number)
        {
            case (0):
                return Fret_Type.GREEN;
            case (1):
                return Fret_Type.RED;
            case (2):
                return Fret_Type.YELLOW;
            case (3):
                return Fret_Type.BLUE;
            case (4):
                return Fret_Type.ORANGE;
            default:
                throw new System.Exception("Note number out of range");
        }
    }

    public static int FretTypeToNoteNumber(Fret_Type fretType)
    {
        switch (fretType)
        {
            case (Fret_Type.GREEN):
                return 0;
            case (Fret_Type.RED):
                return 1;
            case (Fret_Type.YELLOW):
                return 2;
            case (Fret_Type.BLUE):
                return 3;
            case (Fret_Type.ORANGE):
                return 4;
            default:
                return 0;
        }
    }

    public string GetSaveString()
    {
        string saveString = "";
        const string TABSPACE = "  ";
        
        saveString += TABSPACE + position + " = N " + FretTypeToNoteNumber(fret_type) + " " + sustain + "\n";          // 48 = N 2 0

        if (forced)
            saveString += TABSPACE + position + " = N 5 0 \n";

        if (note_type == Note_Type.TAP)
            saveString += TABSPACE + position + " = N 6 0 \n";

        // Still need to do star power, will probably do it independant of the note
        // 10752 = S 2 3072

        return saveString;
    }

    public int Compare(object x, object y)
    {
        Note a = (Note)x, b = (Note)y;

        if (a == b)
            return 0;
        else if (a < b)
            return -1;
        else
            return 1;
    }

    public static bool operator == (Note a, Note b)
    {
        if (a.position == b.position && a.fret_type == b.fret_type)
            return true;
        else
            return false;
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
