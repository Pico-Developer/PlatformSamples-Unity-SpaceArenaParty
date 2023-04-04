Shader "PicoURP_PBR"
{
    Properties
    {
        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
    	//PicoVideo;LightMode;YangFan;Begin
    	_LightingModel("Lighting Model ", float) = 1.0
    	//PicoVideo;LightMode;YangFan;End

        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        
        //PicoVideo;LightMode;YangFan;Begin
        _EnableAlphaMap("Enable Alpha Map", float) = 0
        _AlphaMap("Alpha Map" , 2D) = "white" {}
        
        _FresnelBias("Fresnel Bias", float) = 0
        _FresnelScale("Fresnel Bias", float) = 1
        _FresnelPower("Fresnel Bias", float) = 4
        //PicoVideo;LightMode;YangFan;End

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax("Scale", Range(0.005, 0.08)) = 0.005
        _ParallaxMap("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        
        //PicoVideo;LightMode;Ernst;Begin
        _LightmapSpecular("LightmapSpecular", Range(0.0, 1.0)) = 1.0
        //PicoVideo;LightMode;Ernst;End
        
        //PicoVideo;LayerBlend;XiaoPengCheng;Begin
        _EnableLayerBlend("Enable Layer Blends", Float) = 0
        _LayerBlendGroup("Layer Blends", Float) = 0
        _LayerBlendStrength("Blend Strength", Range(0.0, 1.0)) = 0.5
        _LayerBlendSmoothness("Blend Smoothness", Range(0.01, 1)) = 1
        _LayerBaseMap("Base Map (RGB), AO(A)", 2D) = "white" {}
        _LayerBaseColor("Base Color", Color) = (1,1,1,1)
        _LayerEnableAlphaMap("Enable Alpha Map", float) = 0
        _LayerAlphaMap("Alpha Map" , 2D) = "white" {}
        _LayerBumpMap("Bump Map(RG), Metallic(B), Smoothness(A)" , 2D) = "white" {}
        _LayerBumpScale("Bump Scale", Float) = 1.0
        _LayerMetallic("Metallic", Range(0.0, 1.0)) = 0.0
        _LayerSmoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _LayerOcclusionStrength("Occlusion", Range(0.0, 1.0)) = 1.0
        //PicoVideo;LayerBlend;XiaoPengCheng;End

        //PicoVideo;Vegetation;ZhengLingFeng;Begin
    	_VegetationMode("Vegetation Mode", float) = 0
    	[Space(10)]_VegetationHideMode("Hide Side", float) = 0
    	[Min(0.001)]_VegetationHidePower("Hide Power", float) = 2
    	[Min(1)]_VegetationHideFalloff("Hide Falloff", float) = 7
        [Header(Gradient Parameters)][Space(10)]_GradientColor("Gradient Color", Color) = (1,1,1,0)
		_GradientFalloff("Gradient Falloff", Range( 0 , 2)) = 2
		_GradientPosition("Gradient Position", Range( 0 , 1)) = 0.5
		[Toggle(_INVERTGRADIENT_ON)] _InvertGradient("Invert Gradient", Float) = 0
        [Space(10)][Header(Color Variation)][Space(10)]_ColorVariation("Color Variation", Color) = (1,0,0,0)
		_ColorVariationPower("Color Variation Power", Range( 0 , 1)) = 1
		_ColorVariationNoise("Color Variation Noise", 2D) = "white" {}
		_NoiseScale("Noise Scale", Float) = 0.5
	    [Space(10)][Header(Wind)][Space(10)]_WindMultiplier("BaseWind Multiplier", Float) = 0
		_MicroWindMultiplier("MicroWind Multiplier", Float) = 1
		_WindTrunkPosition("Wind Trunk Position", Float) = 0
		_WindTrunkContrast("Wind Trunk Contrast", Float) = 10
        //PicoVideo;Vegetation;ZhengLingFeng;End
    	
    	//PicoVideo;Vegetation;ZhengLingFeng;Begin
    	[ToggleOff] _ForceDisableSnowSurface("Force Disable Snow Surface", float) = 0
    	[ToggleOff] _ForceDisableRainSurface("Force Disable Rain Surface", float) = 0
    	//PicoVideo;Vegetation;ZhengLingFeng;End
        
        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMapScale("Scale", Range(0.0, 2.0)) = 1.0
        _DetailAlbedoMap("Detail Albedo x2", 2D) = "linearGrey" {}
        _DetailNormalMapScale("Scale", Range(0.0, 2.0)) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        // SRP batching compatibility for Clear Coat (Not used in Lit)
        [HideInInspector] _ClearCoatMask("_ClearCoatMask", Float) = 0.0
        [HideInInspector] _ClearCoatSmoothness("_ClearCoatSmoothness", Float) = 0.0

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        _ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        [HideInInspector] _CustomQueue("Custom queue", Float) = 3000.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections("EnvironmentReflections", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"}

        HLSLINCLUDE
            //PicoVideo;LightMode;YangFan;Begin
            #pragma shader_feature_local _ _ALPHAMAP_ON
            #define _MATRIX_INDEPENDENT_OFFSET_SCALE
            #define _MATRIX_ALPHA_MAP
            #define _MATRIX_CUSTOM_FRESNEL
            //PicoVideo;LightMode;YangFan;End
            
            //PicoVideo;LightMode;Ernst;Begin
            #pragma multi_compile _ _MATRIX_SPC_LIGHTINGMAP
            #define _MATRIX_MIX_NORMAL_MR
            //PicoVideo;LightMode;Ernst;End

        	//PicoVideo;LightMode;YangFan;Begin
            #pragma multi_compile_local _ USE_BLINN_PHONG
            //PicoVideo;LightMode;YangFan;End
        ENDHLSL
        
        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ /*_ADDITIONAL_LIGHTS_VERTEX*/ _ADDITIONAL_LIGHTS
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            // #if LIGHTMAP_ON
            //     #define DIRLIGHTMAP_COMBINED
            // #endif

            //PicoVideo;WeatherSimulate;ZhengLingFeng;Begin
            #pragma multi_compile _ _GLOBAL_RAIN_SURFACE _GLOBAL_SNOW_SURFACE
            #pragma shader_feature_local _ _FORCE_DISABLE_SNOW_SURFACE
            #pragma shader_feature_local _ _FORCE_DISABLE_RAIN_SURFACE
            //PicoVideo;WeatherSimulate;ZhengLingFeng;End
            
            //PicoVideo;FogSimulate;ZhengLingFeng;Begin
            #pragma multi_compile_fog
            #pragma multi_compile _ CUSTOM_FOG
            #ifdef CUSTOM_FOG
			#include "Packages/org.byted.vrtv.engine.ecosystem-simulate/Runtime/FogSimulate/Resource/Shaders/Fog.hlsl"
            #endif
            //PicoVideo;FogSimulate;ZhengLingFeng;End

            //PicoVideo;Vegetation;ZhengLingFeng;Begin
			#pragma shader_feature_local _ _VEGETATION_LEAVES _VEGETATION_TRUNK
			#pragma shader_feature_local _ _VEGETATION_HIDE_SIDE_VERTEX _VEGETATION_HIDE_SIDE_FRAGMENT
			//PicoVideo;Vegetation;ZhengLingFeng;End

            //PicoVideo;LayerBlend;XiaoPengCheng;Begin
            #pragma shader_feature_local_fragment _ENABLE_LAYER_BLEND
            #pragma shader_feature_local_fragment _LAYER_ALPHAMAP_ON
            #pragma shader_feature_local _LAYER_NORMALMAP
            //PicoVideo;LayerBlend;XiaoPengCheng;End
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //PicoVideo;Vegetation;ZhengLingFeng;Begin
			#pragma shader_feature_local _ _VEGETATION_LEAVES _VEGETATION_TRUNK
			//PicoVideo;Vegetation;ZhengLingFeng;End

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //PicoVideo;Vegetation;ZhengLingFeng;Begin
			#pragma shader_feature_local _ _VEGETATION_LEAVES _VEGETATION_TRUNK
			//PicoVideo;Vegetation;ZhengLingFeng;End
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        
        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMeta

            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.SLitShader"
}
