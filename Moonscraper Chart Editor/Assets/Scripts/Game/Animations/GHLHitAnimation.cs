using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHLHitAnimation : DefaultHitAnimation
{
    public bool canUse = false;

    public SpriteRenderer pressRen
    {
        get
        {
            if (!ren)
            {
                base.Start();
            }
            return ren;
        }
    }
}
