using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSync
{
    float desyncAmount;
    float desyncLenience = .005f;
    double audioPosition;
    double lastAudioPosition;
    float secondTimer;
    float songSpeed;

    bool isAdjusting;

    private float practiceSpeed = 1;
    public float PracticeSpeed
    {
        set { practiceSpeed = value; }
    }

    double songTime;
    public double SongTime
    {
        set
        {
            songTime = value;
            audioPosition = 0;
        }
    }

    private double songOffset;
    public double Offset
    {
        set
        {
            songOffset = value;
            songTime = -1;
            songSpeed = Globals.gameSpeed;
        }
    }

    public void StopSync()
    {
        isAdjusting = false;
        audioPosition = 0;
        desyncAmount = 0;
    }

    public void ForceSync()
    {
        secondTimer = .9f;
    }

    public double GetTime()
    {
        while(secondTimer >= 1)
        {
            if (Globals.applicationMode == Globals.ApplicationMode.Playing)
            {
                int audioStream = ChartEditor.FindCurrentEditor().currentSong.bassAudioStreams[0];
                long bytePos = Un4seen.Bass.Bass.BASS_ChannelGetPosition(audioStream);
                double elapsedtime = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(audioStream, bytePos);

                audioPosition = elapsedtime;// BassAudioManager.instance.GetPosition() / songSpeed;

                if (audioPosition > 0 && audioPosition != lastAudioPosition)
                {
                    desyncAmount = (float)(audioPosition - (songTime));

                    isAdjusting = desyncAmount > desyncLenience || desyncAmount < -desyncLenience;
                }
                else
                    isAdjusting = false;

                lastAudioPosition = audioPosition;
            }

            secondTimer -= 1;
        }

        if (isAdjusting)
            songTime += desyncAmount * Time.deltaTime;

        songTime += Time.deltaTime * Globals.gameSpeed;
        secondTimer += Time.deltaTime;

        return songTime + songOffset;
    }
}
