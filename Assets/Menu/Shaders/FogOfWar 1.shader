Shader "Unlit/FogOfWar 1"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SecondaryTex("Second Albedo", 2D) = "black" {}
		_OffsetX("offset X", Range(-2,2)) = 0
		_OffsetY("offset Y", Range(-2,2)) = 0
		_Scale("Scale", Range(-1,2)) = 1
		_PlayerTexture("Player Texture", 2D) = "white" {}
		_PlayerPos("Player", Vector) = (0,0,0)
		_ScalePlayer("Scale Player", Range(-1,100)) = 0

		_FuncInfo("Function Information", Vector) = (0,0,0)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }


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
				float4 _MainTex_ST;
				float4 _SecondaryTex_ST;
				float4 _PlayerTexture_ST;
				half _OffsetX;
				half _OffsetY;
				half _Scale;
				half _ScalePlayer;

				float3 _PlayerPos;
				float3 _FuncInfo;


				float2 worldToTex(float x,float y)
				{
					return float2((((x + 0.5) / _FuncInfo.x) - 0.5), (((y + 0.5) / _FuncInfo.y) - 0.5))*(-1);
					/*return (x - _FuncInfo.y * 0.5) * (-_FuncInfo.x);*/
				}

				float texPosNormalization(float value)
				{
					return value + 0.5;
				}

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//Map uv
					float2 uv = float2(((i.uv.x + _OffsetX)), ((i.uv.y + _OffsetY)));
					uv = (uv*(1 + _Scale)) - (_Scale * float2(texPosNormalization(_OffsetX), texPosNormalization(_OffsetY)));

					//Player uv
					float2 playerTexPos = worldToTex(_PlayerPos.x, _PlayerPos.z);
					float2 uvPlayer = float2(uv.x + playerTexPos.x, uv.y + playerTexPos.y);
					uvPlayer = uvPlayer * (1 + _ScalePlayer) - (_ScalePlayer * 0.5);

					fixed4 col = tex2D(_PlayerTexture, uvPlayer);
					if (col.a == 0)
						col = tex2D(_SecondaryTex, uv) * tex2D(_MainTex, uv);

					return col;
				}
				ENDCG
			}
		}
}
