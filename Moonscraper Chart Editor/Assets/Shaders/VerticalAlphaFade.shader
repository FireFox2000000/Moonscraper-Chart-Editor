// Source: http://wiki.unity3d.com/index.php?title=Texture_Mask

Shader "Unlit/VerticalAlphaFade"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Mask("Culling Mask", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.1
	}
		SubShader
	{
		Tags
		{ 
			"Queue" = "Geometry" 
			"RenderType" = "Geometry" 
			"IgnoreProjector" = "True"
		}
		Lighting Off
		ZWrite Off
		Cull Off
		ZTest[unity_GUIZTestMode]
		Blend Zero OneMinusSrcAlpha
		AlphaTest GEqual[_Cutoff]
		Pass
	{
		SetTexture[_Mask]{ combine texture }
		SetTexture[_MainTex]{ combine texture, previous }
	}
	}
}
