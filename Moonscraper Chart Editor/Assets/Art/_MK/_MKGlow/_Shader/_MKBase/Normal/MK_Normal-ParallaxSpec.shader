﻿Shader "MK/MKGlow/Normal/ParallaxSpec" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_Parallax ("Height", Range (0.005, 0.08)) = 0.02
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_ParallaxMap ("Heightmap (A)", 2D) = "black" {}
		
		_MKGlowColor ("Glow Color", Color) = (1,1,1,1)
		_MKGlowPower ("Glow Power", Range(0.0,5.0)) = 2.5
		_MKGlowTex ("Glow Texture", 2D) = "black" {}
		_MKGlowTexColor ("Glow Texture Color", Color) = (1,1,1,1)
		_MKGlowTexStrength ("Glow Texture Strength ", Range(0.0,1.0)) = 1.0
		
	}
	SubShader 
	{ 
		Tags { "RenderType"="MKGlow" }
		LOD 600
		
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _ParallaxMap;
		fixed4 _Color;
		half _Shininess;
		float _Parallax;
		
		sampler2D _MKGlowTex;
		half _MKGlowTexStrength;
		fixed4 _MKGlowTexColor;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_MKGlowTex;
			float2 uv_BumpMap;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			half h = tex2D (_ParallaxMap, IN.uv_BumpMap).w;
			float2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);
			IN.uv_MainTex += offset;
			IN.uv_BumpMap += offset;
			
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 d = tex2D(_MKGlowTex, IN.uv_MKGlowTex) * _MKGlowTexColor;
			tex += (d * _MKGlowTexStrength);
			
			o.Albedo = tex.rgb * _Color.rgb;
			o.Gloss = tex.a;
			o.Alpha = tex.a * _Color.a;
			o.Specular = _Shininess;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	}

FallBack "Bumped Specular"
}
