// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SustainResources : ScriptableObject
{
    [Header("Sustain Colours (Line Renderer)")]
    public Material[] sustainColours = new Material[6];
    public Material[] ghlSustainColours = new Material[7];
}
