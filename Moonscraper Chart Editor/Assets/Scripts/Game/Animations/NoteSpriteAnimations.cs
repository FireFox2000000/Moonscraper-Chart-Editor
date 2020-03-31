﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpriteAnimations : ScriptableObject
{
    public NoteSpriteAnimationData[] animations;
}

[System.Serializable]
public class NoteSpriteAnimationData
{  
    public string name;
    public float speed = 1;
    public int totalSprites = 1;
}
