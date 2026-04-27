#ifndef PAINTERLY_LIGHTING_GRAPH_INCLUDED
#define PAINTERLY_LIGHTING_GRAPH_INCLUDED

void PainterlyLightingCore(
    float3 normalWS,
    float3 viewDirWS,
    float3 lightDirWS,
    float3 lightColor,
    float attenuation,
    float3 albedo,
    float3 gradientColor,
    float painterlyGuide,
    float smoothness,
    float3 specularColor,
    float painterlySmoothness,
    out float3 outColor,
    out float outAlpha)
{
    float3 n = normalize(normalWS);
    float3 v = normalize(viewDirWS);
    float3 l = normalize(lightDirWS);

    float nDotL = saturate(dot(n, l) + 0.2);
    float diff = smoothstep(
        painterlyGuide - painterlySmoothness,
        painterlyGuide + painterlySmoothness,
        nDotL
    );

    float3 refl = reflect(l, n);
    float vDotRefl = saturate(dot(v, -refl));
    float specThreshold = painterlyGuide + smoothness;
    float specMask = smoothstep(
        specThreshold - painterlySmoothness,
        specThreshold + painterlySmoothness,
        vDotRefl
    ) * smoothness;

    float atten = smoothstep(
        painterlyGuide - painterlySmoothness,
        painterlyGuide + painterlySmoothness,
        attenuation
    );

    float3 specular = specularColor * lightColor * specMask;
    outColor = (albedo * gradientColor * lightColor + specular) * atten;
    outAlpha = 1.0;
}

void PainterlyLightingGraph_float(
    float3 NormalWS,
    float3 ViewDirWS,
    float3 LightDirWS,
    float3 LightColor,
    float Attenuation,
    float3 Albedo,
    float3 GradientColor,
    float PainterlyGuide,
    float Smoothness,
    float3 SpecularColor,
    float PainterlySmoothness,
    out float3 OutColor,
    out float OutAlpha)
{
    PainterlyLightingCore(
        NormalWS,
        ViewDirWS,
        LightDirWS,
        LightColor,
        Attenuation,
        Albedo,
        GradientColor,
        PainterlyGuide,
        Smoothness,
        SpecularColor,
        PainterlySmoothness,
        OutColor,
        OutAlpha
    );
}

void PainterlyLightingGraph_half(
    half3 NormalWS,
    half3 ViewDirWS,
    half3 LightDirWS,
    half3 LightColor,
    half Attenuation,
    half3 Albedo,
    half3 GradientColor,
    half PainterlyGuide,
    half Smoothness,
    half3 SpecularColor,
    half PainterlySmoothness,
    out half3 OutColor,
    out half OutAlpha)
{
    float3 outColor;
    float outAlpha;
    PainterlyLightingCore(
        (float3)NormalWS,
        (float3)ViewDirWS,
        (float3)LightDirWS,
        (float3)LightColor,
        (float)Attenuation,
        (float3)Albedo,
        (float3)GradientColor,
        (float)PainterlyGuide,
        (float)Smoothness,
        (float3)SpecularColor,
        (float)PainterlySmoothness,
        outColor,
        outAlpha
    );

    OutColor = (half3)outColor;
    OutAlpha = (half)outAlpha;
}

#endif
