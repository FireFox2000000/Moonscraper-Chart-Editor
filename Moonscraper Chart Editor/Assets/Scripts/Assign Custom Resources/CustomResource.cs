using UnityEngine;
using System.Collections;
using System;
using System.IO;

public abstract class CustomResource
{
    protected string _name;
    public string name { get { return _name; } }
    public WWW www;
    protected string[] validExtentions;

    public CustomResource(string name)
    {
        this._name = name;
    }

    public bool InitWWW(string[] files)
    {
        string file = string.Empty;

        foreach(string searchFile in files)
        {
            if (Utility.validateExtension(searchFile, validExtentions) && Path.GetFileNameWithoutExtension(searchFile) == _name)
            {
                file = searchFile;
                break;
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
        validExtentions = new string[] { ".png", ".jpg", ".dds" }; 
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
                        texture = LoadTextureDXT(www.bytes, TextureFormat.DXT5);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        texture = null;
                    }
                    return;
                default:
                    Debug.LogError("Unsupported texture format detected");
                    return;
            }
            texture = new Texture2D(width, height, texFormat, false);
            www.LoadImageIntoTexture(texture);
        }
    }

    // Method from http://answers.unity3d.com/questions/555984/can-you-load-dds-textures-during-runtime.html#answer-707772
    public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
    {
        if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
            throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
            throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

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
