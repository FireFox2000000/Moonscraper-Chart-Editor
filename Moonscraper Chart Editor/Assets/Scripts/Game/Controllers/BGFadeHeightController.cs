// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class BGFadeHeightController : MonoBehaviour {
    [SerializeField]
    Transform objectMaxPosition = null;
    [SerializeField]
    float offset = 0.0f;
    Renderer ren;
    MaterialPropertyBlock matBlock;

    // Use this for initialization
    void Start () {
        ren = GetComponent<Renderer>();
        matBlock = new MaterialPropertyBlock();
    }
	
	// Update is called once per frame
	public void AdjustHeight () {
        Vector3 worldFadePos = objectMaxPosition.position;
        worldFadePos.y -= offset;
        float screenHeight = Camera.main.WorldToScreenPoint(worldFadePos).y;

        ren.GetPropertyBlock(matBlock);
        matBlock.SetFloat("_HeightPosition", screenHeight / Screen.height);
        ren.SetPropertyBlock(matBlock);
    }

#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        ren.SetPropertyBlock(null);
    }
#endif
}
