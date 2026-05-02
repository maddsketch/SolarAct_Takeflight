using System.Collections.Generic;
using UnityEngine;

// Pulses renderer alpha while active, then restores original values.
public class PlayerInvincibilityPulse : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly struct PulseTarget
    {
        public readonly Renderer Renderer;
        public readonly int MaterialIndex;
        public readonly int ColorPropertyId;
        public readonly Color InitialColor;

        public PulseTarget(Renderer renderer, int materialIndex, int colorPropertyId, Color initialColor)
        {
            Renderer = renderer;
            MaterialIndex = materialIndex;
            ColorPropertyId = colorPropertyId;
            InitialColor = initialColor;
        }
    }

    [SerializeField, Range(0.05f, 1f)] private float minAlphaFactor = 0.35f;
    [SerializeField, Range(0.1f, 1f)] private float minBrightnessFactor = 0.6f;
    [SerializeField, Min(0f)] private float pulseFrequency = 8f;
    [SerializeField] private bool includeInactiveChildren = true;

    private readonly List<PulseTarget> targets = new List<PulseTarget>();
    private MaterialPropertyBlock propertyBlock;
    private float pulseUntilTime;
    private bool pulseActive;
    private float pulseSeed;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        pulseSeed = Random.Range(0f, 1000f);
        CacheTargets();
    }

    void OnEnable()
    {
        if (targets.Count == 0)
            CacheTargets();
    }

    void Update()
    {
        if (!pulseActive)
            return;

        float remaining = pulseUntilTime - Time.time;
        if (remaining <= 0f)
        {
            Restore();
            pulseActive = false;
            return;
        }

        ApplyPulse();
    }

    void OnDisable()
    {
        Restore();
        pulseActive = false;
    }

    public void StartPulse(float duration)
    {
        if (duration <= 0f)
            return;

        if (targets.Count == 0)
            CacheTargets();
        if (targets.Count == 0)
            return;

        pulseUntilTime = Mathf.Max(pulseUntilTime, Time.time + duration);
        pulseActive = true;
    }

    private void ApplyPulse()
    {
        float wave = Mathf.Sin((Time.time + pulseSeed) * Mathf.Max(0f, pulseFrequency) * Mathf.PI * 2f) * 0.5f + 0.5f;
        float alphaFactor = Mathf.Lerp(minAlphaFactor, 1f, wave);
        float brightnessFactor = Mathf.Lerp(minBrightnessFactor, 1f, wave);

        for (int i = 0; i < targets.Count; i++)
        {
            PulseTarget target = targets[i];
            if (target.Renderer == null)
                continue;

            target.Renderer.GetPropertyBlock(propertyBlock, target.MaterialIndex);
            Color c = target.InitialColor;
            c.r *= brightnessFactor;
            c.g *= brightnessFactor;
            c.b *= brightnessFactor;
            c.a *= alphaFactor;
            propertyBlock.SetColor(target.ColorPropertyId, c);
            target.Renderer.SetPropertyBlock(propertyBlock, target.MaterialIndex);
        }
    }

    private void Restore()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            PulseTarget target = targets[i];
            if (target.Renderer == null)
                continue;

            target.Renderer.GetPropertyBlock(propertyBlock, target.MaterialIndex);
            propertyBlock.SetColor(target.ColorPropertyId, target.InitialColor);
            target.Renderer.SetPropertyBlock(propertyBlock, target.MaterialIndex);
        }
    }

    private void CacheTargets()
    {
        targets.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
        var scratch = new MaterialPropertyBlock();
        for (int r = 0; r < renderers.Length; r++)
        {
            Renderer renderer = renderers[r];
            if (renderer == null)
                continue;

            Material[] mats = renderer.sharedMaterials;
            if (mats == null || mats.Length == 0)
                continue;

            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null)
                    continue;

                if (!TryResolveColor(renderer, i, mat, scratch, out int colorId, out Color initialColor))
                    continue;

                targets.Add(new PulseTarget(renderer, i, colorId, initialColor));
            }
        }
    }

    private static bool TryResolveColor(Renderer renderer, int materialIndex, Material mat, MaterialPropertyBlock scratch, out int colorPropertyId, out Color color)
    {
        colorPropertyId = -1;
        color = Color.white;

        scratch.Clear();
        renderer.GetPropertyBlock(scratch, materialIndex);

        if (scratch.HasColor(BaseColorId))
        {
            colorPropertyId = BaseColorId;
            color = scratch.GetColor(BaseColorId);
            return true;
        }

        if (scratch.HasColor(ColorId))
        {
            colorPropertyId = ColorId;
            color = scratch.GetColor(ColorId);
            return true;
        }

        if (mat.HasProperty(BaseColorId))
        {
            colorPropertyId = BaseColorId;
            color = mat.GetColor(BaseColorId);
            return true;
        }

        if (mat.HasProperty(ColorId))
        {
            colorPropertyId = ColorId;
            color = mat.GetColor(ColorId);
            return true;
        }

        return false;
    }
}
