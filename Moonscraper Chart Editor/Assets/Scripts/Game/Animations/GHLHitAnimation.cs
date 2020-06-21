// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
