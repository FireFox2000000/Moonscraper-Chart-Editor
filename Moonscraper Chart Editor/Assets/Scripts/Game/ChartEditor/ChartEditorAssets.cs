// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChartEditorAssets : MonoBehaviour
{
    [Header("Song Object Prefabs")]
    public GameObject notePrefab;
    public GameObject starpowerPrefab;
    public GameObject sectionPrefab;
    public GameObject bpmPrefab;
    public GameObject tsPrefab;
    public GameObject songEventPrefab;
    public GameObject chartEventPrefab;

    [Header("Beat Line Prefabs")]
    public GameObject measureLine;
    public GameObject beatLine;
    public GameObject quarterBeatLine;

    [Header("Object Highlight Prefabs")]
    public GameObject hoverHighlight;
    public GameObject selectedHighlight;
}
