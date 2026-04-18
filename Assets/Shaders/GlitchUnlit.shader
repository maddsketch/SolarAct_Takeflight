Shader "OtherPlanets/GlitchUnlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        _GlitchIntensity("Glitch Intensity", Range(0, 2)) = 0.5
        _GlitchSpeed("Glitch Speed", Range(0, 3)) = 1
        _RgbSplit("RGB Split", Range(0, 2)) = 1
        _BlockScale("Block Scale", Range(4, 256)) = 48
        _ScanlineStrength("Scanline Strength", Range(0, 1)) = 0.12
        _ScanlineFrequency("Scanline Frequency", Range(50, 1200)) = 500
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                half _GlitchIntensity;
                half _GlitchSpeed;
                half _RgbSplit;
                half _BlockScale;
                half _ScanlineStrength;
                half _ScanlineFrequency;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float GlitchHash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                const float2 uv = input.uv;
                const float t = _Time.y * _GlitchSpeed;
                const float intensity = saturate((float)_GlitchIntensity);

                const float blockScale = max((float)_BlockScale, 4.0);
                const float2 cellId = floor(uv * blockScale);
                const float tBlock = floor(t * 14.0);
                const float2 cellSeed = cellId + float2(tBlock * 17.0, tBlock * 31.0);
                const float cellRand = GlitchHash(cellSeed);
                const float burst = step(0.93, cellRand);
                const float2 jitter = (float2(GlitchHash(cellId + 1.7), GlitchHash(cellId + 2.3)) - 0.5) * 2.0;
                const float2 blockOffset = jitter * (0.012 + burst * 0.045) * intensity;

                const float2 wobble = float2(
                    sin(uv.y * 40.0 + t * 9.0),
                    cos(uv.x * 35.0 + t * 7.0)) * 0.004 * intensity;

                const float2 uvG = uv + blockOffset + wobble;
                const float split = (float)_RgbSplit * 0.018 * intensity;

                const half4 sampleR = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvG + float2(split, 0.0));
                const half4 sampleG = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvG);
                const half4 sampleB = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvG - float2(split, 0.0));

                half3 color = half3(sampleR.r, sampleG.g, sampleB.b);
                const half alpha = sampleG.a * _BaseColor.a;
                color *= _BaseColor.rgb;

                const float scan = sin(uvG.y * (float)_ScanlineFrequency * 6.2831853) * 0.5 + 0.5;
                color *= half(1.0 - scan * (float)_ScanlineStrength);

                color = MixFog(color, input.fogCoord);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
