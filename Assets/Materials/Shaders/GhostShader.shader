Shader "Custom/HologramShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Offset("Offset", Range(0,20)) = 0.5
    }
    SubShader
    {        
        Tags { "RenderType"="Opaque" "Queue" = "Transparent"}
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard no alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 viewDir;
        };

        fixed4 _Color;
        float _Offset;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float dotp = pow(dot(IN.viewDir, o.Normal), _Offset);

            o.Emission = _Color;
            o.Alpha = 1 - dotp;
        }
        ENDCG
    }
    FallBack "Diffuse"
}