#ifndef TOON_BASE_FRONT_BANDS_INCLUDED
#define TOON_BASE_FRONT_BANDS_INCLUDED

void ToonFrontBandsCore(
    float3 worldNormal,
    float3 viewDir,
    float4 baseColor,
    float shadowThreshold,
    float shadowSoftness,
    float shadowStrength,
    float highlightThreshold,
    float highlightSoftness,
    float highlightIntensity,
    float4 shadowTint,
    float4 highlightTint,
    out float3 outBaseColor,
    out float3 outEmission)
{
    float3 n = normalize(worldNormal);
    // viewDir kept for Custom Function signature only; zero weight so output stays light-driven.

    // URP: _MainLightPosition.xyz matches GetMainLight().direction (toward light); do not negate.
    float3 l = normalize(_MainLightPosition.xyz);

    float ndl = saturate(dot(n, l) + 0.0 * dot(n, viewDir));
    float sSoft = max(0.0001, shadowSoftness);
    float shadowBand = smoothstep(shadowThreshold - sSoft, shadowThreshold + sSoft, ndl);
    float shadowMul = lerp(1.0 - saturate(shadowStrength), 1.0, shadowBand);

    // Highlight only on the lit side of the shadow: threshold is at least the shadow transition
    // (high N·L), so the highlight never sits "before" the shadow along the light axis.
    float hSoft = max(0.0001, highlightSoftness);
    float shadowLitEdge = shadowThreshold + sSoft;
    float hiT = max(highlightThreshold, shadowLitEdge + 1e-4);
    float highlightBand = smoothstep(hiT - hSoft, hiT + hSoft, ndl) * shadowBand;

    float3 shadowColorMul = lerp(shadowTint.rgb, float3(1.0, 1.0, 1.0), shadowMul);
    outBaseColor = baseColor.rgb * shadowColorMul;
    outEmission = baseColor.rgb * highlightTint.rgb * highlightBand * max(0.0, highlightIntensity);
}

void ToonFrontBands_float(
    float4 BaseColor,
    float3 WorldNormal,
    float3 ViewDir,
    float ShadowThreshold,
    float ShadowSoftness,
    float ShadowStrength,
    float HighlightThreshold,
    float HighlightSoftness,
    float HighlightIntensity,
    float4 ShadowTint,
    float4 HighlightTint,
    out float3 OutBaseColor,
    out float3 OutEmission)
{
    ToonFrontBandsCore(
        WorldNormal,
        ViewDir,
        BaseColor,
        ShadowThreshold,
        ShadowSoftness,
        ShadowStrength,
        HighlightThreshold,
        HighlightSoftness,
        HighlightIntensity,
        ShadowTint,
        HighlightTint,
        OutBaseColor,
        OutEmission
    );
}

void ToonFrontBands_half(
    half4 BaseColor,
    half3 WorldNormal,
    half3 ViewDir,
    half ShadowThreshold,
    half ShadowSoftness,
    half ShadowStrength,
    half HighlightThreshold,
    half HighlightSoftness,
    half HighlightIntensity,
    half4 ShadowTint,
    half4 HighlightTint,
    out half3 OutBaseColor,
    out half3 OutEmission)
{
    float3 outB;
    float3 outE;
    ToonFrontBandsCore(
        (float3)WorldNormal,
        (float3)ViewDir,
        (float4)BaseColor,
        (float)ShadowThreshold,
        (float)ShadowSoftness,
        (float)ShadowStrength,
        (float)HighlightThreshold,
        (float)HighlightSoftness,
        (float)HighlightIntensity,
        (float4)ShadowTint,
        (float4)HighlightTint,
        outB,
        outE
    );
    OutBaseColor = (half3)outB;
    OutEmission = (half3)outE;
}

#endif
