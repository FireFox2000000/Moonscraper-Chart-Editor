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
    public int[] offsets = new int[1];
}
