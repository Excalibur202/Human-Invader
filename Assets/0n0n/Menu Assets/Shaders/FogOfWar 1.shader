Shader "Unlit/FogOfWar 1"
{
    Properties
	{
		/*_PlayerPos("Player Position", Vector) = (0,0,0)*/
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SecondaryTex("Second Albedo", 2D) = "black" {}
		_TiltX("offset X", Range(-2,2)) = 0
		_TiltY("offset Y", Range(-2,2)) = 0
		_Scale("Scale", Range(-1,2)) = 1
		_PlayerTexture("Player Texture", 2D) = "white" {}
		_ConsoleTexture("Console Texture", 2D) = "white" {}
		_PlayerPos("Player", Vector) = (0,0,0)
		_ConsolePos("Console", Vector) = (0,0,0)

		_TiltPlayerX("Offset Player X", Range(-2,2)) = 0
		_TiltPlayerY("Offset Payer Y", Range(-2,2)) = 0
		_ScalePlayer("Scale Player", Range(-1,50)) = 0
		
		
		_FuncInfo("Function Information", Vector) = (0,0,0)
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
			sampler2D _PlayerTexture;
			sampler2D _ConsoleTexture;
            float4 _MainTex_ST;
			float4 _SecondaryTex_ST;
			float4 _PlayerTexture_ST;
			float4 _ConsoleTexture_ST;
			half _TiltX;
			half _TiltY;
			half _Scale;
			half _TiltPlayerX;
			half _TiltPlayerY;
			half _ScalePlayer;
			
			
			float3 _PlayerPos;
			float3 _ConsolePos;
			
			float3 _FuncInfo;
			
			float worldToMap(float wl)
			{

			}

			float y(float x)
			{
				return (x - _FuncInfo.y * 0.5) * (-_FuncInfo.x);
			}

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
				
				float2 uvPlayer = float2(uv.x + y(_PlayerPos.x), uv.y + y(_PlayerPos.z));
				uvPlayer = uvPlayer * (1 + _ScalePlayer) - (_ScalePlayer * 0.5);

				fixed4 pl = tex2D(_PlayerTexture, uvPlayer);
				fixed4 col;

				if (pl.a > 0)
					col = pl;
				else
					col = tex2D(_SecondaryTex, uv) * tex2D(_MainTex, uv);

                return col;
            }
            ENDCG
        }
    }
}
