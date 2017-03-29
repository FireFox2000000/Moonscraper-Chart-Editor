using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin : ScriptableObject {
    public CustomResource[] resources = new CustomResource[] {
        new CustomAudioClip("break-0"),
        new CustomTexture("background-0", 1920, 1080),
        new CustomTexture("fretboard-0", 512, 1024),
        new CustomAudioClip("clap")
    };

    public AudioClip break0 { get { return ((CustomAudioClip)resources[0]).audio; } }
    public Texture2D background0 { get { return ((CustomTexture)resources[1]).texture; } }
    public Texture2D fretboard { get { return ((CustomTexture)resources[2]).texture; } }
    public AudioClip clap { get { return ((CustomAudioClip)resources[3]).audio; } }
}
