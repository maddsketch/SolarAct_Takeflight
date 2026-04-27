Shader "Custom/PainterlyLighting"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        [HDR]_SpecularColor("Specular color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [Normal]_Normal("Normal", 2D) = "bump" {}
        _NormalStrength("Normal strength", Range(-2, 2)) = 1

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _ShadingGradient("Shading gradient", 2D) = "white" {}
        _PainterlyGuide("Painterly guide", 2D) = "white" {}
        _PainterlySmoothness("Painterly smoothness", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Normal);
            SAMPLER(sampler_Normal);
            TEXTURE2D(_PainterlyGuide);
            SAMPLER(sampler_PainterlyGuide);
            TEXTURE2D(_ShadingGradient);
            SAMPLER(sampler_ShadingGradient);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _SpecularColor;
                half _Glossiness;
                half _Metallic;
                half _PainterlySmoothness;
                half _NormalStrength;
                float4 _MainTex_ST;
                float4 _Normal_ST;
                float4 _PainterlyGuide_ST;
                float4 _ShadingGradient_ST;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                half fogFactor : TEXCOORD5;
            };

            float3 EvaluatePainterlyLighting(
                float3 albedo,
                float3 normalWS,
                float3 viewDirWS,
                half painterlyGuide,
                half smoothness,
                Light lightData)
            {
                half nDotL = saturate(dot(normalWS, lightData.direction) + 0.2h);
                half diff = smoothstep(painterlyGuide - _PainterlySmoothness, painterlyGuide + _PainterlySmoothness, nDotL);

                float3 refl = reflect(lightData.direction, normalWS);
                float vDotRefl = saturate(dot(viewDirWS, -refl));
                float specularThreshold = painterlyGuide + smoothness;
                float specMask = smoothstep(
                    specularThreshold - _PainterlySmoothness,
                    specularThreshold + _PainterlySmoothness,
                    vDotRefl
                ) * smoothness;

                half atten = smoothstep(
                    painterlyGuide - _PainterlySmoothness,
                    painterlyGuide + _PainterlySmoothness,
                    lightData.distanceAttenuation * lightData.shadowAttenuation
                );

                float3 gradient = SAMPLE_TEXTURE2D(_ShadingGradient, sampler_ShadingGradient, float2(diff, 0.5)).rgb;
                float3 specular = _SpecularColor.rgb * lightData.color * specMask;
                return (albedo * gradient * lightData.color + specular) * atten;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = posInput.positionCS;
                output.positionWS = posInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.shadowCoord = GetShadowCoord(posInput);
                output.fogFactor = ComputeFogFactor(posInput.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 albedoSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, input.uv), _NormalStrength);

                float3 bitangentWS = normalize(cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w);
                float3x3 tbn = float3x3(normalize(input.tangentWS.xyz), bitangentWS, normalize(input.normalWS));
                float3 normalWS = normalize(mul(normalTS, tbn));

                half painterlyGuide = SAMPLE_TEXTURE2D(_PainterlyGuide, sampler_PainterlyGuide, input.uv).r;
                float3 viewDirWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));

                float3 color = 0.0;
                Light mainLight = GetMainLight(input.shadowCoord, input.positionWS, half4(1, 1, 1, 1));
                color += EvaluatePainterlyLighting(albedoSample.rgb, normalWS, viewDirWS, painterlyGuide, _Glossiness, mainLight);

                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < pixelLightCount; ++i)
                    {
                        Light lightData = GetAdditionalLight(i, input.positionWS, half4(1, 1, 1, 1));
                        color += EvaluatePainterlyLighting(albedoSample.rgb, normalWS, viewDirWS, painterlyGuide, _Glossiness, lightData);
                    }
                #endif

                color = MixFog(color, input.fogFactor);
                return half4(color, albedoSample.a);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}