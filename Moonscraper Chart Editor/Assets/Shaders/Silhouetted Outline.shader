// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//http://coredumping.com/random/OutlineShader.shader

Shader "Outlined/Silhouette Only"
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.03)) = .005
	}

		CGINCLUDE
#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _Outline;
	uniform float4 _OutlineColor;

	v2f vert(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += offset * o.pos.z * _Outline;
		o.color = _OutlineColor;

		return o;
	}
	ENDCG

		SubShader
	{
		Tags{ "Queue" = "Transparent" }

		Pass
	{
		Name "BASE"
		CULL OFF
		ZTEST ALWAYS
		ZWRITE OFF
		Blend ONE ONE

		Stencil
	{
		REF 2
		COMP ALWAYS
		PASS REPLACE
		ZFAIL REPLACE
	}

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		half4 frag(v2f i) :COLOR
	{
		return half4(0,0,0,0);
	}
		ENDCG
	}

		Pass
	{
		Name "INNER"
		CULL OFF
		ZTEST ALWAYS
		ZWRITE OFF
		Blend One One

		Stencil
	{
		REF 1
		COMP ALWAYS
		PASS REPLACE
		ZFAIL REPLACE
	}

		CGPROGRAM
#pragma vertex vert2
#pragma fragment frag
		v2f vert2(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.color = _OutlineColor;
		return o;
	}

	half4 frag(v2f i) :COLOR
	{
		return half4(0,0,0,0);
	}
		ENDCG
	}

		Pass
	{
		Name "OUTLINE"
		CULL OFF
		ZTEST ALWAYS
		ZWRITE OFF
		BLEND ONE ONEMINUSDSTCOLOR

		Stencil
	{
		REF 2
		COMP EQUAL
		PASS REPLACE
		ZFAIL REPLACE
	}

		CGPROGRAM
#pragma vertex vert 
#pragma fragment frag
		half4 frag(v2f i) :COLOR
	{
		return i.color;
	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}