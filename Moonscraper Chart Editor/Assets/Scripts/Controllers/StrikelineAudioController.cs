using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class StrikelineAudioController : MonoBehaviour {

    public AudioClip clap;
    AudioSource source;
    public MovementController movement;

    float lastClapPos = -1;
    Vector3 initLocalPos;

    void Start()
    {
        source = GetComponent<AudioSource>();

        initLocalPos = transform.localPosition;
    }

    void Update()
    {
        Vector3 pos = initLocalPos;
        pos.y += Song.TimeToWorldYPosition((float)Globals.audioCalibrationMS / 1000.0f);
        transform.localPosition = pos;
    }

    void OnTriggerEnter2D (Collider2D col)
    {
        NoteController note = col.gameObject.GetComponentInParent<NoteController>();

        if (note != null && movement.applicationMode == MovementController.ApplicationMode.Playing && col.transform.position.y != lastClapPos)
        {
            switch (note.note.note_type)
            {
                case (Note.Note_Type.STRUM):
                    if ((Globals.clapToggle & Globals.ClapToggle.STRUM) == 0)
                        return;
                    break;
                case (Note.Note_Type.HOPO):
                    if ((Globals.clapToggle & Globals.ClapToggle.HOPO) == 0)
                        return;
                    break;
                case (Note.Note_Type.TAP):
                    if ((Globals.clapToggle & Globals.ClapToggle.TAP) == 0)
                        return;
                    break;
                default:
                    break;
            }

            source.PlayOneShot(clap);

            lastClapPos = col.transform.position.y;
        }
    }
}
