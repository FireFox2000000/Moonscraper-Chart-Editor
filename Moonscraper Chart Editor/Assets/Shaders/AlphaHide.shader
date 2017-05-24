// Source: http://wiki.unity3d.com/index.php?title=Texture_Mask

Shader "Unlit/AlphaHide"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Mask("Culling Mask", 2D) = "white" {}
		_HeightPosition("Height", Range(0, 1)) = 0.7
		_Spread("Range of Fade", Range(0, 1)) = 0.2
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
		/*	
		CGPROGRAM
//#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float2 uv :TEXCOORD0;
			float4 vertex : SV_POSITION;
		};
		
		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.normal * 0.5 + 0.5;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			return fixed4(i.color, 1);
		}
		ENDCG*/
	}
	}
}
