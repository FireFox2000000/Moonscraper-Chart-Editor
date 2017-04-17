using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class StrikelineAudioController : MonoBehaviour {

    public AudioClip clap;
    static AudioClip _clap;
    static AudioSource source;

    static float lastClapPos = -1;
    public static float startYPoint = -1;
    Vector3 initLocalPos;

    void Start()
    {
        source = GetComponent<AudioSource>();
        _clap = clap;
        initLocalPos = transform.localPosition;  
    }
    
    void Update()
    {
        Vector3 pos = initLocalPos;
        pos.y += 0.02f * Globals.hyperspeed / Globals.gameSpeed; // Song.TimeToWorldYPosition((float)Globals.audioCalibrationMS / 1000.0f);
        transform.localPosition = pos;

        if (Globals.applicationMode != Globals.ApplicationMode.Playing)
            lastClapPos = -1;
    }

    public static void Clap(float worldYPos)
    {
        if (worldYPos > lastClapPos && worldYPos >= startYPoint)
            source.PlayOneShot(_clap);
        lastClapPos = worldYPos;
    }
    /*
    void OnTriggerEnter2D (Collider2D col)
    {
        NoteController note = col.gameObject.GetComponentInParent<NoteController>();

        if (note != null && Globals.applicationMode == Globals.ApplicationMode.Playing && col.transform.position.y != lastClapPos && !note.isActivated)
        {
            switch (note.note.type)
            {
                case (Note.Note_Type.STRUM):
                    if ((Globals.clapSetting & Globals.ClapToggle.STRUM) == 0)
                        return;
                    break;
                case (Note.Note_Type.HOPO):
                    if ((Globals.clapSetting & Globals.ClapToggle.HOPO) == 0)
                        return;
                    break;
                case (Note.Note_Type.TAP):
                    if ((Globals.clapSetting & Globals.ClapToggle.TAP) == 0)
                        return;
                    break;
                default:
                    break;
            }
            
            source.PlayOneShot(clap);
            //Debug.Log("Played clap");

            lastClapPos = col.transform.position.y;
        }
    }

    bool modeSwitch = false;
    void OnTriggerStay2D (Collider2D col)
    {
        if (modeSwitch)
        {
            if (Globals.applicationMode == Globals.ApplicationMode.Editor)
                modeSwitch = false;
        }
        else
        {
            if (Globals.applicationMode == Globals.ApplicationMode.Playing)
            {
                modeSwitch = true;
                OnTriggerEnter2D(col);
            }
        }
    }*/
}
