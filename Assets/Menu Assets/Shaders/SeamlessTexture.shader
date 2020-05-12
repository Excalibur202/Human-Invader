Shader "Custom/SeamlessTexture"
{

	Properties
	{
		_ColorIntensity("Color Intensity", Range(0,1)) = 0
		_EmissionIntensity("Emission Intensity", Range(0,10)) = 1
		_UvOffset("Uv Offset", Range(0,100)) = 0
		_AnimationSpeed("Animation Speed", Range(0, 100)) = 1

		_MainTex("Albedo Texture", 2D) = "white" {}
		_EmissionTex("Emission Texture", 2D) = "white" {}
	}
		SubShader
	{
		CGPROGRAM
		#pragma surface surf Standard

		struct Input {
			float2 uv_MainTex;
			float2 _Time;
		};

		half _ColorIntensity;
		half _UvOffset;
		half _AnimationSpeed;
		half _EmissionIntensity;
		sampler2D _MainTex;
		sampler2D _EmissionTex;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex * _UvOffset).rgb * _ColorIntensity;
			o.Emission = tex2D(_EmissionTex, IN.uv_MainTex * _UvOffset).rgb * lerp(0, 1, (cos(_Time.y * _AnimationSpeed) + 1) * _EmissionIntensity);
		}

		ENDCG
	}
		FallBack "Diffuse"

}
