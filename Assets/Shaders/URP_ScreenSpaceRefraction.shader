// URP: screen-space refraction by sampling _CameraOpaqueTexture with distorted UVs.
// Requires: Universal Renderer > Opaque Texture enabled on the active camera (default On in many templates).

Shader "TakeFlight/URP/ScreenSpaceRefraction"
{
    Properties
    {
        [Header(Refraction)]
        _Distortion ("Distortion (view XY from normal)", Range(0, 0.5)) = 0.08
        _NoiseAmount ("Procedural noise strength", Range(0, 0.1)) = 0.02
        _NoiseScale ("Noise world scale", Range(0.1, 5)) = 1.5
        _NoiseSpeed ("Noise scroll speed", Range(0, 5)) = 1

        [Header(Look)]
        _Tint ("Tint", Color) = (1, 1, 1, 0.15)
        _ChromaticAberration ("Chromatic aberration", Range(0, 0.02)) = 0.004

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
            Name "ScreenRefraction"
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
                float _Distortion;
                float _NoiseAmount;
                float _NoiseScale;
                float _NoiseSpeed;
                float4 _Tint;
                float _ChromaticAberration;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
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
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.screenPos.xy / i.screenPos.w;

                const float3 n = normalize(i.worldNormal);
                float2 baseOffset = n.xy * _Distortion * 0.25f;

                const float t = _Time.y * _NoiseSpeed;
                const float2 wpos = i.positionWS.xz * _NoiseScale;
                const float2 noise = float2(
                    sin(t + wpos.x * 3.17f + wpos.y * 1.73f),
                    cos(t * 0.93f + wpos.y * 2.91f - wpos.x * 1.11f)
                ) * _NoiseAmount;

                const float2 distort = baseOffset + noise;

                half3 col;
                if (_ChromaticAberration > 0.0001f)
                {
                    const float2 dir = normalize(distort + 1e-5f) * _ChromaticAberration;
                    const half r = SampleSceneColor(uv + distort + dir).r;
                    const half g = SampleSceneColor(uv + distort).g;
                    const half b = SampleSceneColor(uv + distort - dir).b;
                    col = half3(r, g, b);
                }
                else
                {
                    col = SampleSceneColor(uv + distort);
                }

                col *= _Tint.rgb;
                return half4(col, _Tint.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
