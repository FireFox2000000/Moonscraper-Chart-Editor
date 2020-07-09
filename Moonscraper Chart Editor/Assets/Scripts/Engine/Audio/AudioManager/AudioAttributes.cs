// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

#if BASS_AUDIO
using Un4seen.Bass;
#endif

namespace MoonscraperEngine.Audio
{
    public enum AudioAttributes
    {
#if BASS_AUDIO

        Volume = BASSAttribute.BASS_ATTRIB_VOL,
        Pan = BASSAttribute.BASS_ATTRIB_PAN,

#endif
    }

    public enum TempoAudioAttributes
    {
#if BASS_AUDIO

        Frequency = BASSAttribute.BASS_ATTRIB_FREQ,
        Tempo = BASSAttribute.BASS_ATTRIB_TEMPO,
        TempoPitch = BASSAttribute.BASS_ATTRIB_TEMPO_PITCH,

#endif
    }
}
