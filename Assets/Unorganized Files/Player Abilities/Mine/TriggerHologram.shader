Shader "Custom/TriggerHologram"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Offset("Offset", Range(0,20)) = 0.5
        _Range("Range", Range(0,1)) = 1
        _EdgeWidth("EdgeWidth", Range(0,1)) = 0.05
        _UpVector("Up Vector", Vector) = (0,1,0)
        [Toggle]
        _Invert("Invert", Range(0, 1)) = 1
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
        float _Range;
        float _EdgeWidth;
        float _Invert;
        float3 _UpVector;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Emission = _Color;

            float dotp = dot(IN.viewDir, o.Normal);
            if(_Invert)
                o.Alpha = 1 - pow(max(dotp, 0.5), _Offset);
            else
                o.Alpha = pow(max(dotp, 0.5), _Offset);

            float cutoff = _Range * 2 - 1;
            float dotn = dot(_UpVector, o.Normal);
            if(dotn > cutoff)
                o.Alpha = 0;
            else if(abs(dotn - cutoff) < _EdgeWidth)
                o.Alpha += 2 * _EdgeWidth;
        }
        ENDCG
    }
}
