// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

public class CustomAudioClip : CustomResource
{
    AudioClip audio;

    public CustomAudioClip(string name) : base(name, new string[] { ".ogg", ".wav" }) { }

    public override void AssignResource()
    {
        if (www.isDone)
        {
            audio = DownloadHandlerAudioClip.GetContent(www);
        }
        else
        {
            Debug.LogError("Trying to assign a custom resource when it hasn't finished loading");
        }
    }

    public override UnityEngine.Object GetObject()
    {
        return audio;
    }

    public override bool InitWWW(Dictionary<string, string> files)
    {
        if (!validateFile(files))
            return false;

        AudioType audioType;
        string extension = Path.GetExtension(filepath);

        switch (extension)
        {
            case ".ogg":
                audioType = AudioType.OGGVORBIS;
                break;

            case ".wav":
                audioType = AudioType.WAV;
                break;

            default:
                Debug.LogError("Unsupported audio format detected");
                return false;
        }

        www = UnityWebRequestMultimedia.GetAudioClip(filepath, audioType);
        www.SendWebRequest();

        return true;
    }
}