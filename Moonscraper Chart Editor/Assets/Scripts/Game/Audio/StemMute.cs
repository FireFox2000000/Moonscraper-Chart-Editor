// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperEngine;
using MoonscraperChartEditor.Song;

/// <summary>
/// Mutes and solos the song's audio while charting, as Audacity's Mute and Solo buttons do, without changing the volume settings.
/// When any audio is soloed, only soloed audio plays. Soloing audio unmutes it, and muting soloed audio silences it. Nothing is saved: everything plays again after a restart.
/// </summary>
public static class StemMute
{
    static readonly bool[] s_muted = new bool[EnumX<Song.AudioInstrument>.Count];
    static readonly bool[] s_soloed = new bool[EnumX<Song.AudioInstrument>.Count];

    public static bool IsMuted(Song.AudioInstrument audio)
    {
        return s_muted[(int)audio];
    }

    public static bool IsSoloed(Song.AudioInstrument audio)
    {
        return s_soloed[(int)audio];
    }

    public static void ToggleMute(Song.AudioInstrument audio)
    {
        s_muted[(int)audio] = !s_muted[(int)audio];
    }

    /// <summary>
    /// Unsolos this audio if it's soloed. Otherwise solos only this audio, and unmutes it so it plays.
    /// </summary>
    /// <param name="keepOthers">Keep other audio soloed too, as Shift-clicking Solo does in Audacity</param>
    public static void ToggleSolo(Song.AudioInstrument audio, bool keepOthers)
    {
        if (s_soloed[(int)audio])
        {
            s_soloed[(int)audio] = false;
            return;
        }

        if (!keepOthers)
        {
            for (int i = 0; i < s_soloed.Length; ++i)
            {
                s_soloed[i] = false;
            }
        }

        s_soloed[(int)audio] = true;
        s_muted[(int)audio] = false;
    }

    public static bool IsSilenced(Song.AudioInstrument audio)
    {
        if (s_muted[(int)audio])
            return true;

        foreach (bool soloed in s_soloed)
        {
            if (soloed)
                return !s_soloed[(int)audio];
        }

        return false;
    }

    /// <summary>
    /// The volume to play this audio at, given its volume setting.
    /// </summary>
    public static float GetVolume(Song.AudioInstrument audio, float volume)
    {
        return IsSilenced(audio) ? 0 : volume;
    }
}
