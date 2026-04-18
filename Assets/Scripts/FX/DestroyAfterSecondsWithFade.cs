using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Destroys this GameObject after a delay. Optionally fades Renderers via MaterialPropertyBlock
// during the last portion of the lifetime (needs transparent / alpha-capable materials to be visible).
public class DestroyAfterSecondsWithFade : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    public enum FadeRendererScope
    {
        /// <summary>Every Renderer under this transform, including on this object.</summary>
        FullHierarchy,
        /// <summary>Only renderers under each direct child (typical for debris roots with no mesh on parent).</summary>
        DirectChildrenSubtrees,
    }

    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool disableCollidersWhenFading = true;
    [SerializeField] private FadeRendererScope rendererScope = FadeRendererScope.DirectChildrenSubtrees;

    readonly struct FadeTarget
    {
        public readonly Renderer Renderer;
        public readonly int MaterialIndex;
        public readonly int ColorPropertyId;
        public readonly Color InitialColor;

        public FadeTarget(Renderer renderer, int materialIndex, int colorPropertyId, Color initialColor)
        {
            Renderer = renderer;
            MaterialIndex = materialIndex;
            ColorPropertyId = colorPropertyId;
            InitialColor = initialColor;
        }
    }

    FadeTarget[] _fadeTargets;
    MaterialPropertyBlock _propertyBlock;
    Collider[] _colliders;

    void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _colliders = GetComponentsInChildren<Collider>(true);

        var scratch = new MaterialPropertyBlock();
        var targets = new List<FadeTarget>();

        foreach (Renderer r in EnumerateFadeRenderers())
        {
            if (r == null)
                continue;

            Material[] mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0)
                continue;

            for (int mi = 0; mi < mats.Length; mi++)
            {
                Material mat = mats[mi];
                if (mat == null)
                    continue;

                int propId;
                Color initial;
                if (!TryResolveFadeColor(r, mi, mat, scratch, out propId, out initial))
                    continue;

                targets.Add(new FadeTarget(r, mi, propId, initial));
            }
        }

        _fadeTargets = targets.Count > 0 ? targets.ToArray() : System.Array.Empty<FadeTarget>();
    }

    IEnumerable<Renderer> EnumerateFadeRenderers()
    {
        if (rendererScope == FadeRendererScope.FullHierarchy || transform.childCount == 0)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                yield return renderers[i];
            yield break;
        }

        for (int c = 0; c < transform.childCount; c++)
        {
            Transform child = transform.GetChild(c);
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                yield return renderers[i];
        }
    }

    void Start()
    {
        StartCoroutine(RunLifetime());
    }

    IEnumerator RunLifetime()
    {
        float total = Mathf.Max(0.01f, lifetime);
        float fade = Mathf.Clamp(fadeDuration, 0f, total);
        float hold = total - fade;

        if (hold > 0f)
            yield return new WaitForSeconds(hold);

        if (disableCollidersWhenFading && _colliders != null)
        {
            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] != null)
                    _colliders[i].enabled = false;
            }
        }

        if (fade <= 0f || _fadeTargets.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fade)
        {
            elapsed += Time.deltaTime;
            float linear = 1f - Mathf.Clamp01(elapsed / fade);
            ApplyAlpha(linear);
            yield return null;
        }

        Destroy(gameObject);
    }

    void ApplyAlpha(float alphaFactor)
    {
        for (int i = 0; i < _fadeTargets.Length; i++)
        {
            FadeTarget t = _fadeTargets[i];
            if (t.Renderer == null)
                continue;

            t.Renderer.GetPropertyBlock(_propertyBlock, t.MaterialIndex);
            Color c = t.InitialColor;
            c.a = t.InitialColor.a * alphaFactor;
            _propertyBlock.SetColor(t.ColorPropertyId, c);
            t.Renderer.SetPropertyBlock(_propertyBlock, t.MaterialIndex);
        }
    }

    static bool TryResolveFadeColor(Renderer renderer, int materialIndex, Material mat, MaterialPropertyBlock scratch, out int propertyId, out Color color)
    {
        propertyId = -1;
        color = Color.white;

        scratch.Clear();
        renderer.GetPropertyBlock(scratch, materialIndex);

        if (scratch.HasColor(BaseColorId))
        {
            propertyId = BaseColorId;
            color = scratch.GetColor(BaseColorId);
            return true;
        }

        if (scratch.HasColor(ColorId))
        {
            propertyId = ColorId;
            color = scratch.GetColor(ColorId);
            return true;
        }

        if (mat.HasProperty(BaseColorId))
        {
            propertyId = BaseColorId;
            color = mat.GetColor(BaseColorId);
            return true;
        }

        if (mat.HasProperty(ColorId))
        {
            propertyId = ColorId;
            color = mat.GetColor(ColorId);
            return true;
        }

        return false;
    }
}
