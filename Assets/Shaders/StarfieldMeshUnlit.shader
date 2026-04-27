Shader "TakeFlight/URP/StarfieldMeshUnlit"
{
    Properties
    {
        [MainColor] _BackgroundColor("Background Color", Color) = (0.01, 0.02, 0.08, 1)
        _StarColorA("Star Color A", Color) = (0.8, 0.88, 1, 1)
        _StarColorB("Star Color B", Color) = (1, 0.92, 0.75, 1)

        _StarDensity("Star Density", Range(5, 240)) = 72
        _StarThreshold("Star Threshold", Range(0.75, 0.9995)) = 0.96
        _StarSizeMin("Star Size Min", Range(0.01, 0.35)) = 0.08
        _StarSizeMax("Star Size Max", Range(0.08, 0.7)) = 0.28

        _TwinkleSpeed("Twinkle Speed", Range(0, 8)) = 1.8
        _TwinkleAmount("Twinkle Amount", Range(0, 1)) = 0.35
        _DriftSpeed("Drift Speed XY", Vector) = (0.01, -0.008, 0, 0)
        _Intensity("Intensity", Range(0, 5)) = 1.4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
            "UniversalMaterialType" = "Unlit"
        }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BackgroundColor;
                float4 _StarColorA;
                float4 _StarColorB;
                float _StarDensity;
                float _StarThreshold;
                float _StarSizeMin;
                float _StarSizeMax;
                float _TwinkleSpeed;
                float _TwinkleAmount;
                float4 _DriftSpeed;
                float _Intensity;
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
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash11(float p)
            {
                return frac(sin(p * 127.1) * 43758.5453123);
            }

            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vtx = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vtx.positionCS;
                output.positionWS = vtx.positionWS;
                output.uv = input.uv;
                return output;
            }

            float StarLayer(float2 uv, float density, float threshold, float sizeMin, float sizeMax, float twinkleSpeed, float twinkleAmount)
            {
                float2 gridUv = uv * density;
                float2 cell = floor(gridUv);
                float2 local = frac(gridUv) - 0.5;

                float seed = Hash21(cell);
                float starToggle = step(threshold, seed);

                float starSize = lerp(sizeMin, sizeMax, Hash11(seed * 93.17));
                float dist = length(local) / max(starSize, 0.0001);
                float core = saturate(1.0 - dist * dist);
                core = core * core;

                float phase = Hash11(seed * 151.3) * 6.2831853;
                float twinkle = 1.0 - twinkleAmount + twinkleAmount * (0.5 + 0.5 * sin(_Time.y * twinkleSpeed + phase));

                return starToggle * core * twinkle;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv + _DriftSpeed.xy * _Time.y;

                // Mix UV and world-space terms so stars feel less flat on curved meshes.
                uv += input.positionWS.xz * 0.035;

                float layerNear = StarLayer(uv, _StarDensity, _StarThreshold, _StarSizeMin, _StarSizeMax, _TwinkleSpeed, _TwinkleAmount);
                float layerFar = StarLayer(uv * 1.9 + float2(21.4, -7.6), _StarDensity * 0.55, saturate(_StarThreshold + 0.02), _StarSizeMin * 0.7, _StarSizeMax * 0.8, _TwinkleSpeed * 0.6, _TwinkleAmount * 0.8);
                float stars = saturate(layerNear + layerFar * 0.8) * _Intensity;

                float colorLerp = saturate(layerNear * 0.75 + layerFar * 0.25);
                float3 starCol = lerp(_StarColorA.rgb, _StarColorB.rgb, colorLerp);
                float3 col = _BackgroundColor.rgb + starCol * stars;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
