Shader "Custom/WaveyNoise"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Strengh("Strengh", Range(0,10)) = 0
		_Speed("Speed", Range(0, 10)) = 1.0
        _TiltX("offset X", Range(-1,1)) = 0
		_ScaleX("Scale on X", Range(0,10)) = 1
		_ScaleY("Scale Y", Range(0,10)) = 1
		_Emissive("Emission", Range(0,10)) = 0
		_EmissionTex("Emission Texture", 2D) = "white" {}
		_SpeedY("Speed Y", Range(0,10)) = 0
		_DeltaY("Delta Y", Range(0,5)) = 0
		_EmissiveMin("Emissive Min", Range(0,10)) = 0
		_EmissiveMax("Emissive Max", Range(0,10)) = 0
		_EmissiveAnimation("Emissive Speed", Range(0,10)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _EmissionTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
		half _Strengh;
		half _Speed;
		half _TiltX;
		half _TiltY;
		half _ScaleX;
		half _ScaleY;
		half _Emissive;
		half _SpeedY;
		half _DeltaY;
		half _EmissiveMax;
		half _EmissiveMin;
		half _EmissiveAnimation;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float s = _Speed * _Time.y;
			_TiltY = cos(_Time.y * _SpeedY) * _DeltaY;
			float2 uv = float2(_ScaleX * (IN.uv_MainTex.x + s * _TiltX),_ScaleY * (IN.uv_MainTex.y + /*s * */ _TiltY));
			
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, uv) * _Color;
			o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
			float emissionAnime = lerp(_EmissiveMin, _EmissiveMax, (cos(_Time.y * _EmissiveAnimation) + 1)) * _Emissive;
			o.Emission = tex2D(_EmissionTex, uv).rgb * emissionAnime * c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
