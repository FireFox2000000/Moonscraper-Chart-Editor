using UnityEngine;
using System.Collections;

public class Note_Controller : MonoBehaviour {
    /* STATIC */
    public float highway_speed = 2;
    public Vector2 strike_pos = Vector2.zero;

    public Note note = new Note(50, Note.Fret_Type.GREEN);

    float strike_time;
    float song_start_time;

    // Update is called once per frame
    void Update()
    {
        move();
    }

	public void init (ref Song song, float start_time, Note _note) {
        note = _note;
        strike_time = Note.strike_time(note.position, song.bpm, song.offset);
        song_start_time = start_time;

        SpriteRenderer s_render = GetComponent<SpriteRenderer>(); 
        if (s_render)
        {
            switch (note.fret_type)
            {
                case (Note.Fret_Type.GREEN):
                    break;
                // Default case
            }
        }
	}

    float move ()
    {
        Vector2 newPos = strike_pos;
        //strike_pos.y = Note.note_distance(highway_speed, Time.realtimeSinceStartup - song_start_time, strike_time);
        strike_pos.y = Note.note_distance(highway_speed, 0, strike_time);

        transform.position = newPos;

        return strike_pos.y;
    }
}
