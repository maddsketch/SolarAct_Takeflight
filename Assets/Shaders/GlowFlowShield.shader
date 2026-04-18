Shader "OtherPlanets/GlowFlowShield"
{
    Properties
    {
        [Header(Base)]
        [HDR] _Tint("Tint", Color) = (0.3, 0.85, 1.2, 1)
        _GlowIntensity("Glow Intensity", Range(0, 8)) = 2.2
        _Alpha("Alpha Scale", Range(0, 2)) = 0.85
        _InnerAlpha("Inner Shell Alpha", Range(0, 0.5)) = 0.08

        [Header(Rim)]
        _RimPower("Rim Power", Range(0.25, 8)) = 2.4
        _RimBoost("Rim Boost", Range(0, 4)) = 1.35

        [Header(Flow)]
        _FlowScale("Flow Scale (world)", Range(0.05, 4)) = 0.65
        _FlowSpeed("Flow Speed", Range(0, 6)) = 1.15
        _FlowDirection("Flow Direction (XY)", Vector) = (1, 0.35, 0, 0)
        _BandFrequency("Band Frequency", Range(0.5, 24)) = 7.5
        _ScrollSpeed("Scroll Speed", Range(0, 12)) = 3.5
        _NoiseAmount("Noise Warp", Range(0, 2)) = 0.65
        _FlowSharpness("Flow Sharpness", Range(0.5, 8)) = 2.2

        [Header(Pattern)]
        [NoScaleOffset] _PatternTex("Pattern (optional)", 2D) = "white" {}
        _PatternStrength("Pattern Strength", Range(0, 2)) = 0.35

        [Header(Pulse)]
        _PulseSpeed("Pulse Speed", Range(0, 8)) = 2.0
        _PulseAmount("Pulse Amount", Range(0, 0.5)) = 0.12
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
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            TEXTURE2D(_PatternTex);
            SAMPLER(sampler_PatternTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float4 _FlowDirection;
                half _GlowIntensity;
                half _Alpha;
                half _InnerAlpha;
                half _RimPower;
                half _RimBoost;
                half _FlowScale;
                half _FlowSpeed;
                half _BandFrequency;
                half _ScrollSpeed;
                half _NoiseAmount;
                half _FlowSharpness;
                half _PatternStrength;
                half _PulseSpeed;
                half _PulseAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float ShieldHash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float ShieldNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = ShieldHash(i);
                float b = ShieldHash(i + float2(1, 0));
                float c = ShieldHash(i + float2(0, 1));
                float d = ShieldHash(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                const VertexPositionInputs posIn = GetVertexPositionInputs(input.positionOS.xyz);
                const VertexNormalInputs normIn = GetVertexNormalInputs(input.normalOS);
                output.positionCS = posIn.positionCS;
                output.positionWS = posIn.positionWS;
                output.normalWS = normIn.normalWS;
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(posIn.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                const float3 N = normalize(input.normalWS);
                const float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
                const float ndotv = saturate(dot(N, V));
                float rim = pow(1.0 - ndotv, max((float)_RimPower, 0.01));
                rim = saturate(rim * (float)_RimBoost);

                const float t = _Time.y;
                const float2 dir = normalize(_FlowDirection.xy + float2(0.001, 0.001));
                const float2 flowBase = input.positionWS.xz * (float)_FlowScale + dir * t * (float)_FlowSpeed;
                const float2 flowWarp = float2(
                    ShieldNoise(flowBase * 1.7 + t * 0.15),
                    ShieldNoise(flowBase * 1.3 - t * 0.12));
                const float2 flowUV = flowBase + (flowWarp - 0.5) * 2.0 * (float)_NoiseAmount;

                float bands = sin((flowUV.x + flowUV.y) * (float)_BandFrequency + t * (float)_ScrollSpeed);
                bands = bands * 0.5 + 0.5;
                bands = pow(saturate(bands), max((float)_FlowSharpness, 0.01));

                half pattern = SAMPLE_TEXTURE2D(_PatternTex, sampler_PatternTex, input.uv * 2.0 + flowUV * 0.08).r;
                const float energy = rim * saturate(0.2 + 0.8 * bands);
                const float combined = saturate(energy * lerp(1.0, pattern, saturate((float)_PatternStrength)));

                const float pulse = 1.0 + sin(t * (float)_PulseSpeed) * (float)_PulseAmount;
                const float innerShell = (float)_InnerAlpha * (0.25 + ndotv * 0.75);
                const float a = saturate(((float)_Alpha * combined + innerShell) * pulse);

                half3 color = (half3)_Tint.rgb * (half)_GlowIntensity * (half)combined * (half)pulse;
                color = MixFog(color, input.fogCoord);
                return half4(color, a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
