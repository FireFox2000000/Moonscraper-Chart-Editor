#if BASS_AUDIO
using Un4seen.Bass;
#endif

using UnityEngine;

public class AudioStream
{
    public int audioHandle { get; private set; }
    bool isDisposed { get { return audioHandle == 0; } }

    public virtual float volume
    {
        get { return AudioManager.GetAttribute(this, AudioAttributes.Volume); }
        set { AudioManager.SetAttribute(this, AudioAttributes.Volume, value); }
    }

    public virtual float pan
    {
        get { return AudioManager.GetAttribute(this, AudioAttributes.Pan); }
        set { AudioManager.SetAttribute(this, AudioAttributes.Pan, value); }
    }

    public AudioStream(int handle)
    {
        audioHandle = handle;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            if (AudioManager.FreeAudioStream(this))
            {
                audioHandle = 0;
                Debug.Log("Audio sample disposed");
            }
        }
    }

    public bool isValid
    {
        get { return !isDisposed; }
    }

    public virtual bool Play(float playPoint, bool restart = false)
    {
        Bass.BASS_ChannelSetPosition(audioHandle, playPoint);
        Bass.BASS_ChannelPlay(audioHandle, restart);
        return true;
    }

    public virtual void Stop()
    {
        Bass.BASS_ChannelStop(audioHandle);
    }

    public long ChannelLengthInBytes()
    {
        return Bass.BASS_ChannelGetLength(audioHandle, BASSMode.BASS_POS_BYTES);
    }

    public float ChannelLengthInSeconds()
    {
        return (float)Bass.BASS_ChannelBytes2Seconds(audioHandle, ChannelLengthInBytes());
    }

    public long ChannelSecondsToBytes(double position)
    {
        return Bass.BASS_ChannelSeconds2Bytes(audioHandle, position);
    }

    public float CurrentPositionInSeconds()
    {
        long bytePos = Bass.BASS_ChannelGetPosition(audioHandle);
        double elapsedtime = Bass.BASS_ChannelBytes2Seconds(audioHandle, bytePos);
        return (float)elapsedtime;
    }

    public bool GetChannelLevels(ref float[] levels, float length)
    {
        return Bass.BASS_ChannelGetLevel(audioHandle, levels, length, BASSLevel.BASS_LEVEL_STEREO);
    }

    public bool IsPlaying()
    {
        return isValid && Bass.BASS_ChannelIsActive(audioHandle) == BASSActive.BASS_ACTIVE_PLAYING;
    }
}