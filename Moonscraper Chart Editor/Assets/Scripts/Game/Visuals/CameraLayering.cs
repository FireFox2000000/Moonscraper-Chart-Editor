// Copyright (c) 2016-2020 Alexander Ong
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

        ChartEditor.Instance.events.viewModeSwitchEvent.Register(UpdateCullingMask);

        UpdateCullingMask(Globals.viewMode);
    }

    void UpdateCullingMask(in Globals.ViewMode viewMode)
    {
        if (viewMode == Globals.ViewMode.Chart)
        {
            // Configure camera to ignore song
            cam.cullingMask |= 1 << LayerMask.NameToLayer("ChartObject");
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("SongObject"));

            if (mkGlow && !(SkinManager.Instance.noteSpritesAvaliable != null && SkinManager.Instance.noteSpritesAvaliable == Skin.AssestsAvaliable.All))
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
