using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin : ScriptableObject {
    public AudioClip break0;
    public Texture2D background0;
    public Texture2D fretboard;
    public AudioClip clap;
    public AudioClip metronome;

    public Texture2D[] reg_strum = new Texture2D[6];
    public Texture2D[] reg_hopo = new Texture2D[6];
    public Texture2D[] reg_tap = new Texture2D[5];
    public Texture2D[] sp_strum = new Texture2D[6];
    public Texture2D[] sp_hopo = new Texture2D[6];
    public Texture2D[] sp_tap = new Texture2D[5];

    public Texture2D[] sustains = new Texture2D[5];
    public Material[] sustain_mats = new Material[6];

    public Texture2D[] fret_base = new Texture2D[5];
    public Texture2D[] fret_cover = new Texture2D[5];
    public Texture2D[] fret_release = new Texture2D[5];
    public Texture2D[] fret_press = new Texture2D[5];
    public Texture2D[] fret_anim = new Texture2D[5];

    public Texture2D fret_stem;
    public Texture2D hit_flames;
}
