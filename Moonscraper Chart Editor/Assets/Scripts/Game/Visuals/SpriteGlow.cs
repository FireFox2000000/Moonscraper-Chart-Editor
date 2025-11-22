// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;

[ExecuteInEditMode]
public class SpriteGlow : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color GlowColor = Color.white;

    [Range(0, 16)]
    public int OutlineWidth = 1;

    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateOutline(true);
    }

    private void OnDisable()
    {
        UpdateOutline(false);
    }

    private void Update()
    {
        UpdateOutline(true);
    }

    private void UpdateOutline(bool outline)
    {
        var mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", outline ? 1f : 0);
        mpb.SetColor("_OutlineColor", GlowColor);
        mpb.SetFloat("_OutlineSize", OutlineWidth);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}