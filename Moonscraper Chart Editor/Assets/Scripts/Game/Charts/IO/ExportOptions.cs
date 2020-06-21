// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
