// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class StarpowerGUIController : TimelineIndicator {
    public Starpower starpower { get { return (Starpower)songObject; } set { songObject = value; } }
    const float MIN_SIZE = 0.2f;

    uint prevLength = 0;

    void LateUpdate()
    {        
        if ((songObject != null && songObject.song != null && prevLength != starpower.length))
        {
            ExplicitUpdate();
        }

        prevLength = starpower.length;
    }

    public override void ExplicitUpdate()
    {
        base.ExplicitUpdate();

        // Change scale to represent starpower length
        Vector3 spLengthLocalPos = GetLocalPos(starpower.position + starpower.length, starpower.song);
        float size = spLengthLocalPos.y - transform.localPosition.y;

        Vector3 scale = transform.localScale;
        Vector3 position = transform.localPosition;

        scale.y = size;      // Offset because it extends past above and beyond for some reason. Possibly look into this later.

        if (scale.y < MIN_SIZE)
            scale.y = MIN_SIZE;

        position.y += size / 2.0f;

        transform.localPosition = position;
        transform.localScale = scale;

        
    }
}
