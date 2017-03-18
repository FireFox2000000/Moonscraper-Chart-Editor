using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour {
    ChartEditor editor;
    AudioSource clapSource;
    Vector3 initLocalPos;

    public AudioClip clap;

    uint nextClapPos = 0;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.FindCurrentEditor();
        clapSource = gameObject.AddComponent<AudioSource>();
        initLocalPos = transform.localPosition;
    }
    
	// Update is called once per frame
	void Update () {

        // Offset by audio calibration
        Vector3 pos = initLocalPos;
        pos.y += Song.TimeToWorldYPosition((float)Globals.audioCalibrationMS / 1000.0f * Globals.gameSpeed);
        transform.localPosition = pos;

        uint currentTickPos = editor.currentSong.WorldYPositionToChartPosition(gameObject.transform.position.y);
        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            if (currentTickPos >= nextClapPos)
            {
                if (Globals.metronomeActive)
                    clapSource.PlayOneShot(clap);

                nextClapPos += (uint)editor.currentSong.resolution;
            }
        }
        else
        {
            // Calculate the starting clap pos
            if (currentTickPos % editor.currentSong.resolution > 0 ? true : false)
                nextClapPos = (currentTickPos / (uint)editor.currentSong.resolution + 1) * (uint)editor.currentSong.resolution;
            else
                nextClapPos = (currentTickPos / (uint)editor.currentSong.resolution) * (uint)editor.currentSong.resolution;
        }
	}
}
