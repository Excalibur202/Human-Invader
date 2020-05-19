Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Delta("Delta", Range(0.0, 1.0)) = 0.0
        _PannerSpeed("Panner Speed", Float) = 1.0
        _TraceColor ("Trace Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }

        GrabPass
        {
            "_BackgroundTexture"
        }

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 grabPos: TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                half3 worldNormal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TraceColor;
            float _PannerSpeed;
            float _Delta;
            sampler2D _BackgroundTexture;


            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
                o.vertex = UnityObjectToClipPos(v.vertex);//+ float4(UnityObjectToWorldNormal(normal)*2.0f,1)*o.uv.x;
                o.worldNormal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //sample the texture
                fixed4 col = tex2D(_MainTex, i.uv+_Time.y*_PannerSpeed);
                fixed4 col2 = tex2D(_MainTex, i.uv - _Time.y * _PannerSpeed);

                float coll = (col.x + col2.x) * 0.25;


                fixed4 final_color;
                //apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                
                if (_Delta < 0.2) {
                    half4 bgcolor = tex2Dproj(_BackgroundTexture, float4(i.grabPos, 0, 0));
                    final_color = (i.uv.x < _Delta * 5) ? _TraceColor+coll.x : bgcolor;
                }
                else {
                    final_color = (i.uv.x > _Delta) ? _TraceColor + coll.x: 0.33+coll;
                }

               // (i.uv.x < (_Delta*2)) ? 1 : 0;


                //return half4(0,i.uv.x,0,1);

                return final_color;
            }
            ENDCG
        }
    }
}
