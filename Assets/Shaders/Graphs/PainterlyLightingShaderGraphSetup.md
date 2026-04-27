# Painterly Lighting Shader Graph Setup (URP Lit)

Use this file to rebuild `Custom/PainterlyLighting` as a Shader Graph while preserving its painterly ramp and specular threshold behavior.

## 1) Create graph

- Create a **URP Lit Shader Graph** named `PainterlyLightingURP`.
- Surface: **Opaque**
- Workflow: **Metallic**

## 2) Add properties

- `Color` (`_Color`) Color, default `(1,1,1,1)`
- `Specular Color` (`_SpecularColor`) HDR Color, default `(1,1,1,1)`
- `MainTex` (`_MainTex`) Texture2D (default white)
- `Normal` (`_Normal`) Texture2D (Normal, default bump)
- `Normal Strength` (`_NormalStrength`) Float Range `[-2,2]`, default `1`
- `Smoothness` (`_Glossiness`) Float Range `[0,1]`, default `0.5`
- `Metallic` (`_Metallic`) Float Range `[0,1]`, default `0`
- `Shading Gradient` (`_ShadingGradient`) Texture2D (default white)
- `Painterly Guide` (`_PainterlyGuide`) Texture2D (default white)
- `Painterly Smoothness` (`_PainterlySmoothness`) Float Range `[0,1]`, default `0.1`

## 3) Base texture + normal

- Sample `MainTex` at mesh UV0, multiply by `Color` -> `Albedo`.
- Sample `Normal` at UV0, run through `Normal Strength` node.
- Feed normal result into Lit `Normal` block.
- Feed `Metallic` property into Lit `Metallic` block.
- Keep Lit `Smoothness` at `0` (the painterly spec comes from the custom function).

## 4) Lighting inputs

- Use your existing URP main-light direction/color/attenuation subgraph or custom node setup used elsewhere in the project.
- Feed:
  - `NormalWS` (world normal after normal map)
  - `ViewDirWS` (world view direction)
  - `LightDirWS`
  - `LightColor`
  - `Attenuation` (distance * shadow attenuation)

## 5) Painterly custom function

- Add a **Custom Function** node:
  - Type: `File`
  - Source: `PainterlyLightingGraph.hlsl`
  - Function: `PainterlyLightingGraph`

- Inputs:
  - `NormalWS` (Vector3)
  - `ViewDirWS` (Vector3)
  - `LightDirWS` (Vector3)
  - `LightColor` (Vector3)
  - `Attenuation` (Float)
  - `Albedo` (Vector3)
  - `GradientColor` (Vector3)
  - `PainterlyGuide` (Float)
  - `Smoothness` (Float)
  - `SpecularColor` (Vector3)
  - `PainterlySmoothness` (Float)

- Outputs:
  - `OutColor` (Vector3)
  - `OutAlpha` (Float)

## 6) Gradient and guide sampling

- Sample `Painterly Guide` at UV0 and use `.R` -> `PainterlyGuide`.
- For `GradientColor`, sample `Shading Gradient` at UV:
  - X = painterly diff coordinate from your graph or custom-light helper
  - Y = `0.5`

If you do not compute diff externally, you can pass an initial `GradientColor` from a neutral gradient sample and refine after visual tuning.

## 7) Final graph outputs

- Connect `OutColor` to Lit `Emission` (for full custom light shaping).
- Keep `Base Color` connected to `Albedo` (for GI/meta compatibility).
- Connect `OutAlpha` to `Alpha`.

## 8) Match old shader look quickly

- Start with:
  - `Painterly Smoothness = 0.1`
  - `Smoothness = 0.5`
  - `Specular Color = white`
- If highlights are too broad, lower `Smoothness`.
- If band edges are too hard/soft, adjust `Painterly Smoothness`.
