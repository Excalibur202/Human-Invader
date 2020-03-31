// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Triplebrick/Base_Normal_Replace_LWRP"
{
    Properties
    {
		_BaseNormal("Base Normal", 2D) = "white" {}
		_DirtRoughness("Dirt Roughness", 2D) = "white" {}
		_DetailNormal("Detail Normal", 2D) = "white" {}
		_DetailMask("Detail Mask", 2D) = "white" {}
		_BaseColor("Base Color", Color) = (0.6544118,0.6544118,0.6544118,0)
		_BaseColorOverlay("Base Color Overlay", Color) = (0.6544118,0.6544118,0.6544118,0)
		_BaseDirtColor("Base Dirt Color", Color) = (0,0,0,0)
		_DetailColor("Detail Color", Color) = (0,0,0,0)
		_BaseNormalStrength("Base Normal Strength", Range( 0 , 1)) = 0
		_BaseSmoothness("Base Smoothness", Range( 0 , 1)) = 0.5
		_BaseDirtStrength("Base Dirt Strength", Range( 0.001 , 3)) = 0
		_BaseMetallic("Base Metallic", Range( 0 , 1)) = 0
		_DetailEdgeWear("Detail Edge Wear", Range( 0 , 1)) = 0
		_DetailEdgeSmoothness("Detail Edge Smoothness", Range( 0 , 1)) = 0
		_DetailDirtStrength("Detail Dirt Strength", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

    }


    SubShader
    {
		LOD 0

		
        Tags { "RenderPipeline"="LightweightPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL
		
        Pass
        {
			
        	Tags { "LightMode"="LightweightForward" }

        	Name "Base"
			Blend One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
            
        	HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #define ASE_FOG 1
            #define ASE_SRP_VERSION 60902
            #define _NORMALMAP 1

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

        	// -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            
        	// -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
        	#pragma fragment frag

        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
		
			

			sampler2D _DirtRoughness;
			sampler2D _DetailMask;
			sampler2D _DetailNormal;
			sampler2D _BaseNormal;
			CBUFFER_START( UnityPerMaterial )
			float4 _BaseDirtColor;
			float4 _DirtRoughness_ST;
			float _BaseDirtStrength;
			float4 _BaseColor;
			float4 _BaseColorOverlay;
			float4 _DetailColor;
			float4 _DetailMask_ST;
			float _DetailDirtStrength;
			float _DetailEdgeWear;
			float4 _DetailNormal_ST;
			float4 _BaseNormal_ST;
			float _BaseNormalStrength;
			float _BaseMetallic;
			float _BaseSmoothness;
			float _DetailEdgeSmoothness;
			CBUFFER_END


            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
                float4 ase_tangent : TANGENT;
                float4 texcoord1 : TEXCOORD1;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct GraphVertexOutput
            {
                float4 clipPos                : SV_POSITION;
                float4 lightmapUVOrVertexSH	  : TEXCOORD0;
        		half4 fogFactorAndVertexLight : TEXCOORD1; // x: fogFactor, yzw: vertex light
            	float4 shadowCoord            : TEXCOORD2;
				float4 tSpace0					: TEXCOORD3;
				float4 tSpace1					: TEXCOORD4;
				float4 tSpace2					: TEXCOORD5;
				float4 ase_texcoord7 : TEXCOORD7;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            	UNITY_VERTEX_OUTPUT_STEREO
            };

			
            GraphVertexOutput vert (GraphVertexInput v  )
        	{
        		GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
            	UNITY_TRANSFER_INSTANCE_ID(v, o);
        		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord7.xy = v.ase_texcoord3.xy;
				o.ase_texcoord7.zw = v.ase_texcoord.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue =  defaultVertexValue ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal =  v.ase_normal ;

        		// Vertex shader outputs defined by graph
                float3 lwWNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
				float3 lwWTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float3 lwWBinormal = normalize(cross(lwWNormal, lwWTangent) * v.ase_tangent.w);
				o.tSpace0 = float4(lwWTangent.x, lwWBinormal.x, lwWNormal.x, lwWorldPos.x);
				o.tSpace1 = float4(lwWTangent.y, lwWBinormal.y, lwWNormal.y, lwWorldPos.y);
				o.tSpace2 = float4(lwWTangent.z, lwWBinormal.z, lwWNormal.z, lwWorldPos.z);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                
         		// We either sample GI from lightmap or SH.
        	    // Lightmap UV and vertex SH coefficients use the same interpolator ("float2 lightmapUV" for lightmap or "half3 vertexSH" for SH)
                // see DECLARE_LIGHTMAP_OR_SH macro.
        	    // The following funcions initialize the correct variable with correct data
        	    OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
        	    OUTPUT_SH(lwWNormal, o.lightmapUVOrVertexSH.xyz);

        	    half3 vertexLight = VertexLighting(vertexInput.positionWS, lwWNormal);
			#ifdef ASE_FOG
        	    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
			#else
				half fogFactor = 0;
			#endif
        	    o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
        	    o.clipPos = vertexInput.positionCS;

        	#ifdef _MAIN_LIGHT_SHADOWS
        		o.shadowCoord = GetShadowCoord(vertexInput);
        	#endif
        		return o;
        	}

        	half4 frag (GraphVertexOutput IN  ) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

        		float3 WorldSpaceNormal = normalize(float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z));
				float3 WorldSpaceTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldSpaceBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldSpacePosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldSpaceViewDirection = SafeNormalize( _WorldSpaceCameraPos.xyz  - WorldSpacePosition );
    
				float2 uv4_DirtRoughness = IN.ase_texcoord7.xy * _DirtRoughness_ST.xy + _DirtRoughness_ST.zw;
				float4 tex2DNode143 = tex2D( _DirtRoughness, uv4_DirtRoughness );
				float clampResult77 = clamp( pow( tex2DNode143.g , _BaseDirtStrength ) , 0.0 , 1.0 );
				float4 lerpResult76 = lerp( _BaseDirtColor , float4( 1,1,1,0 ) , clampResult77);
				float4 lerpResult71 = lerp( _BaseColor , _BaseColorOverlay , tex2DNode143.r);
				float2 uv_DetailMask = IN.ase_texcoord7.zw * _DetailMask_ST.xy + _DetailMask_ST.zw;
				float4 tex2DNode36 = tex2D( _DetailMask, uv_DetailMask );
				float4 lerpResult134 = lerp( lerpResult71 , _DetailColor , ceil( ( ( 1.0 - tex2DNode36.b ) + -0.95 ) ));
				float temp_output_120_0 = ceil( ( tex2DNode36.b + -0.8 ) );
				float4 lerpResult123 = lerp( lerpResult134 , float4( float3(1,0.95,0.9) , 0.0 ) , temp_output_120_0);
				float clampResult49 = clamp( pow( ( tex2DNode36.r + 0.55 ) , 6.0 ) , 0.0 , 1.0 );
				float4 lerpResult92 = lerp( lerpResult123 , ( lerpResult123 * clampResult49 ) , _DetailDirtStrength);
				float clampResult62 = clamp( ( ( tex2DNode36.r + -0.55 ) * 2.0 ) , 0.0 , 1.0 );
				float4 clampResult101 = clamp( ( clampResult62 + lerpResult92 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 lerpResult32 = lerp( lerpResult92 , clampResult101 , _DetailEdgeWear);
				
				float2 uv_DetailNormal = IN.ase_texcoord7.zw * _DetailNormal_ST.xy + _DetailNormal_ST.zw;
				float3 tex2DNode3 = UnpackNormalScale( tex2D( _DetailNormal, uv_DetailNormal ), 1.0f );
				float2 uv4_BaseNormal = IN.ase_texcoord7.xy * _BaseNormal_ST.xy + _BaseNormal_ST.zw;
				float3 lerpResult11 = lerp( float3(0,0,1) , UnpackNormalScale( tex2D( _BaseNormal, uv4_BaseNormal ), 1.0f ) , _BaseNormalStrength);
				float2 uv0104 = IN.ase_texcoord7.zw * float2( 1,1 ) + float2( 0,0 );
				float clampResult111 = clamp( ( uv0104.x + -0.92 ) , 0.0 , 1.0 );
				float clampResult112 = clamp( ( uv0104.y + -0.92 ) , 0.0 , 1.0 );
				float3 lerpResult115 = lerp( tex2DNode3 , lerpResult11 , ( ceil( clampResult111 ) * ceil( clampResult112 ) ));
				
				float lerpResult94 = lerp( 0.0 , clampResult62 , _DetailEdgeWear);
				float clampResult97 = clamp( ( lerpResult94 + temp_output_120_0 ) , 0.0 , 1.0 );
				float clampResult144 = clamp( ( clampResult97 + _BaseMetallic ) , 0.0 , 1.0 );
				
				float4 temp_cast_2 = (_DetailEdgeSmoothness).xxxx;
				float4 lerpResult121 = lerp( ( ( max( clampResult62 , tex2DNode143.a ) * lerpResult76 ) * _BaseSmoothness ) , temp_cast_2 , clampResult97);
				
				
		        float3 Albedo = ( lerpResult76 * lerpResult32 ).rgb;
				float3 Normal = lerpResult115;
				float3 Emission = 0;
				float3 Specular = float3(0.5, 0.5, 0.5);
				float Metallic = clampResult144;
				float Smoothness = lerpResult121.r;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0;

        		InputData inputData;
        		inputData.positionWS = WorldSpacePosition;

        #ifdef _NORMALMAP
        	    inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal)));
        #else
            #if !SHADER_HINT_NICE_QUALITY
                inputData.normalWS = WorldSpaceNormal;
            #else
        	    inputData.normalWS = normalize(WorldSpaceNormal);
            #endif
        #endif

			#if !SHADER_HINT_NICE_QUALITY
        	    // viewDirection should be normalized here, but we avoid doing it as it's close enough and we save some ALU.
        	    inputData.viewDirectionWS = WorldSpaceViewDirection;
			#else
        	    inputData.viewDirectionWS = normalize(WorldSpaceViewDirection);
			#endif

        	    inputData.shadowCoord = IN.shadowCoord;
			#ifdef ASE_FOG
        	    inputData.fogCoord = IN.fogFactorAndVertexLight.x;
			#endif
        	    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
        	    inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS);

        		half4 color = LightweightFragmentPBR(
        			inputData, 
        			Albedo, 
        			Metallic, 
        			Specular, 
        			Smoothness, 
        			Occlusion, 
        			Emission, 
        			Alpha);

		#ifdef ASE_FOG
			#ifdef TERRAIN_SPLAT_ADDPASS
				color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
			#else
				color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
			#endif
		#endif

        #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

		#if ASE_LW_FINAL_COLOR_ALPHA_MULTIPLY
				color.rgb *= color.a;
		#endif
		
		#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
		#endif
        		return color;
            }

        	ENDHLSL
        }

		
        Pass
        {
			
        	Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #define ASE_FOG 1
            #define ASE_SRP_VERSION 60902

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			CBUFFER_START( UnityPerMaterial )
			float4 _BaseDirtColor;
			float4 _DirtRoughness_ST;
			float _BaseDirtStrength;
			float4 _BaseColor;
			float4 _BaseColorOverlay;
			float4 _DetailColor;
			float4 _DetailMask_ST;
			float _DetailDirtStrength;
			float _DetailEdgeWear;
			float4 _DetailNormal_ST;
			float4 _BaseNormal_ST;
			float _BaseNormalStrength;
			float _BaseMetallic;
			float _BaseSmoothness;
			float _DetailEdgeSmoothness;
			CBUFFER_END


        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
        	};

			
            // x: global clip space bias, y: normal world space bias
            float3 _LightDirection;

            VertexOutput ShadowPassVertex(GraphVertexInput v )
        	{
        	    VertexOutput o;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO (o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue =  defaultVertexValue ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

        	    float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

                float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                float scale = invNdotL * _ShadowBias.y;

                // normal bias is negative since we want to apply an inset normal offset
                positionWS = _LightDirection * _ShadowBias.xxx + positionWS;
				positionWS = normalWS * scale.xxx + positionWS;
                float4 clipPos = TransformWorldToHClip(positionWS);

                // _ShadowBias.x sign depens on if platform has reversed z buffer
                //clipPos.z += _ShadowBias.x;

        	#if UNITY_REVERSED_Z
        	    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        	#else
        	    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        	#endif
                o.clipPos = clipPos;

        	    return o;
        	}

            half4 ShadowPassFragment(VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

               

				float Alpha = 1;
				float AlphaClipThreshold = AlphaClipThreshold;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

		#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
		#endif
				return 0;
            }

            ENDHLSL
        }

		
        Pass
        {
			
        	Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
			ColorMask 0

            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #define ASE_FOG 1
            #define ASE_SRP_VERSION 60902

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            

			CBUFFER_START( UnityPerMaterial )
			float4 _BaseDirtColor;
			float4 _DirtRoughness_ST;
			float _BaseDirtStrength;
			float4 _BaseColor;
			float4 _BaseColorOverlay;
			float4 _DetailColor;
			float4 _DetailMask_ST;
			float _DetailDirtStrength;
			float _DetailEdgeWear;
			float4 _DetailNormal_ST;
			float4 _BaseNormal_ST;
			float _BaseNormalStrength;
			float _BaseMetallic;
			float _BaseSmoothness;
			float _DetailEdgeSmoothness;
			CBUFFER_END


            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

			           

            VertexOutput vert(GraphVertexInput v  )
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue =  defaultVertexValue ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

        	    o.clipPos = TransformObjectToHClip(v.vertex.xyz);
        	    return o;
            }

            half4 frag(VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

				

				float Alpha = 1;
				float AlphaClipThreshold = AlphaClipThreshold;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif
		#ifdef LOD_FADE_CROSSFADE
				LODDitheringTransition (IN.clipPos.xyz, unity_LODFade.x);
		#endif
				return 0;
            }
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
		
        Pass
        {
			
        	Name "Meta"
            Tags { "LightMode"="Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #define ASE_FOG 1
            #define ASE_SRP_VERSION 60902

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

			
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/MetaInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            

			sampler2D _DirtRoughness;
			sampler2D _DetailMask;
			CBUFFER_START( UnityPerMaterial )
			float4 _BaseDirtColor;
			float4 _DirtRoughness_ST;
			float _BaseDirtStrength;
			float4 _BaseColor;
			float4 _BaseColorOverlay;
			float4 _DetailColor;
			float4 _DetailMask_ST;
			float _DetailDirtStrength;
			float _DetailEdgeWear;
			float4 _DetailNormal_ST;
			float4 _BaseNormal_ST;
			float _BaseNormalStrength;
			float _BaseMetallic;
			float _BaseSmoothness;
			float _DetailEdgeSmoothness;
			CBUFFER_END


            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            
            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

			
            VertexOutput vert(GraphVertexInput v  )
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord.xy = v.ase_texcoord3.xy;
				o.ase_texcoord.zw = v.ase_texcoord.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = v.vertex.xyz;
				#else
				float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue =  defaultVertexValue ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;
#if !defined( ASE_SRP_VERSION ) || ASE_SRP_VERSION  > 51300				
                o.clipPos = MetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST);
#else
				o.clipPos = MetaVertexPosition (v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST);
#endif
        	    return o;
            }

            half4 frag(VertexOutput IN  ) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

           		float2 uv4_DirtRoughness = IN.ase_texcoord.xy * _DirtRoughness_ST.xy + _DirtRoughness_ST.zw;
           		float4 tex2DNode143 = tex2D( _DirtRoughness, uv4_DirtRoughness );
           		float clampResult77 = clamp( pow( tex2DNode143.g , _BaseDirtStrength ) , 0.0 , 1.0 );
           		float4 lerpResult76 = lerp( _BaseDirtColor , float4( 1,1,1,0 ) , clampResult77);
           		float4 lerpResult71 = lerp( _BaseColor , _BaseColorOverlay , tex2DNode143.r);
           		float2 uv_DetailMask = IN.ase_texcoord.zw * _DetailMask_ST.xy + _DetailMask_ST.zw;
           		float4 tex2DNode36 = tex2D( _DetailMask, uv_DetailMask );
           		float4 lerpResult134 = lerp( lerpResult71 , _DetailColor , ceil( ( ( 1.0 - tex2DNode36.b ) + -0.95 ) ));
           		float temp_output_120_0 = ceil( ( tex2DNode36.b + -0.8 ) );
           		float4 lerpResult123 = lerp( lerpResult134 , float4( float3(1,0.95,0.9) , 0.0 ) , temp_output_120_0);
           		float clampResult49 = clamp( pow( ( tex2DNode36.r + 0.55 ) , 6.0 ) , 0.0 , 1.0 );
           		float4 lerpResult92 = lerp( lerpResult123 , ( lerpResult123 * clampResult49 ) , _DetailDirtStrength);
           		float clampResult62 = clamp( ( ( tex2DNode36.r + -0.55 ) * 2.0 ) , 0.0 , 1.0 );
           		float4 clampResult101 = clamp( ( clampResult62 + lerpResult92 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
           		float4 lerpResult32 = lerp( lerpResult92 , clampResult101 , _DetailEdgeWear);
           		
				
		        float3 Albedo = ( lerpResult76 * lerpResult32 ).rgb;
				float3 Emission = 0;
				float Alpha = 1;
				float AlphaClipThreshold = 0;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = Albedo;
                metaInput.Emission = Emission;
                
                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
		
    }
    Fallback "Hidden/InternalErrorShader"
	CustomEditor "ASEMaterialInspector"
	
}
/*ASEBEGIN
Version=17400
0;73;643;681;878.715;235.058;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;18;-3446.678,-1132.657;Inherit;False;515.9005;765.2666;R = Overlay, G = Dirtmap, B = empty , A = Roughness;2;26;27;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;27;-3378.464,-769.8761;Inherit;False;371;280;uv4;1;36;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;36;-3346.899,-727.3181;Inherit;True;Property;_DetailMask;Detail Mask;3;0;Create;True;0;0;False;0;-1;None;4a3848591ce935e4dad9a07fc6696f83;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;26;-3376.254,-1063.391;Inherit;False;371;280;uv0;1;143;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;130;-2928.12,-250.9236;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;131;-2728.081,-271.6658;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;72;-3394.463,442.1131;Float;False;Property;_BaseColorOverlay;Base Color Overlay;5;0;Create;True;0;0;False;0;0.6544118,0.6544118,0.6544118,0;0.9294118,0.9411765,0.9490196,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-3384.122,232.3007;Float;False;Property;_BaseColor;Base Color;4;0;Create;True;0;0;False;0;0.6544118,0.6544118,0.6544118,0;0.9490196,0.945098,0.9294118,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;143;-3339.083,-1003.106;Inherit;True;Property;_DirtRoughness;Dirt Roughness;1;0;Create;True;0;0;False;0;-1;None;9ee49387df9326442af2587f7e7327c3;True;3;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;135;-2727.841,-120.0552;Float;False;Property;_DetailColor;Detail Color;7;0;Create;True;0;0;False;0;0,0,0,0;0.3215685,0.3215685,0.3215685,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;118;-2195.756,499.6233;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;71;-2757.186,328.1992;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CeilOpNode;133;-2596.711,-254.2214;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;50;-2436.12,-512.9489;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.55;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;124;-2305.558,-92.16446;Float;False;Constant;_MetalTrimColor;MetalTrimColor;15;0;Create;True;0;0;False;0;1,0.95,0.9;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CeilOpNode;120;-2036.613,500.8069;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;127;-2250.55,-487.9212;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;6;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;134;-2470.921,75.57092;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-1899.193,-566.8974;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.55;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;49;-2178.536,-328.6187;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;123;-2092.614,134.7373;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-1709.371,39.12919;Float;False;Property;_DetailDirtStrength;Detail Dirt Strength;14;0;Create;True;0;0;False;0;0;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-1647.851,-541.9295;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-1681.178,-329.5637;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;92;-1550.481,-187;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;62;-1504.473,-486.1902;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1825.482,-1086.847;Float;False;Property;_BaseDirtStrength;Base Dirt Strength;10;0;Create;True;0;0;False;0;0;0.01;0.001;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;74;-1533.299,-804.511;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-1299.281,-406.7545;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1382.548,84.26886;Float;False;Property;_DetailEdgeWear;Detail Edge Wear;12;0;Create;True;0;0;False;0;0;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;101;-1131.909,-418.7224;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;75;-1364.064,-1160.842;Float;False;Property;_BaseDirtColor;Base Dirt Color;6;0;Create;True;0;0;False;0;0,0,0,0;0.2039215,0.1647058,0.07450978,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;77;-1339.607,-746.6707;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;76;-1209.608,-885.7709;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;32;-843.7327,-307.658;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;141;-2579.118,1184.372;Float;False;Constant;_Color0;Color 0;17;0;Create;True;0;0;False;0;0.5036319,0.4980392,1,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;138;-29.25116,346.7925;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;144;-229.3649,207.5919;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-3014.076,1273.918;Inherit;True;Property;_DetailNormal;Detail Normal;2;0;Create;True;0;0;False;0;-1;None;1c1221435ae8dd740802e4127090b7fa;True;0;True;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;521.5351,-714.0347;Float;False;Constant;_DetailRoughness;Detail Roughness;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;121;-428.1258,-35.81998;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;115;-1674.393,1046.433;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;6;-1409.035,1314.675;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;137;-361.1563,453.2509;Float;False;Property;_DetailOcclusionStrength;Detail Occlusion Strength;16;0;Create;True;0;0;False;0;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-1495.678,1879.227;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-667.9778,-185.7681;Float;False;Property;_DetailEdgeSmoothness;Detail Edge Smoothness;13;0;Create;True;0;0;False;0;0;0.74;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;11;-2260.987,1516.026;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-652.3083,-38.56518;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;117;-618.9706,462.8683;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;208.0109,-674.2496;Float;False;Property;_DetailRoughnessContrast;Detail Roughness Contrast;15;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-507.6571,-343.7149;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;129;-1135.764,-102.5898;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;114;-1622.688,1908.212;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;104;-2233.729,1844.064;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;94;-1296.066,290.8427;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;107;-1925.507,1957.204;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.92;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;108;-1918.21,1842.53;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.92;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;111;-1762.038,1704.703;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-880.2324,-37.65833;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;112;-1772.688,1948.212;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;-1216.755,522.9928;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;97;-930.2104,482.7545;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-1418.552,817.4737;Float;False;Property;_BaseMetallic;Base Metallic;11;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;113;-1580.688,1754.212;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2556.354,1703.328;Float;False;Property;_BaseNormalStrength;Base Normal Strength;8;0;Create;True;0;0;False;0;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1045.729,75.21457;Float;False;Property;_BaseSmoothness;Base Smoothness;9;0;Create;True;0;0;False;0;0.5;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-3041.785,1516.055;Inherit;True;Property;_BaseNormal;Base Normal;0;0;Create;True;0;0;False;0;-1;None;7d09e7c7fffa5a94eadbfb28633eaa1e;True;3;True;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;10;-2528.757,1410.333;Float;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;False;0;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;147;135.1354,-137.3909;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;1976390536c6c564abb90fe41f6ee334;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;145;135.1354,-137.3909;Float;False;True;-1;2;ASEMaterialInspector;0;10;Triplebrick/Base_Normal_Replace_LWRP;1976390536c6c564abb90fe41f6ee334;True;Base;0;0;Base;11;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=LightweightForward;False;0;Hidden/InternalErrorShader;0;0;Standard;4;Vertex Position,InvertActionOnDeselection;1;Receive Shadows;1;LOD CrossFade;1;Built-in Fog;1;1;_FinalColorxAlpha;0;4;True;True;True;True;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;146;135.1354,-137.3909;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;1976390536c6c564abb90fe41f6ee334;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;148;135.1354,-137.3909;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;1976390536c6c564abb90fe41f6ee334;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;130;0;36;3
WireConnection;131;0;130;0
WireConnection;118;0;36;3
WireConnection;71;0;8;0
WireConnection;71;1;72;0
WireConnection;71;2;143;1
WireConnection;133;0;131;0
WireConnection;50;0;36;1
WireConnection;120;0;118;0
WireConnection;127;0;50;0
WireConnection;134;0;71;0
WireConnection;134;1;135;0
WireConnection;134;2;133;0
WireConnection;51;0;36;1
WireConnection;49;0;127;0
WireConnection;123;0;134;0
WireConnection;123;1;124;0
WireConnection;123;2;120;0
WireConnection;99;0;51;0
WireConnection;45;0;123;0
WireConnection;45;1;49;0
WireConnection;92;0;123;0
WireConnection;92;1;45;0
WireConnection;92;2;93;0
WireConnection;62;0;99;0
WireConnection;74;0;143;2
WireConnection;74;1;21;0
WireConnection;46;0;62;0
WireConnection;46;1;92;0
WireConnection;101;0;46;0
WireConnection;77;0;74;0
WireConnection;76;0;75;0
WireConnection;76;2;77;0
WireConnection;32;0;92;0
WireConnection;32;1;101;0
WireConnection;32;2;33;0
WireConnection;138;1;36;2
WireConnection;138;2;137;0
WireConnection;144;0;117;0
WireConnection;121;0;65;0
WireConnection;121;1;122;0
WireConnection;121;2;97;0
WireConnection;115;0;3;0
WireConnection;115;1;11;0
WireConnection;115;2;109;0
WireConnection;6;0;3;0
WireConnection;6;1;11;0
WireConnection;109;0;113;0
WireConnection;109;1;114;0
WireConnection;11;0;10;0
WireConnection;11;1;2;0
WireConnection;11;2;12;0
WireConnection;65;0;66;0
WireConnection;65;1;40;0
WireConnection;117;0;97;0
WireConnection;117;1;78;0
WireConnection;52;0;76;0
WireConnection;52;1;32;0
WireConnection;129;0;62;0
WireConnection;129;1;143;4
WireConnection;114;0;112;0
WireConnection;94;1;62;0
WireConnection;94;2;33;0
WireConnection;107;0;104;2
WireConnection;108;0;104;1
WireConnection;111;0;108;0
WireConnection;66;0;129;0
WireConnection;66;1;76;0
WireConnection;112;0;107;0
WireConnection;96;0;94;0
WireConnection;96;1;120;0
WireConnection;97;0;96;0
WireConnection;113;0;111;0
WireConnection;145;0;52;0
WireConnection;145;1;115;0
WireConnection;145;3;144;0
WireConnection;145;4;121;0
ASEEND*/
//CHKSM=3E106E1F279552BCC42FC7039DF5CCD6CC3AAADB