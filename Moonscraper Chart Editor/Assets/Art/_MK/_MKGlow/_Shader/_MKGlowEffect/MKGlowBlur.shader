Shader "Hidden/MKGlowBlur"
{
	Properties{_Color ("", Color) = (1,1,1,1)}

	Subshader
	{
		ZTest Off
		Fog{ Mode Off }
		Cull Off
		Lighting Off
		ZWrite Off

		UsePass "Hidden/MKGlowBlurVHP/HB"
		UsePass "Hidden/MKGlowBlurVHP/VB"
	}
	Fallback off
}