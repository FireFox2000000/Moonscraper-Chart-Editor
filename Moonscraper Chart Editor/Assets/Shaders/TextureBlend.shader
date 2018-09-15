// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Source: http://wiki.unity3d.com/index.php?title=Texture_Mask

Shader "Unlit/TextureBlend"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
        _SecondaryTex("Secondary (RGB)", 2D) = "white" {}
		_Blend("Blend", Range(0,1)) = 0
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
            sampler2D _SecondaryTex;
            float _Blend;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 main_col = tex2D(_MainTex, i.uv);
                fixed4 secondary_col = tex2D(_SecondaryTex, i.uv);

                return lerp(main_col, secondary_col, _Blend);

				//return main_col;
			}

			ENDCG
		}
	}
}
