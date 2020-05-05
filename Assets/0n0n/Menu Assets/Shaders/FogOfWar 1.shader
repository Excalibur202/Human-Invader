Shader "Unlit/FogOfWar 1"
{
    Properties
	{
		/*_PlayerPos("Player Position", Vector) = (0,0,0)*/
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SecondaryTex("Second Albedo", 2D) = "black" {}
		_TiltX("offset X", Range(-2,2)) = 0
		_TiltY("offset Y", Range(-2,2)) = 0
		_Scale("Scale", Range(0,2)) = 1
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
			half _TiltX;
			half _TiltY;
			half _Scale;
			
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
                
				float2 uv = float2(( (i.uv.x + _TiltX)), ( (i.uv.y +  _TiltY)));
				uv = uv * (1 + _Scale) - (_Scale * 0.5);
				

				fixed4 col = tex2D(_SecondaryTex, uv) * tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}
