using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStream
{
    public int audioHandle { get; private set; }
    bool isDisposed { get { return audioHandle == 0; } }

    public float volume
    {
        get { return AudioManager.GetAttribute(this, AudioAttributes.Volume); }
        set { AudioManager.SetAttribute(this, AudioAttributes.Volume, value); }
    }

    public float pan
    {
        get { return AudioManager.GetAttribute(this, AudioAttributes.Pan); }
        set { AudioManager.SetAttribute(this, AudioAttributes.Pan, value); }
    }

    public AudioStream(int handle)
    {
        audioHandle = handle;
    }

    ~AudioStream()
    {
        Debug.Assert(isDisposed, "Audio streams must be explicity disposed");
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            Debug.Assert(!AudioManager.disposed, "Should have been disposed before the audio manager's disposal");

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
}