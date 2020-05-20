Shader "Unlit/HealthBar"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
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
        Tags { "RenderType"="Opaque" }
        

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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _EmissionTex;
			float4 _EmissionTex_ST;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				/*fixed4 col = tex2D(_MainTex, i.uv);*/
                
				float s = _Speed * _Time.y;
				_TiltY = cos(_Time.y * _SpeedY) * _DeltaY;
				float2 uv = float2(_ScaleX * (i.uv.x + s * _TiltX), _ScaleY * (i.uv.y + /*s * */ _TiltY));

				// Albedo comes from a texture tinted by color
				fixed4 col = tex2D(_MainTex, uv) * _Color;
				/*o.Albedo = c.rgb;*/
				// Metallic and smoothness come from slider variables
				float emissionAnime = lerp(_EmissiveMin, _EmissiveMax, (cos(_Time.y * _EmissiveAnimation) + 1)) * _Emissive;
				col *= emissionAnime;
				/*fixed4 col = tex2D(_EmissionTex, uv).rgb * emissionAnime * c.rgb;*/
				/*o.Alpha = c.a;*/

                return col;
            }
            ENDCG
        }
    }
}
