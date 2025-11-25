//https://en.wikibooks.org/wiki/Cg_Programming/Unity/Billboards

Shader "Custom/SpriteBillboarding"
{
	Properties
	{
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
			"SortingLayer" = "Resources_Sprites"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
			"DisableBatching" = "True"
		}
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

			struct vertexInput {
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};

			sampler2D _MainTex;
			
			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				output.pos = mul(UNITY_MATRIX_P,
					UnityObjectToViewPos(float4(0.0, 0.0, 0.0, 1.0))
					+ float4(input.vertex.x, input.vertex.y, 0.0, 0.0)
					* float4(1.0, 1.0, 1.0, 1.0));

				output.tex = input.tex;

				return output;
			}
			
			float4 frag(vertexOutput input) : COLOR
			{
				return tex2D(_MainTex, float2(input.tex.xy));
			}
			ENDCG
		}
	}
}
