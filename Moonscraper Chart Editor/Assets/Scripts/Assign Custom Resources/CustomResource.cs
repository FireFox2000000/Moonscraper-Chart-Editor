using UnityEngine;
using System.Collections;
using System;
using System.IO;

public abstract class CustomResource
{
    protected string name;
    public WWW www;
    protected string[] validExtentions;

    public CustomResource(string name)
    {
        this.name = name;
    }

    public bool InitWWW(string[] files)
    {
        string file = string.Empty;

        foreach(string searchFile in files)
        {
            if (Utility.validateExtension(searchFile, validExtentions) && Path.GetFileNameWithoutExtension(searchFile) == name)
            {
                file = searchFile;
            }
        }

        if (file != string.Empty)
        {
            www = new WWW("file://" + file);
            return true;
        }
        else
            return false;
    }

    public abstract void AssignResource();
}

public class CustomAudioClip : CustomResource
{
    public AudioClip audio;

    public CustomAudioClip(string name) : base(name) { validExtentions = new string[] { ".ogg", ".wav" }; }

    public override void AssignResource()
    {
        if (www.isDone)
        {
            audio = www.GetAudioClip(false, true);
        }
    }
}

public class CustomTexture : CustomResource
{
    public Texture2D texture;
    int width, height;

    public CustomTexture(string name, int width, int height) : base(name)
    {
        validExtentions = Globals.validTextureExtensions;
        this.width = width;
        this.height = height;
    }

    public override void AssignResource()
    {
        if (www.isDone)
        {
            TextureFormat texFormat;
            if (Path.GetExtension(www.url) == ".png")
            {
                Debug.Log("png");
                texFormat = TextureFormat.ARGB32;
            }
            else
                texFormat = TextureFormat.DXT1;

            texture = new Texture2D(width, height, texFormat, false);
            www.LoadImageIntoTexture(texture);
        }
    }
}
