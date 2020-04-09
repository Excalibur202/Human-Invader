Shader "Custom/HologramShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Offset("Offset", Range(0,20)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200
        
        Pass{
            ZWrite On
            ColorMask 0
        }

        Blend One One

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 _viewDir;
        };

        fixed4 _Color;
        float _Offset;

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 viewDirection = normalize(UNITY_MATRIX_IT_MV[2].xyz);
            float dotp = pow(1 - dot(viewDirection, o.Normal), _Offset);

            o.Emission = dotp * _Color;
            o.Alpha = dotp;
        }
        ENDCG
    }
}