// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

#if BASS_AUDIO
using Un4seen.Bass;
#endif

using UnityEngine;
using System.Collections.Generic;

namespace MoonscraperEngine.Audio
{
    public class AudioStream
    {
        public int audioHandle { get; private set; }
        bool isDisposed { get { return audioHandle == 0; } }
        List<int> childSyncedStreams = new List<int>();

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
            CurrentPositionSeconds = playPoint;
            Bass.BASS_ChannelPlay(audioHandle, restart);
            return true;
        }

        public bool PlaySynced(float playPoint, IList<AudioStream> streamsToSync)
        {
            foreach(var stream in streamsToSync)
            {
                if (stream != null && stream.isValid)
                {
                    stream.CurrentPositionSeconds = playPoint;
                    SyncWithStream(stream);
                }
            }

            return Play(playPoint, false);
        }

        public virtual void Stop()
        {
            Bass.BASS_ChannelStop(audioHandle);

            // Synchronisation is only temporary as user may add or remove streams between different play sessions. 
            foreach (int stream in childSyncedStreams)
            {
                if (!Bass.BASS_ChannelRemoveLink(this.audioHandle, stream))
                {
                    var bassError = Bass.BASS_ErrorGetCode();
                    Debug.LogError("AudioStream ClearSyncedStreams error: " + bassError);
                }
            }

            childSyncedStreams.Clear();
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

        public float CurrentPositionSeconds
        {
            get
            {
                long bytePos = Bass.BASS_ChannelGetPosition(audioHandle);
                double elapsedtime = Bass.BASS_ChannelBytes2Seconds(audioHandle, bytePos);
                return (float)elapsedtime;
            }
            set
            {
                Bass.BASS_ChannelSetPosition(audioHandle, value);
            }
        }

        public bool GetChannelLevels(ref float[] levels, float length)
        {
            return Bass.BASS_ChannelGetLevel(audioHandle, levels, length, BASSLevel.BASS_LEVEL_STEREO);
        }

        public bool IsPlaying()
        {
            return isValid && Bass.BASS_ChannelIsActive(audioHandle) == BASSActive.BASS_ACTIVE_PLAYING;
        }

        // Call this before playing any audio
        void SyncWithStream(AudioStream childStream)
        {
            if (audioHandle == childStream.audioHandle)
                return;

            if (Bass.BASS_ChannelSetLink(this.audioHandle, childStream.audioHandle))
            {
                childSyncedStreams.Add(childStream.audioHandle);
            }
            else
            {
                var bassError = Bass.BASS_ErrorGetCode();
                Debug.LogError("AudioStream SyncWithStream error: " + bassError);
            }
        }
    }
}