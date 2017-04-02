using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNoteResources : ScriptableObject {
    public Texture2D fullAtlus;
    public Sprite[] reg_strum = new Sprite[6];
    public Sprite[] reg_hopo = new Sprite[6];
    public Sprite[] reg_tap = new Sprite[5];
    public Sprite[] sp_strum = new Sprite[6];
    public Sprite[] sp_hopo = new Sprite[6];
    public Sprite[] sp_tap = new Sprite[5];

    public Sprite[] sustains = new Sprite[5];
}
