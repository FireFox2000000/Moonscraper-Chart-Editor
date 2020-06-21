﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MK/MKGlow/Unlit/Texture" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	
	_MKGlowColor ("Glow Color", Color) = (1,1,1,1)
	_MKGlowPower ("Glow Power", Range(0.0,5.0)) = 2.5
	_MKGlowTex ("Glow Texture", 2D) = "black" {}
	_MKGlowTexColor ("Glow Texture Color", Color) = (1,1,1,1)
	_MKGlowTexStrength ("Glow Texture Strength ", Range(0.0,1.0)) = 1.0
	
}

SubShader {
	Tags { "RenderType"="MKGlow" }
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _MKGlowTex;
			half _MKGlowTexStrength;
			fixed4 _MKGlowTexColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed4 d = tex2D(_MKGlowTex, i.texcoord) * _MKGlowTexColor;
				col += (d * _MKGlowTexStrength);
				return col;
			}
		ENDCG
	}
}

}
