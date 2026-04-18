#ifndef FLAMING_CIRCLE_UNLIT_INCLUDED
#define FLAMING_CIRCLE_UNLIT_INCLUDED

float FlameHash21(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

float FlameNoise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = FlameHash21(i + float2(0.0, 0.0));
    float b = FlameHash21(i + float2(1.0, 0.0));
    float c = FlameHash21(i + float2(0.0, 1.0));
    float d = FlameHash21(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float FlameFbm(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    float2 q = p;
    v += a * FlameNoise2D(q);
    q = q * 2.03 + float2(13.7, 7.9);
    a *= 0.5;
    v += a * FlameNoise2D(q);
    q = q * 2.01 + float2(3.1, 19.1);
    a *= 0.5;
    v += a * FlameNoise2D(q);
    return v / 0.875;
}

void FlamingCircleUnlitCore(
    float2 uv,
    float4 InnerColor,
    float4 OuterColor,
    float Radius,
    float Thickness,
    float EdgeSoftness,
    float NoiseScale,
    float FlameSpeed,
    float FlameAmount,
    float FlickerSpeed,
    float EmissionStrength,
    out float3 OutColor,
    out float OutAlpha)
{
    float2 centered = uv - 0.5;
    float dist = length(centered);

    float t = _Time.y;
    float2 nUvA = centered * max(NoiseScale, 0.001) + float2(0.0, t * FlameSpeed);
    float2 nUvB = centered * max(NoiseScale * 1.93, 0.001) + float2(t * FlameSpeed * 0.57, -t * FlameSpeed * 0.73);
    float nA = FlameFbm(nUvA);
    float nB = FlameFbm(nUvB);
    float n = (nA * 0.7 + nB * 0.3) * 2.0 - 1.0;

    float pulse = sin(t * (FlameSpeed * 1.35) + nA * 6.2831853) * 0.5 + 0.5;
    float radiusWarp = n * FlameAmount + (pulse - 0.5) * (FlameAmount * 0.35);

    float rOuter = max(Radius + radiusWarp, 0.001);
    float rInner = max(rOuter - max(Thickness, 0.001), 0.0005);
    float soft = max(EdgeSoftness, 0.0001);

    float outerMask = 1.0 - smoothstep(rOuter, rOuter + soft, dist);
    float innerMask = 1.0 - smoothstep(rInner, rInner + soft, dist);
    float ring = saturate(outerMask - innerMask);

    float edgeGlow = saturate((dist - rInner) / max(rOuter - rInner, 0.0001));
    float heat = saturate(edgeGlow + nA * 0.35);
    float3 flame = lerp(OuterColor.rgb, InnerColor.rgb, heat);

    float flicker = 0.85 + (sin(t * FlickerSpeed + nB * 12.0) * 0.5 + 0.5) * 0.35;
    float alpha = ring * saturate(0.55 + nA * 0.65);
    float3 emissive = flame * alpha * flicker * max(EmissionStrength, 0.0);

    OutColor = emissive;
    OutAlpha = alpha;
}

void FlamingCircleUnlit_float(
    float4 UV,
    float4 InnerColor,
    float4 OuterColor,
    float Radius,
    float Thickness,
    float EdgeSoftness,
    float NoiseScale,
    float FlameSpeed,
    float FlameAmount,
    float FlickerSpeed,
    float EmissionStrength,
    out float3 OutColor,
    out float OutAlpha)
{
    FlamingCircleUnlitCore(UV.xy, InnerColor, OuterColor, Radius, Thickness, EdgeSoftness, NoiseScale, FlameSpeed, FlameAmount, FlickerSpeed, EmissionStrength, OutColor, OutAlpha);
}

void FlamingCircleUnlit_half(
    float4 UV,
    float4 InnerColor,
    float4 OuterColor,
    half Radius,
    half Thickness,
    half EdgeSoftness,
    half NoiseScale,
    half FlameSpeed,
    half FlameAmount,
    half FlickerSpeed,
    half EmissionStrength,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 c;
    float a;
    FlamingCircleUnlitCore((float2)UV.xy, (float4)InnerColor, (float4)OuterColor, (float)Radius, (float)Thickness, (float)EdgeSoftness, (float)NoiseScale, (float)FlameSpeed, (float)FlameAmount, (float)FlickerSpeed, (float)EmissionStrength, c, a);
    OutColor = (half3)c;
    OutAlpha = (half)a;
}

#endif
