// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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

    [Header("Pro drums")]
    public Sprite[] reg_cymbal = new Sprite[6];
    public Sprite[] sp_cymbal = new Sprite[6];

    [Header("GHL")]
    public Texture2D fullAtlusGhl;
    public Sprite[] reg_strum_ghl = new Sprite[3];
    public Sprite[] reg_hopo_ghl = new Sprite[3];
    public Sprite[] reg_tap_ghl = new Sprite[2];
    public Sprite[] sp_strum_ghl = new Sprite[3];
    public Sprite[] sp_hopo_ghl = new Sprite[3];
    public Sprite[] sp_tap_ghl = new Sprite[2];
}
