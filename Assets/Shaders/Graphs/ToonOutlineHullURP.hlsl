#ifndef TOON_OUTLINE_HULL_URP_INCLUDED
#define TOON_OUTLINE_HULL_URP_INCLUDED

void ToonOutlineHullURPCore(
    float3 ObjectPos,
    float3 ObjectNormal,
    float OutlineWidth,
    float4 OutlineColor,
    out float3 OutPosition,
    out float3 OutColor,
    out float OutAlpha)
{
    float3 n = normalize(ObjectNormal);
    OutPosition = ObjectPos + n * OutlineWidth;
    OutColor = OutlineColor.rgb;
    OutAlpha = OutlineColor.a;
}

void ToonOutlineHullURP_float(
    float3 ObjectPos,
    float3 ObjectNormal,
    float OutlineWidth,
    float4 OutlineColor,
    out float3 OutPosition,
    out float3 OutColor,
    out float OutAlpha)
{
    ToonOutlineHullURPCore(
        ObjectPos,
        ObjectNormal,
        OutlineWidth,
        OutlineColor,
        OutPosition,
        OutColor,
        OutAlpha
    );
}

void ToonOutlineHullURP_half(
    half3 ObjectPos,
    half3 ObjectNormal,
    half OutlineWidth,
    half4 OutlineColor,
    out half3 OutPosition,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 outPos;
    float3 outCol;
    float outA;
    ToonOutlineHullURPCore(
        (float3)ObjectPos,
        (float3)ObjectNormal,
        (float)OutlineWidth,
        (float4)OutlineColor,
        outPos,
        outCol,
        outA
    );
    OutPosition = (half3)outPos;
    OutColor = (half3)outCol;
    OutAlpha = (half)outA;
}

#endif
