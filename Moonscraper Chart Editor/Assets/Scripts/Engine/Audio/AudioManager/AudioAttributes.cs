#if BASS_AUDIO
using Un4seen.Bass;
#endif

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
