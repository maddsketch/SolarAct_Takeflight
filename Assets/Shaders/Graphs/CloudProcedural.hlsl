#ifndef CLOUD_PROCEDURAL_INCLUDED
#define CLOUD_PROCEDURAL_INCLUDED

float Hash2(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

float ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = Hash2(i);
    float b = Hash2(i + float2(1, 0));
    float c = Hash2(i + float2(0, 1));
    float d = Hash2(i + float2(1, 1));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float VoronoiEdge(float2 uv)
{
    float2 g = floor(uv);
    float2 f = frac(uv);
    float md = 8.0;
    float2 mk = f;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 b = float2(x, y);
            float2 r = float2(Hash2(g + b), Hash2(g + b + 19.19)) - 0.5;
            float2 o = b + r - f;
            float d = dot(o, o);
            if (d < md)
            {
                md = d;
                mk = o;
            }
        }
    }

    md = 8.0;
    for (int y2 = -1; y2 <= 1; y2++)
    {
        for (int x2 = -1; x2 <= 1; x2++)
        {
            float2 b = float2(x2, y2);
            float2 r = float2(Hash2(g + b), Hash2(g + b + 19.19)) - 0.5;
            float2 o = b + r - f;

            float2 delta = mk - o;
            if (dot(delta, delta) > 1e-5)
                md = min(md, dot(0.5 * (mk + o), normalize(delta)));
        }
    }

    return md;
}

void CloudApply(
    float3 WorldPos,
    float3 ObjectPos,
    float Time,
    float CloudScale,
    float Opacity,
    float Thickness,
    float ChunkScale,
    float ChunkWeight,
    float3 WindSpeed,
    float4 BaseColor,
    float4 ShadowTint,
    float Animate,
    float UseChunks,
    float UseObjectSpace,
    out float3 OutColor,
    out float OutAlpha)
{
    float3 p = lerp(WorldPos.xyz, ObjectPos.xyz, UseObjectSpace);
    float t = (Animate > 0.5) ? Time : 0.0;
    p += WindSpeed * t;
    p /= max(CloudScale, 1e-4);

    float2 q = p.xz + p.y * 0.37;

    float n = 0;
    float a = 0.5;
    float2 qq = q;
    for (int o = 0; o < 4; o++)
    {
        n += a * ValueNoise(qq);
        qq *= 2.17;
        a *= 0.5;
    }

    float cell = ChunkScale;
    float vor = VoronoiEdge(q * cell);
    float chunkMask = saturate(1.0 - vor * 2.5);
    float cw = ChunkWeight * (UseChunks > 0.5);
    float blended = lerp(n, n * lerp(0.35, 1.15, chunkMask), cw);

    float halfBand = lerp(0.02, 0.45, Thickness);
    float lo = 0.5 - halfBand;
    float hi = 0.5 + halfBand;
    float density = smoothstep(lo, hi, blended);
    density = saturate(density);

    OutAlpha = density * Opacity;
    OutColor = lerp(ShadowTint.rgb, BaseColor.rgb, density);
}

void CloudProcedural_float(
    float3 WorldPos,
    float3 ObjectPos,
    float Time,
    float CloudScale,
    float Opacity,
    float Thickness,
    float ChunkScale,
    float ChunkWeight,
    float3 WindSpeed,
    float4 BaseColor,
    float4 ShadowTint,
    float Animate,
    float UseChunks,
    float UseObjectSpace,
    out float3 OutColor,
    out float OutAlpha)
{
    CloudApply(WorldPos, ObjectPos, Time, CloudScale, Opacity, Thickness, ChunkScale, ChunkWeight,
        WindSpeed, BaseColor, ShadowTint, Animate, UseChunks, UseObjectSpace, OutColor, OutAlpha);
}

void CloudProcedural_half(
    half3 WorldPos,
    half3 ObjectPos,
    half Time,
    half CloudScale,
    half Opacity,
    half Thickness,
    half ChunkScale,
    half ChunkWeight,
    half3 WindSpeed,
    half4 BaseColor,
    half4 ShadowTint,
    half Animate,
    half UseChunks,
    half UseObjectSpace,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 oc;
    float oa;
    CloudApply((float3)WorldPos, (float3)ObjectPos, (float)Time,
        (float)CloudScale, (float)Opacity, (float)Thickness, (float)ChunkScale, (float)ChunkWeight,
        (float3)WindSpeed, (float4)BaseColor, (float4)ShadowTint,
        (float)Animate, (float)UseChunks, (float)UseObjectSpace,
        oc, oa);
    OutColor = (half3)oc;
    OutAlpha = (half)oa;
}

#endif
