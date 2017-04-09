// Source: http://wiki.unity3d.com/index.php?title=Texture_Mask

Shader "Unlit/AlphaHide"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_Mask("Culling Mask", 2D) = "white" {}
	_Cutoff("Alpha cutoff", Range(0,1)) = 0.1
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" }
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest GEqual[_Cutoff]
		Pass
	{
		SetTexture[_Mask]{ combine texture }
		SetTexture[_MainTex]{ combine texture, previous }
	}
	}
}
