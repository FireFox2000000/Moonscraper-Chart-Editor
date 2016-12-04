Shader "Hidden/MKGlowBlurVHP"
{
	Properties{_Color ("", Color) = (1,1,1,1)}

	Subshader
	{
		ZTest Off
		Fog{ Mode Off }
		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
			Blend One Zero
			Name "HB"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			uniform sampler2D _MainTex;
			uniform fixed4 _Color;
			uniform half _Shift;
			uniform fixed4 _MainTex_TexelSize;

			struct Input
			{
				float4 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
			};

			struct Output
			{
				float4 pos : SV_POSITION;
				half4 uv : TEXCOORD0;
				half off : TEXCOORD1;
			};

			Output vert(Input i)
			{
				Output o;

				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.texcoord;
				o.off = _MainTex_TexelSize.x * _Shift;
				return o;
			}

			fixed4 frag(Output i) : SV_TARGET
			{
				fixed4 c = fixed4(0,0,0,0);

				for (int r = 0; r < 4; r++)
				{
					half2 offUv = half2(i.off, 0);
					c += tex2D(_MainTex, i.uv + offUv * r);
					c += tex2D(_MainTex, i.uv - offUv * r);
				}
				c /= 8;
				c.rgb = c.rgb * _Color.rgb;

				return c * _Color.a;
			}

		ENDCG
		}

		Pass
		{
			Blend One One
			Name "VB"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			uniform sampler2D _MainTex;
			uniform fixed4 _Color;
			uniform half _Shift;
			uniform fixed4 _MainTex_TexelSize;

			struct Input
			{
				float4 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
			};

			struct Output
			{
				float4 pos : SV_POSITION;
				half4 uv : TEXCOORD0;
				half off : TEXCOORD1;
			};

			Output vert(Input i)
			{
				Output o;

				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.texcoord;
				o.off = _MainTex_TexelSize.y * _Shift;
				return o;
			}

			fixed4 frag(Output i) : SV_TARGET
			{
				fixed4 c = fixed4(0,0,0,0);
				
				for (int r = 0; r < 4; r++)
				{
					half2 offUv = half2(0, i.off);
					c += tex2D(_MainTex, i.uv + offUv * r);
					c += tex2D(_MainTex, i.uv - offUv * r);
				}
				c /= 8;
				c.rgb = c.rgb * _Color.rgb;

				return c * _Color.a;
			}

			ENDCG
		}
	}
	Fallback off
}