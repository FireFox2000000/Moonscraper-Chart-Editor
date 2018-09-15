// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader found at- https://github.com/robertcupisz/fire

Shader "Fire" {
Properties
{
	[KeywordEnum(On, Off)] _Depth_Aware ("Depth aware", Float) = 0
	[KeywordEnum(O4, O3, O2, O1)] _Detail ("Detail", Float) = 0
	_Color("Color", Color) = (1,1,1)
	_Fire ("Fire", 2D) = "black"
	_Noise ("Noise", 2D) = "black"
}
SubShader {
    Tags { "Queue"="Transparent" }
    Pass {
        Fog { Mode Off }
		Cull Back
		ZWrite Off
		Blend One OneMinusSrcColor

CGPROGRAM
#pragma glsl
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _DETAIL_O4 _DETAIL_O3 _DETAIL_O2 _DETAIL_O1
#pragma multi_compile _DEPTH_AWARE_ON _DEPTH_AWARE_OFF
#include "UnityCG.cginc"

struct v2f {
	float4 pos : SV_POSITION;
	float3 localPos : TEXCOORD0;
#if _DEPTH_AWARE_ON
	float4 uv : TEXCOORD1;
	float3 ray : TEXCOORD2;
#endif	
};

sampler2D _Fire;
sampler2D _Noise;
float4 _CameraLocalPos;
float4 _Scale;
float4 _Color;
float _FireTime;
float _Brightness;
#if _DEPTH_AWARE_ON
sampler2D _CameraDepthTexture;
float4x4 _CameraToLocal;
#endif

v2f vert (float4 vertex : POSITION)
{
	v2f o;
	o.pos = UnityObjectToClipPos(vertex);
	o.localPos = vertex.xyz / _Scale.xyz;

#if _DEPTH_AWARE_ON
	o.uv = ComputeScreenPos (o.pos);
	o.ray = mul (UNITY_MATRIX_MV, vertex).xyz * float3(-1,-1,1);
#endif
	
	return o;
}

inline float2 Rand(float2 val, float m)
{
	val = fmod(val, m);
	return fmod(val * val, m);
}

float MNoise(float3 pos)
{
	float intArg = floor(pos.z);
	float fracArg = frac(pos.z);
	// 61 is the prime used for the mnoise texture
	float m = 61.0;
	float2 r = Rand(intArg * 3.0 + float2(0, 3), m);
	float4 g;
	g.xy = tex2Dlod(_Noise, float4(pos.x, pos.y + r.x,0,0) / m).xy;
	g.zw = tex2Dlod(_Noise, float4(pos.x, pos.y + r.y,0,0) / m).xy;
	g = g * 2.0 - 1.0;

	return lerp(g.x + g.y * fracArg, g.z + g.w * (fracArg - 1.0), smoothstep(0.0, 1.0, fracArg));
}

float FBM(float3 pos)
{
	float sum = 0.0;
	sum += abs(MNoise(pos));

#if _DETAIL_O4 || _DETAIL_O3 || _DETAIL_O2
	pos *= 2.0;
	sum += abs(MNoise(pos))*0.5;
#endif
#if _DETAIL_O4 || _DETAIL_O3
	pos *= 2.0;
	sum += abs(MNoise(pos))*0.25;
#endif
#if _DETAIL_O4
	pos *= 2.0;
	sum += abs(MNoise(pos))*0.125;
#endif
	
	return sum;
}

// pos in [-0.5, 0.5]^3
float2 FireUV(float3 pos)
{
	// Convert to [-1, 1]x[0, 1]x[-1, 1]
	pos.xz *= 2.0;
	pos.y += 0.5;
	// Stretch out y a little bit to use the mostly empty space
	pos.y *= 0.9;

	float2 uv = float2(sqrt(dot(pos.xz, pos.xz)), pos.y);

	pos.y -= _FireTime;
	pos *= _Scale.xyz;

	// sqrt makes the flame stationary and compressed at the bottom
	// and fast moving at the top
	uv.y += sqrt(uv.y) * FBM(pos);

	return uv;
}

bool Cylinder(float3 org, float3 dir, out float near, out float far)
{
	// quadratic x^2 + z^2 = 0.5^2 => (org.x + t*dir.x)^2 + (org.z + t*dir.z)^2 = 0.5^2
	float a = dot(dir.xz, dir.xz);
	float b = dot(org.xz, dir.xz) * 2.0;
	float c = dot(org.xz, org.xz) - 0.25;

	float delta = b * b - 4.0 * a * c;
	if( delta < 0.0 )
		return false;

	// 2 roots
	float deltasqrt = sqrt(delta);
	float arcp = 0.5 / a;
	near = (-b - deltasqrt) * arcp;
	far = (-b + deltasqrt) * arcp;
	
	// order roots
	float temp = min(far, near);
	far = max(far, near);
	near = temp;

	float ynear = org.y + near * dir.y;
	float yfar = org.y + far * dir.y;

	// top, bottom
	float2 ycap = float2(0.5, -0.5);
	float2 cap = (ycap - org.y) / dir.y;

	if ( ynear < ycap.y )
		near = cap.y;
	else if ( ynear > ycap.x )
		near = cap.x;

	if ( yfar < ycap.y )
		far = cap.y;
	else if ( yfar > ycap.x )
		far = cap.x;
	
	return far > 0.0 && far > near;
}

#define MAX_STEPS 28.0

float4 frag(v2f i) : COLOR
{	
	float3 dir = i.localPos - _CameraLocalPos.xyz;
	dir = normalize(dir);

	float near, far;
	if (!Cylinder (_CameraLocalPos.xyz, dir, near, far))
		return 0;

#if _DEPTH_AWARE_ON
	i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
	float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy / i.uv.w));
	float4 viewDepthPos = float4(i.ray*depth, 1);
	float3 localDepthPos = mul(_CameraToLocal, viewDepthPos).xyz;
	far = min(far, length(localDepthPos - _CameraLocalPos.xyz));
#endif

	float depthAlongView = far - near;
	int steps = depthAlongView * MAX_STEPS;
	steps = min (steps, MAX_STEPS);
	float stepLength = depthAlongView / float(steps);
	float3 stepVec = stepLength * dir;
	float3 frontPos = _CameraLocalPos.xyz + dir * near;

	float3 temp = 0;
	for (int i = 0; i < steps; i++)
	{
		float3 pos = frontPos + i * stepVec;
		temp += tex2Dlod(_Fire, float4(FireUV(pos), 0, 0)).rgb;
	}

	return max(0.0, temp.xyzz * stepLength) * _Brightness * _Color;
}

ENDCG
    }
}
}
