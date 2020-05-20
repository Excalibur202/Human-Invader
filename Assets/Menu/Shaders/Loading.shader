Shader "Unlit/Loading"
{
    Properties
    {
        _WhiteCard("White card", 2D) = "white" {}
		_PrimaryTex("Texture", 2D) = "white" {}
		_SecondaryTex("Texture", 2D) = "white" {}
		_PrimaryColor("Color", Color) = (0,0,0,1)
		_SecondaryColor("Color", Color) = (0,0,0,1)
		_Freq("Freq", Range(0,1)) = 0
		_Amp("Amp", Range(0,1)) = 0
		_Speed("Speed", Range(0,1000)) = 0
		_FreqFrag("Frequence Frag", Range(0,1)) = 0
		_AmpFrag("Amplitude Frag", Range(0,1)) = 0
		_SpeedFrag("Sped Frag", Range(0,1000)) = 0
    }
    SubShader
    {
		Tags {"Queue" = "Transparent"}
        
		 Blend SrcAlpha OneMinusSrcAlpha
	   

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

            sampler2D _WhiteCard;
            float4 _WhiteCard_ST;
			sampler2D _PrimaryTex;
			float4 _PrimaryTex_ST;
			sampler2D _SecondaryTex;
			float4 _SecondaryTex_ST;

			float4 _PrimaryColor;
			float4 _SecondaryColor;

			half _Freq;
			half _Amp;
			half _Speed;
			half _FreqFrag;
			half _AmpFrag;
			half _SpeedFrag;

            v2f vert (appdata v)
            {
                v2f o;

				float t = _Time * _Speed;	//Tempo
				float waveHeight = sin(t + v.vertex.x * _Freq) * _Amp;

				v.vertex.z += waveHeight;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _WhiteCard);
				/*o.waveHeight = waveHeight;*/

                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				float t = _Time * _SpeedFrag;	//Tempo
				float waveHeight = sin(t + i.vertex.x * _FreqFrag) * _AmpFrag;

				fixed4 primCol = tex2D(_PrimaryTex, i.uv);
				fixed4 seconCol = tex2D(_SecondaryTex, i.uv);
				fixed4 col = tex2D(_WhiteCard, i.uv);

				if (primCol.a > 0)
				{
					col = primCol * _PrimaryColor;
					
				}
				if (seconCol.a > 0)
				{

					 col = seconCol * _SecondaryColor;
				}
				
				if (col.a > 0)
				{
					col.a -= waveHeight;
				}
                return col;
            }
            ENDCG
        }
    }
}
