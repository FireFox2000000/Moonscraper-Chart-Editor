// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

#if BASS_AUDIO
using Un4seen.Bass;
#endif

namespace MoonscraperEngine.Audio
{
    public class OneShotSampleStream : AudioStream
    {
        float _volume;
        float _pan;
        public bool onlyPlayIfStopped = false;

        public OneShotSampleStream(int handle, int maxChannels) : base(handle)
        {
        }

        public override float volume
        {
            get { return _volume; }
            set { _volume = value; }
        }

        public override float pan
        {
            get { return _pan; }
            set { _pan = value; }
        }

        public override bool Play(float playPoint = 0, bool restart = false)
        {
            base.Play(playPoint, restart);

            int channel = Bass.BASS_SampleGetChannel(audioHandle, false);

            bool isPlaying = Bass.BASS_ChannelIsActive(channel) != BASSActive.BASS_ACTIVE_STOPPED && Bass.BASS_ChannelIsActive(channel) != BASSActive.BASS_ACTIVE_PAUSED;
            if (onlyPlayIfStopped && isPlaying)
            {
                return false;
            }

            if (channel != 0)
            {
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, volume);
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_PAN, pan);

                Bass.BASS_ChannelPlay(channel, restart);
                return true;
            }
            else
                UnityEngine.Debug.LogError("Error when playing sample stream: " + Bass.BASS_ErrorGetCode() + ", " + audioHandle);

            return false;
        }

        public override void Stop()
        {
            // Unsupported, stops when completed
        }
    }
}
