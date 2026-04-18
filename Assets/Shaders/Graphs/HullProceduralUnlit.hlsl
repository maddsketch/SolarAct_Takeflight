#ifndef HULL_PROCEDURAL_UNLIT_INCLUDED
#define HULL_PROCEDURAL_UNLIT_INCLUDED

float HullHash21(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

float HullGridLines(float2 u, float lineWidthFrac)
{
    float2 f = frac(u);
    float d = min(min(f.x, 1.0 - f.x), min(f.y, 1.0 - f.y));
    return 1.0 - smoothstep(0.0, lineWidthFrac, d);
}

// Height used for derivative bump (lines read as raised ridges).
float HullLineHeight(
    float2 uv,
    float Tiling,
    float LineWidth,
    float SubGrid,
    float SubLineStrength,
    float DiagonalBlend)
{
    float tile = max(Tiling, 1e-3);
    float lw = saturate(LineWidth) * 0.48 + 1e-4;
    float sub = max(SubGrid, 1.0);

    float2 u = uv * tile;
    float mainL = HullGridLines(u, lw);
    float subL = HullGridLines(u * sub, lw * 0.52) * saturate(SubLineStrength);
    float2 ruv = float2(uv.x + uv.y, uv.y - uv.x) * 0.70710678;
    float diagL = HullGridLines(ruv * tile * 0.72, lw * 1.05) * saturate(DiagonalBlend);

    return saturate(mainL * 0.55 + subL * 0.35 + diagL * 0.35);
}

void HullProceduralUnlitCore(
    float2 uv,
    float4 BaseColor,
    float4 LineColor,
    float4 AccentColor,
    float Tiling,
    float LineWidth,
    float SubGrid,
    float SubLineStrength,
    float DiagonalBlend,
    float PanelVariation,
    out float3 OutColor)
{
    float tile = max(Tiling, 1e-3);
    float lw = saturate(LineWidth) * 0.48 + 1e-4;
    float sub = max(SubGrid, 1.0);

    float2 u = uv * tile;
    float2 cell = floor(u);
    float vary = HullHash21(cell + float2(3.1, 9.7));
    float shade = lerp(1.0, 0.72 + 0.36 * vary, saturate(PanelVariation));

    float mainL = HullGridLines(u, lw);
    float subL = HullGridLines(u * sub, lw * 0.52) * saturate(SubLineStrength);
    float2 ruv = float2(uv.x + uv.y, uv.y - uv.x) * 0.70710678;
    float diagL = HullGridLines(ruv * tile * 0.72, lw * 1.05) * saturate(DiagonalBlend);

    float3 base = BaseColor.rgb * shade;
    float3 col = base;
    col = lerp(col, LineColor.rgb, saturate(mainL));
    col = lerp(col, LineColor.rgb * 0.5, saturate(subL) * (1.0 - mainL * 0.95));
    col = lerp(col, AccentColor.rgb, saturate(diagL) * AccentColor.a);

    OutColor = saturate(col);
}

void HullProceduralUnlit_float(
    float4 UV,
    float4 BaseColor,
    float4 LineColor,
    float4 AccentColor,
    float Tiling,
    float LineWidth,
    float SubGrid,
    float SubLineStrength,
    float DiagonalBlend,
    float PanelVariation,
    out float3 OutColor,
    out float OutAlpha)
{
    HullProceduralUnlitCore(UV.xy, BaseColor, LineColor, AccentColor, Tiling, LineWidth,
        SubGrid, SubLineStrength, DiagonalBlend, PanelVariation, OutColor);
    OutAlpha = 1.0;
}

void HullProceduralUnlit_half(
    float4 UV,
    float4 BaseColor,
    float4 LineColor,
    float4 AccentColor,
    half Tiling,
    half LineWidth,
    half SubGrid,
    half SubLineStrength,
    half DiagonalBlend,
    half PanelVariation,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 o;
    HullProceduralUnlitCore((float2)UV.xy, (float4)BaseColor, (float4)LineColor, (float4)AccentColor,
        (float)Tiling, (float)LineWidth, (float)SubGrid, (float)SubLineStrength,
        (float)DiagonalBlend, (float)PanelVariation, o);
    OutColor = (half3)o;
    OutAlpha = 1.0;
}

// Tangent-space normal from procedural height (ddx/ddy of line mask).
// Blend a sampled normal map in Shader Graph (Normal Blend node) with this output.
void HullProceduralLitCore(
    float2 uv,
    float4 BaseColor,
    float4 LineColor,
    float4 AccentColor,
    float Tiling,
    float LineWidth,
    float SubGrid,
    float SubLineStrength,
    float DiagonalBlend,
    float PanelVariation,
    float BumpStrength,
    out float3 OutColor,
    out float3 OutNormalTS)
{
    HullProceduralUnlitCore(uv, BaseColor, LineColor, AccentColor, Tiling, LineWidth,
        SubGrid, SubLineStrength, DiagonalBlend, PanelVariation, OutColor);

    float h = HullLineHeight(uv, Tiling, LineWidth, SubGrid, SubLineStrength, DiagonalBlend);
    float amp = max(BumpStrength, 0.0) * 0.25;
    float hx = h * amp;
    float2 dh = float2(ddx(hx), ddy(hx));
    OutNormalTS = normalize(float3(-dh.x, -dh.y, 1.0));
}

void HullProceduralLit_float(
    float4 UV,
    float4 BaseColor,
    float4 LineColor,
    float4 AccentColor,
    float Tiling,
    float LineWidth,
    float SubGrid,
    float SubLineStrength,
    float DiagonalBlend,
    float PanelVariation,
    float BumpStrength,
    out float3 OutColor,
    out float3 OutNormalTS,
    out float OutAlpha)
{
    HullProceduralLitCore(UV.xy, BaseColor, LineColor, AccentColor, Tiling, LineWidth,
        SubGrid, SubLineStrength, DiagonalBlend, PanelVariation, BumpStrength, OutColor, OutNormalTS);
    OutAlpha = 1.0;
}

void HullProceduralLit_half(
    float4 UV,
    float4 BaseColor,
    float4 LineColor,
    float4 AccentColor,
    half Tiling,
    half LineWidth,
    half SubGrid,
    half SubLineStrength,
    half DiagonalBlend,
    half PanelVariation,
    half BumpStrength,
    out half3 OutColor,
    out half3 OutNormalTS,
    out half OutAlpha)
{
    float3 c, n;
    HullProceduralLitCore((float2)UV.xy, (float4)BaseColor, (float4)LineColor, (float4)AccentColor,
        (float)Tiling, (float)LineWidth, (float)SubGrid, (float)SubLineStrength,
        (float)DiagonalBlend, (float)PanelVariation, (float)BumpStrength, c, n);
    OutColor = (half3)c;
    OutNormalTS = (half3)n;
    OutAlpha = 1.0;
}

#endif
