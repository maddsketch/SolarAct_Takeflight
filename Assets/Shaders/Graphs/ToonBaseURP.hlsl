#ifndef TOON_BASE_URP_INCLUDED
#define TOON_BASE_URP_INCLUDED

void ToonBaseCore(
    float3 WorldNormal,
    float3 ViewDir,
    float3 LightDir,
    float4 BaseColor,
    float4 ShadowColor,
    float4 RimColor,
    float ShadowThreshold,
    float ShadowSteps,
    float RimPower,
    out float3 OutColor)
{
    float3 n = normalize(WorldNormal);
    float3 l = normalize(LightDir);
    float ndl = saturate(dot(n, l));

    float steps = max(1.0, ShadowSteps);
    float stepped = floor(ndl * steps) / steps;
    float lit = step(ShadowThreshold, stepped);
    float3 toon = lerp(ShadowColor.rgb, BaseColor.rgb, lit);

    float3 v = normalize(ViewDir);
    float rim = pow(saturate(1.0 - dot(n, v)), max(0.01, RimPower));
    toon = lerp(toon, RimColor.rgb, rim * RimColor.a);

    OutColor = toon;
}

void ToonBaseURP_float(
    float3 WorldNormal,
    float3 ViewDir,
    float3 LightDir,
    float4 BaseColor,
    float4 ShadowColor,
    float4 RimColor,
    float ShadowThreshold,
    float ShadowSteps,
    float RimPower,
    out float3 OutColor,
    out float OutAlpha)
{
    ToonBaseCore(
        WorldNormal,
        ViewDir,
        LightDir,
        BaseColor,
        ShadowColor,
        RimColor,
        ShadowThreshold,
        ShadowSteps,
        RimPower,
        OutColor
    );
    OutAlpha = 1.0;
}

void ToonBaseURP_half(
    half3 WorldNormal,
    half3 ViewDir,
    half3 LightDir,
    half4 BaseColor,
    half4 ShadowColor,
    half4 RimColor,
    half ShadowThreshold,
    half ShadowSteps,
    half RimPower,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 outColor;
    ToonBaseCore(
        (float3)WorldNormal,
        (float3)ViewDir,
        (float3)LightDir,
        (float4)BaseColor,
        (float4)ShadowColor,
        (float4)RimColor,
        (float)ShadowThreshold,
        (float)ShadowSteps,
        (float)RimPower,
        outColor
    );
    OutColor = (half3)outColor;
    OutAlpha = 1.0h;
}

#endif
