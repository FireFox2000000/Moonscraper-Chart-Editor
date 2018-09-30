#define BASS_AUDIO

using System;
using System.Collections.Generic;

#if BASS_AUDIO
using Un4seen.Bass;
#endif

public static class AudioManager {
    public static bool disposed { get; private set; }
    static int streamRefCount = 0;

    #region Memory
    public static bool Init()
    {
        disposed = false;
        UnityEngine.Debug.Log("Audio Manager ref count: " + streamRefCount);

        bool success = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        if (!success)
            UnityEngine.Debug.LogError("Failed Bass.Net initialisation");
        else
            UnityEngine.Debug.Log("Bass.Net initialised");

        return success;
    }

    public static void Dispose()
    {
        Bass.BASS_Free();
        UnityEngine.Debug.Log("Freed Bass Audio memory");
        disposed = true;

        UnityEngine.Debug.Log("Audio Manager ref count: " + streamRefCount);
    }

    public static bool FreeAudioStream(AudioStream stream)
    {
        bool success = false;

        if (StreamIsValid(stream))
        {
            success = Bass.BASS_StreamFree(stream.audioHandle);
            if (!success)
                UnityEngine.Debug.LogError("Error while freeing audio stream " + stream.audioHandle);
            else
            {
                UnityEngine.Debug.Log("Successfully freed audio stream");
                --streamRefCount;
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Attempted to free an invalid audio stream");
        }

        return success;
    }

    #endregion

    #region IO

    public static TempoStream LoadTempoStream(string filepath)
    {
        int audioStreamHandle = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_ASYNCFILE | BASSFlag.BASS_STREAM_PRESCAN);
        audioStreamHandle = Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_TempoCreate(audioStreamHandle, BASSFlag.BASS_FX_FREESOURCE);
     
        var newStream = new TempoStream(audioStreamHandle);
        ++streamRefCount;
        return newStream;
    }

    public static AudioStream LoadStream(string filepath)
    {
        int audioStreamHandle = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_STREAM_DECODE);

        var newStream = new AudioStream(audioStreamHandle);
        ++streamRefCount;
        return newStream;
    }

    #endregion

    public static void Play(AudioStream audioStream, float playPoint)
    {
        Bass.BASS_ChannelSetPosition(audioStream.audioHandle, playPoint);
        Bass.BASS_ChannelPlay(audioStream.audioHandle, false);
    }

    public static void Stop(AudioStream audioStream)
    {
        Bass.BASS_ChannelStop(audioStream.audioHandle);
    }

    #region Attributes

    public static float GetAttribute(AudioStream audioStream, AudioAttributes attribute)
    {
        float value = 0;
        Bass.BASS_ChannelGetAttribute(audioStream.audioHandle, BASSAttribute.BASS_ATTRIB_FREQ, ref value);
        return value;
    }

    public static void SetAttribute(AudioStream audioStream, AudioAttributes attribute, float value)
    {
        Bass.BASS_ChannelSetAttribute(audioStream.audioHandle, BASSAttribute.BASS_ATTRIB_FREQ, value);
    }

    public static float GetAttribute(TempoStream audioStream, TempoAudioAttributes attribute)
    {
        float value = 0;
        Bass.BASS_ChannelGetAttribute(audioStream.audioHandle, BASSAttribute.BASS_ATTRIB_FREQ, ref value);
        return value;
    }

    public static void SetAttribute(TempoStream audioStream, TempoAudioAttributes attribute, float value)
    {
        Bass.BASS_ChannelSetAttribute(audioStream.audioHandle, BASSAttribute.BASS_ATTRIB_FREQ, value);
    }

    #endregion

    #region Helper Functions

    public static bool StreamIsValid(AudioStream audioStream)
    {
        return audioStream != null && audioStream.isValid;
    }

    #endregion
}

// Written as extensions so we can keep BASS.NET references in as few files as possible
public static class AudioStreamExtensions
{
    public static long ChannelLengthInBytes(this AudioStream audioStream)
    {
        return Bass.BASS_ChannelGetLength(audioStream.audioHandle, BASSMode.BASS_POS_BYTES);
    }

    public static float ChannelLengthInSeconds(this AudioStream audioStream)
    {
        return (float)Bass.BASS_ChannelBytes2Seconds(audioStream.audioHandle, ChannelLengthInBytes(audioStream));
    }

    public static long ChannelSecondsToBytes(this AudioStream audioStream, double position)
    {
        return Bass.BASS_ChannelSeconds2Bytes(audioStream.audioHandle, position);
    }

    public static float CurrentPositionInSeconds(this AudioStream audioStream)
    {
        long bytePos = Bass.BASS_ChannelGetPosition(audioStream.audioHandle);
        double elapsedtime = Bass.BASS_ChannelBytes2Seconds(audioStream.audioHandle, bytePos);
        return (float)elapsedtime;
    }

    public static bool GetChannelLevels(this AudioStream audioStream, ref float[] levels, float length)
    {
        return Bass.BASS_ChannelGetLevel(audioStream.audioHandle, levels, length, BASSLevel.BASS_LEVEL_STEREO);
    }
}
