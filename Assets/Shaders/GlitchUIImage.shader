Shader "OtherPlanets/GlitchUIImage"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)

        _GlitchIntensity("Glitch Intensity", Range(0, 2)) = 0
        _GlitchSpeed("Glitch Speed", Range(0, 3)) = 1
        _RgbSplit("RGB Split", Range(0, 2)) = 1
        _BlockScale("Block Scale", Range(4, 256)) = 48
        _ScanlineStrength("Scanline Strength", Range(0, 1)) = 0.12
        _ScanlineFrequency("Scanline Frequency", Range(50, 1200)) = 500

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [HideInInspector] unity_GUIZTestMode("unity_GUIZTestMode", Float) = 8

        [HideInInspector] _TextureSampleAdd("Texture Sample Add", Vector) = (0, 0, 0, 0)
        [HideInInspector] _ClipRect("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            half _GlitchIntensity;
            half _GlitchSpeed;
            half _RgbSplit;
            half _BlockScale;
            half _ScanlineStrength;
            half _ScanlineFrequency;

            float GlitchHash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                const float2 uv = IN.texcoord;
                const float t = _Time.y * (float)_GlitchSpeed;
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

                half4 sampleR = tex2D(_MainTex, uvG + float2(split, 0.0)) + _TextureSampleAdd;
                half4 sampleG = tex2D(_MainTex, uvG) + _TextureSampleAdd;
                half4 sampleB = tex2D(_MainTex, uvG - float2(split, 0.0)) + _TextureSampleAdd;

                half3 rgb = half3(sampleR.r, sampleG.g, sampleB.b);
                half a = sampleG.a * IN.color.a;
                rgb *= IN.color.rgb;

                const float scan = sin(uvG.y * (float)_ScanlineFrequency * 6.2831853) * 0.5 + 0.5;
                rgb *= half(1.0 - scan * (float)_ScanlineStrength);

                half4 color = half4(rgb, a);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
