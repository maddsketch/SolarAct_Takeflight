// URP: screen-space refraction with a circular (UV) bubble / lens falloff — no noise.
// Best on a quad or sphere with 0–1 UVs; tune _BubbleRadius / _BubbleCenter on the material.
// Requires: Opaque Texture on the URP camera/renderer.

Shader "TakeFlight/URP/ScreenSpaceRefractionBubble"
{
    Properties
    {
        [Header(Bubble lens)]
        _BubbleCenter ("Bubble center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _BubbleRadius ("Bubble radius (UV space)", Range(0.05, 0.95)) = 0.42
        _BubbleStrength ("Refraction strength at center", Range(0, 0.35)) = 0.14
        _EdgeSoftness ("Edge falloff", Range(0.01, 0.5)) = 0.12
        _NormalBlend ("Extra offset from world normal", Range(0, 0.2)) = 0.04

        [Header(Look)]
        _Tint ("Tint", Color) = (1, 1, 1, 0.2)
        _ChromaticAberration ("Chromatic aberration", Range(0, 0.02)) = 0.005

        [Header(Rim)]
        _RimPower ("Rim falloff sharpness", Range(0.25, 16)) = 3.5
        _RimColor ("Rim color", Color) = (0.85, 0.95, 1, 1)
        _RimIntensity ("Rim brightness add", Range(0, 2)) = 0.35
        _RimAlphaFalloff ("Rim alpha falloff (grazing = thinner)", Range(0, 1)) = 0.4

        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ScreenRefractionBubble"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BubbleCenter;
                float _BubbleRadius;
                float _BubbleStrength;
                float _EdgeSoftness;
                float _NormalBlend;
                float4 _Tint;
                float _ChromaticAberration;
                float _RimPower;
                float4 _RimColor;
                float _RimIntensity;
                float _RimAlphaFalloff;
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
                float4 screenPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                const float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                const float2 uvScreen = i.screenPos.xy / i.screenPos.w;

                const float2 c = _BubbleCenter.xy;
                const float2 d = i.uv - c;
                const float dist = length(d);
                const float inner = max(1e-5, _BubbleRadius - _EdgeSoftness);
                const float outer = _BubbleRadius + 1e-5;
                // 1 at center, 0 outside bubble
                const float bubbleMask = 1.0 - smoothstep(inner, outer, dist);
                // Lens profile: strongest in middle, smooth toward edge
                const float lens = bubbleMask * bubbleMask * (1.0 + (1.0 - bubbleMask) * 0.35);

                const float2 radial = dist > 1e-5 ? d / dist : float2(0, 0);
                const float2 bubbleOffset = radial * lens * _BubbleStrength;

                const float3 n = normalize(i.worldNormal);
                const float2 normalOffset = n.xy * _NormalBlend * 0.25 * bubbleMask;

                const float2 distort = bubbleOffset + normalOffset;

                half3 col;
                if (_ChromaticAberration > 0.0001f)
                {
                    const float2 cabDir = normalize(distort + 1e-5f) * _ChromaticAberration;
                    const half r = SampleSceneColor(uvScreen + distort + cabDir).r;
                    const half g = SampleSceneColor(uvScreen + distort).g;
                    const half b = SampleSceneColor(uvScreen + distort - cabDir).b;
                    col = half3(r, g, b);
                }
                else
                {
                    col = SampleSceneColor(uvScreen + distort);
                }

                col *= _Tint.rgb;

                const float3 V = normalize(_WorldSpaceCameraPos.xyz - i.positionWS);
                const float ndotv = saturate(dot(n, V));
                const float rim = pow(1.0 - ndotv, _RimPower);
                col += _RimColor.rgb * rim * _RimIntensity;

                half a = _Tint.a * bubbleMask;
                a *= saturate(1.0 - _RimAlphaFalloff * rim);
                return half4(col, a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
