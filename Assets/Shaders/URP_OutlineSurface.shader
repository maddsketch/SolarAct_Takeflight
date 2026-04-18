// TakeFlight URP — two-pass inverted-hull outline + simple opaque base (single material on the mesh).
//
// How it works:
//   Pass 0 pushes each vertex along its object-space normal, then uses Cull Front so only the
//   "inner" faces of the inflated hull draw — a ring around the silhouette. Pass 1 draws the
//   real surface with Cull Back on top.
//
// Properties:
//   _OutlineColor / _OutlineWidth — ring color and extrusion in object units (scale matters; try ~0.001–0.05).
//   _BaseColor / _MainTex — albedo tint and optional texture (defaults to white if unassigned).
//
// Limitation: meshes with hard/smoothed-split normals can show small gaps in the outline ring;
//   smooth normals or a slightly larger _OutlineWidth usually fixes it.

Shader "TakeFlight/URP/OutlineSurface"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _RenderFace ("Render Face", Float) = 1

        [Header(Outline)]
        _OutlineColor ("Outline color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline width (object space)", Range(0, 0.1)) = 0.02

        [Header(Base)]
        _BaseColor ("Base color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _MainTex ("Main texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_RenderFace]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 n = normalize(v.normalOS);
                float3 posOS = v.positionOS.xyz + n * _OutlineWidth;
                const float3 positionWS = TransformObjectToWorld(posOS);
                o.positionCS = TransformWorldToHClip(positionWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return half4(_OutlineColor.rgb, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Base"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_RenderFace]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _MainTex_ST;
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
                float2 uv : TEXCOORD0;
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
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return tex * half4(_BaseColor.rgb, _BaseColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
