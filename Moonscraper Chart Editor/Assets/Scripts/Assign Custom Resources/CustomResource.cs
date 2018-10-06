// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public abstract class CustomResource
{
    public string name { get; private set; }
    public WWW www { get; private set; }
    protected readonly string[] validExtentions;

    protected CustomResource(string name, string[] validExtentions)
    {
        this.name = name;
        this.validExtentions = validExtentions;
    }

    public bool InitWWW(Dictionary<string, string> files)
    {
        string file = string.Empty;

        if (!(files.TryGetValue(name, out file) && Utility.validateExtension(file, validExtentions)))
            return false;

        if (file != string.Empty)
        {
            www = new WWW("file://" + file);
            return true;
        }
        else
            return false;
    }

    public abstract void AssignResource();
    public abstract UnityEngine.Object GetObject();
}

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

public class CustomTexture : CustomResource
{
    protected Texture2D texture;
    int width, height;

    public CustomTexture(string name, int width, int height) : base(name, new string[] { ".png", ".jpg", ".dds" })
    {
        this.width = width;
        this.height = height;
    }

    public override void AssignResource()
    {
        if (www.isDone)
        {
            TextureFormat texFormat;
            string extension = Path.GetExtension(www.url);

            switch (extension)
            {
                case (".png"):
                    texFormat = TextureFormat.ARGB32;
                    break;
                case (".jpg"):
                    texFormat = TextureFormat.DXT1;
                    break;
                case (".dds"):
                    try
                    {
                        texture = LoadTextureDXT(www.bytes, TextureFormat.DXT5, width, height);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("DXT5 read failed. Will try DTX1. Error: " + e.Message);

                        try
                        {
                            texture = LoadTextureDXT(www.bytes, TextureFormat.DXT1, width, height);
                            Debug.Log("DTX1 read successful");
                        }
                        catch (Exception e1)
                        {
                            Debug.LogError("DXT1 Error: " + e1.Message);
                            texture = null;
                        } 
                    }
                    return;
                default:
                    Debug.LogError("Unsupported texture format detected");
                    return;
            }
            texture = new Texture2D(width, height, texFormat, false);
            www.LoadImageIntoTexture(texture);
        }
        else
        {
            Debug.LogError("Trying to assign a custom resource when it hasn't finished loading");
        }
    }

    public override UnityEngine.Object GetObject()
    {
        return texture;
    }

    // Method from http://answers.unity3d.com/questions/555984/can-you-load-dds-textures-during-runtime.html#answer-707772
    public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat, int width, int height)
    {
        if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
            throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
            throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

        //int height_0 = ddsBytes[13] * 256 + ddsBytes[12];
        //int width_0 = ddsBytes[17] * 256 + ddsBytes[16];

        int DDS_HEADER_SIZE = 128;
        
        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Texture2D texture = new Texture2D(width, height, textureFormat, false);
        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();

        texture = texture.VerticalFlip();    // dds files load in upsidedown for some reason

        return (texture);
    }
}

public class CustomSprite : CustomTexture
{
    Sprite sprite;
    int _pixelsPerUnit;

    public CustomSprite(string name, int width, int height, int pixelsPerUnit) : base(name, width, height)
    {
        _pixelsPerUnit = pixelsPerUnit;
    }

    public override void AssignResource()
    {
        base.AssignResource();
        if (texture)
        {
            sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), _pixelsPerUnit);
        }
    }

    public override UnityEngine.Object GetObject()
    {
        return sprite;
    }
}
