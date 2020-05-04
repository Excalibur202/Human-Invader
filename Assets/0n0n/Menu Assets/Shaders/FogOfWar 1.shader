Shader "Unlit/FogOfWar 1"
{
    Properties
    {
		/*_PlayerPos("Player Position", Vector) = (0,0,0)*/
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SecondaryTex("Second Albedo", 2D) = "black" {}
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
			sampler2D _SecondaryTex;
            float4 _MainTex_ST;
			float4 _SecondaryTex_ST;
			/*float3 _PlayerPos;*/
			

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
         
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_SecondaryTex, i.uv) * tex2D(_MainTex, i.uv);
				
				/*if (distance(i.uv.xy * 200, _PlayerPos.xy) < 10)
				{
					col = tex2D(_MainTex, i.uv);
					
				}*/
                return col;
            }
            ENDCG
        }
    }
}
