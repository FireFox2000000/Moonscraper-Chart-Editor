// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;

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
