// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;

public class CustomAudioClip : CustomResource
{
    AudioClip audio;

    public CustomAudioClip(string name) : base(name, new string[] { ".ogg", ".wav" }) { }

    public override void AssignResource()
    {
        if (www.isDone)
        {
            audio = www.GetAudioClip(false, false);
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
}