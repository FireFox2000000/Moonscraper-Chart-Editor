// Source: http://wiki.unity3d.com/index.php?title=Texture_Mask

Shader "Unlit/VerticalAlphaFadeVariable"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
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
		/*
		SetTexture[_Mask]{ combine texture }
		SetTexture[_MainTex]{ combine texture, previous }*/
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};
		
		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.uv;
			return o;
		}

		sampler2D _MainTex;
		float _HeightPosition;
		float _Spread;

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 main_col = tex2D(_MainTex, i.uv);
		
			if (i.uv.y < _HeightPosition - _Spread / 2.0f)
				main_col.a = 0;
			else if (i.uv.y > _HeightPosition + _Spread / 2.0f)
				main_col.a = 1;
			else
			{
				// Blend alpha
				float spread_min = _HeightPosition - _Spread / 2.0f;
				float pix_pos = i.uv.y;
				main_col.a = (pix_pos - spread_min) / _Spread;
			}

			return main_col;
		}

		ENDCG
	}
	}
}
