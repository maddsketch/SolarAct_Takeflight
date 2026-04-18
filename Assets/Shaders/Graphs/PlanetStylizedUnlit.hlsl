#ifndef PLANET_STYLIZED_UNLIT_INCLUDED
#define PLANET_STYLIZED_UNLIT_INCLUDED

float Hash31(float3 p)
{
    return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453123);
}

float Noise3(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);

    float n000 = Hash31(i);
    float n100 = Hash31(i + float3(1, 0, 0));
    float n010 = Hash31(i + float3(0, 1, 0));
    float n110 = Hash31(i + float3(1, 1, 0));
    float n001 = Hash31(i + float3(0, 0, 1));
    float n101 = Hash31(i + float3(1, 0, 1));
    float n011 = Hash31(i + float3(0, 1, 1));
    float n111 = Hash31(i + float3(1, 1, 1));

    float nx00 = lerp(n000, n100, f.x);
    float nx10 = lerp(n010, n110, f.x);
    float nx01 = lerp(n001, n101, f.x);
    float nx11 = lerp(n011, n111, f.x);

    float nxy0 = lerp(nx00, nx10, f.y);
    float nxy1 = lerp(nx01, nx11, f.y);

    return lerp(nxy0, nxy1, f.z);
}

float Fbm(float3 p)
{
    float v = 0.0;
    float a = 0.5;
    float3 q = p;
    for (int i = 0; i < 5; i++)
    {
        v += a * Noise3(q);
        q *= 2.08;
        a *= 0.5;
    }
    return v;
}

void PlanetStylizedUnlitCore(
    float3 WorldPos,
    float3 ObjectPos,
    float3 WorldNormal,
    float3 ViewDir,
    float UseWorldSpace,
    float NoiseScale,
    float LandThreshold,
    float LandBlend,
    float IceLatitude,
    float RimStrength,
    float3 PlanetCenter,
    float4 ColOceanDeep,
    float4 ColOceanShore,
    float4 ColLand,
    float4 ColPeak,
    float4 ColIce,
    float4 RimColor,
    out float3 OutColor)
{
    float3 dir = (UseWorldSpace > 0.5)
        ? normalize(WorldPos - PlanetCenter)
        : normalize(ObjectPos);

    float n = Fbm(dir * NoiseScale);

    float lb = max(LandBlend, 1e-4);
    float tLand = smoothstep(LandThreshold - lb, LandThreshold + lb, n);

    float shoreMix = smoothstep(0.0, LandThreshold * 0.85, n);
    float3 oceanCol = lerp(ColOceanDeep.rgb, ColOceanShore.rgb, shoreMix);

    float elev = (tLand > 0.001) ? saturate((n - LandThreshold) / max(1e-4, 1.0 - LandThreshold)) : 0.0;
    float3 landCol = lerp(ColLand.rgb, ColPeak.rgb, pow(elev, 1.15));

    float3 base = lerp(oceanCol, landCol, tLand);

    float lat = abs(dir.y);
    float iceMask = smoothstep(IceLatitude, min(1.0, IceLatitude + 0.2), lat);
    base = lerp(base, ColIce.rgb, iceMask * max(tLand, 0.35));

    float3 N = normalize(WorldNormal);
    float3 V = normalize(ViewDir);
    float ndv = saturate(dot(N, V));
    float rim = pow(1.0 - ndv, 2.2);
    base = lerp(base, RimColor.rgb, rim * RimStrength * RimColor.a);

    OutColor = base;
}

void PlanetStylizedUnlit_float(
    float3 WorldPos,
    float3 ObjectPos,
    float3 WorldNormal,
    float3 ViewDir,
    float UseWorldSpace,
    float NoiseScale,
    float LandThreshold,
    float LandBlend,
    float IceLatitude,
    float RimStrength,
    float3 PlanetCenter,
    float4 ColOceanDeep,
    float4 ColOceanShore,
    float4 ColLand,
    float4 ColPeak,
    float4 ColIce,
    float4 RimColor,
    out float3 OutColor,
    out float OutAlpha)
{
    PlanetStylizedUnlitCore(WorldPos, ObjectPos, WorldNormal, ViewDir, UseWorldSpace, NoiseScale,
        LandThreshold, LandBlend, IceLatitude, RimStrength, PlanetCenter,
        ColOceanDeep, ColOceanShore, ColLand, ColPeak, ColIce, RimColor, OutColor);
    OutAlpha = 1.0;
}

void PlanetStylizedUnlit_half(
    half3 WorldPos,
    half3 ObjectPos,
    half3 WorldNormal,
    half3 ViewDir,
    half UseWorldSpace,
    half NoiseScale,
    half LandThreshold,
    half LandBlend,
    half IceLatitude,
    half RimStrength,
    half3 PlanetCenter,
    half4 ColOceanDeep,
    half4 ColOceanShore,
    half4 ColLand,
    half4 ColPeak,
    half4 ColIce,
    half4 RimColor,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 o;
    PlanetStylizedUnlitCore((float3)WorldPos, (float3)ObjectPos, (float3)WorldNormal, (float3)ViewDir,
        (float)UseWorldSpace, (float)NoiseScale, (float)LandThreshold, (float)LandBlend, (float)IceLatitude,
        (float)RimStrength, (float3)PlanetCenter,
        (float4)ColOceanDeep, (float4)ColOceanShore, (float4)ColLand, (float4)ColPeak, (float4)ColIce,
        (float4)RimColor, o);
    OutColor = (half3)o;
    OutAlpha = 1.0;
}

#endif
