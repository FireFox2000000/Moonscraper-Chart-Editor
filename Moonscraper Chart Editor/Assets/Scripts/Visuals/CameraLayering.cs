// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraLayering : MonoBehaviour {

    Camera cam;
    MKGlowSystem.MKGlow mkGlow;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
        mkGlow = GetComponent<MKGlowSystem.MKGlow>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            // Configure camera to ignore song
            cam.cullingMask |= 1 << LayerMask.NameToLayer("ChartObject");
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("SongObject"));

            if (mkGlow && !(AssignCustomResources.noteSpritesAvaliable != null && AssignCustomResources.noteSpritesAvaliable == Skin.AssestsAvaliable.All))
            {
                mkGlow.enabled = true;
                //mkGlow.GlowLayer |= 1 << LayerMask.NameToLayer("ChartObject");
                mkGlow.GlowLayer &= ~(1 << LayerMask.NameToLayer("SongObject"));
            }
        }
        else
        {
            // Configure camera to ignore chart
            cam.cullingMask |= 1 << LayerMask.NameToLayer("SongObject");
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("ChartObject"));

            if (mkGlow)
            {
                mkGlow.enabled = false;
            }
        }
    }
}
