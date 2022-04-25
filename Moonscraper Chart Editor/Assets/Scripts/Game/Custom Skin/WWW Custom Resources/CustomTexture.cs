// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.IO;
using UnityEngine;

public class CustomTexture : CustomResource
{
    protected Texture2D texture;
    int? width, height;

    public CustomTexture(string name, int? width = null, int? height = null) : base(name, new string[] { ".png", ".jpg", ".dds" })
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
            texture = new Texture2D(width.HasValue ? width.Value : www.texture.width, height.HasValue ? height.Value : www.texture.height, texFormat, false);
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
    public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat, int? width = null, int? height = null)
    {
        if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
            throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
            throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

        int imageHeight = ddsBytes[13] * 256 + ddsBytes[12];
        int imageWidth = ddsBytes[17] * 256 + ddsBytes[16];

        int textureHeight = height.HasValue ? height.Value : imageHeight;
        int textureWidth = width.HasValue ? width.Value : imageWidth;

        int DDS_HEADER_SIZE = 128;

        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Texture2D texture = new Texture2D(textureWidth, textureHeight, textureFormat, false);
        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();

        texture = texture.VerticalFlip();    // dds files load in upsidedown for some reason

        return (texture);
    }
}
