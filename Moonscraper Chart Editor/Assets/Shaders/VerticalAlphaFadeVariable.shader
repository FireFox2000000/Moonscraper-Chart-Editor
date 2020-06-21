// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

Shader "Unlit/VerticalAlphaFadeVariable"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_HeightPosition("Height", Range(0, 1)) = 0.7
		_Spread("Range of Fade", Range(0, 1)) = 0.2
        _BlendTex("Secondary (RGB)", 2D) = "white" {}
        _Blend("Blend", Range(0, 1)) = 0
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
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest GEqual[_Cutoff]

		Pass
		{		
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
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float _HeightPosition;
			float _Spread;

            sampler2D _BlendTex;
            float _Blend;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 main_col = tex2D(_MainTex, i.uv);
                fixed4 secondary_col = tex2D(_BlendTex, i.uv);

                // Apply blending
                main_col = lerp(main_col, secondary_col, _Blend);
		
				// Fade alpha
				float spread_min = _HeightPosition - _Spread / 2.0f;
				float pix_pos = i.uv.y;
				main_col.a = (pix_pos - spread_min) / _Spread;

				return main_col;
			}

			ENDCG
		}
	}
}
