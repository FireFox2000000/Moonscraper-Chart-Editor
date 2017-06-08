using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ExportOptions {

    public bool forced;
    public Format format;
    public uint tickOffset;
    public float targetResolution;
    public bool copyDownEmptyDifficulty;

    public enum Format
    {
        Chart, Midi
    }

    public enum Game
    {
        PhaseShift, RockBand2, RockBand3
    }

}
