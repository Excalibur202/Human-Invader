Shader "Custom/DoubleMask"
{
    Properties
    {
        _TextureOne("Mask 1", 2D) = "white" {}
        _TextureTwo("Mask 2", 2D) = "white" {}

        _EmissionMap("Emission Map", 2D) = "black" {}
       [HDR] _EmissionColorOne("Emission Color One", Color) = (1,1,1,1)
       [HDR] _EmissionColorTwo("Emission Color Two", Color) = (1,1,1,1)
       [HDR] _EmissionColorThree("Emission Color Three", Color) = (1,1,1,1)




        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalIntensity("Normal Intensity", Range(0,1)) = 0.5

        _ColorOne ("Color One", Color) = (1,1,1,1)
        _GlossinessOne ("Smoothness One", Range(0,1)) = 0.5
        _MetallicOne ("Metallic One", Range(0,1)) = 0.0

        _ColorTwo("Color Two", Color) = (1,1,1,1)
        _GlossinessTwo("Smoothness Two", Range(0,1)) = 0.5
        _MetallicTwo("Metallic Two", Range(0,1)) = 0.0

        _ColorThree("Color Three", Color) = (1,1,1,1)
        _GlossinessThree("Smoothness Three", Range(0,1)) = 0.5
        _MetallicThree("Metallic Three", Range(0,1)) = 0.0

        _ColorFour("Color Four", Color) = (1,1,1,1)
        _GlossinessFour("Smoothness Four", Range(0,1)) = 0.5
        _MetallicFour("Metallic Four", Range(0,1)) = 0.0

        _ColorFive("Color Five", Color) = (1,1,1,1)
        _GlossinessFive("Smoothness Five", Range(0,1)) = 0.5
        _MetallicFive("Metallic Five", Range(0,1)) = 0.0

        _ColorSix("Color Six", Color) = (1,1,1,1)
        _GlossinessSix("Smoothness Six", Range(0,1)) = 0.5
        _MetallicSix("Metallic Six", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _TextureOne;
        sampler2D _TextureTwo;
        sampler2D _EmissionMap;
        sampler2D _NormalMap;

        struct Input
        {
            float2 uv_TextureOne;
            float2 uv_TextureTwo;
            float2 uv_NormalMap;
            float2 uv_EmissionMap;
        };

        fixed4 _EmissionColorOne;
        fixed4 _EmissionColorTwo;
        fixed4 _EmissionColorThree;

        half _GlossinessOne;
        half _MetallicOne;
        fixed4 _ColorOne;

        half _GlossinessTwo;
        half _MetallicTwo;
        fixed4 _ColorTwo;

        half _GlossinessThree;
        half _MetallicThree;
        fixed4 _ColorThree;

        half _GlossinessFour;
        half _MetallicFour;
        fixed4 _ColorFour;

        half _GlossinessFive;
        half _MetallicFive;
        fixed4 _ColorFive;

        half _GlossinessSix;
        half _MetallicSix;
        fixed4 _ColorSix;

        half _NormalIntensity;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 maskone = tex2D(_TextureOne, IN.uv_TextureOne);
            fixed4 masktwo = tex2D(_TextureTwo, IN.uv_TextureTwo);
            fixed4 emissionMap = tex2D(_EmissionMap, IN.uv_EmissionMap);

            o.Albedo = _ColorOne * maskone.r 
                        + _ColorTwo * maskone.g 
                        + _ColorThree * maskone.b 
                        + _ColorFour * masktwo.r 
                        + _ColorFive * masktwo.g 
                        + _ColorSix * masktwo.b;

            o.Metallic = _MetallicOne* maskone.r 
                        + _MetallicTwo* maskone.g 
                        + _MetallicThree* maskone.b 
                        + _MetallicFour * masktwo.r 
                        + _MetallicFive * masktwo.g 
                        + _MetallicSix * masktwo.b;

            o.Smoothness = _GlossinessOne * maskone.r 
                        + _GlossinessTwo * maskone.g 
                        + _GlossinessThree * maskone.b 
                        + _GlossinessFour * masktwo.r 
                        + _GlossinessFive * masktwo.g 
                        + _GlossinessSix * masktwo.b; 

            o.Alpha = 1;

            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            
            o.Emission = emissionMap.r * _EmissionColorOne + emissionMap.g * _EmissionColorTwo + emissionMap.b * _EmissionColorThree;


        }
        ENDCG
    }
    FallBack "Diffuse"
}
