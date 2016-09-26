public class Note {
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

    // Returns the exact time the note should be in the middle of the strikeline
    public static float strike_time(int note_pos, float song_bpm, float offset)
    {
        return note_pos / 192 * 60 / song_bpm + offset;
    }

    // Returns the distance from the strikeline a note should be
    public static float note_distance(float highway_speed, float elapsed_time, float note_time)
    {
        return highway_speed * (note_time - elapsed_time);
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
}
