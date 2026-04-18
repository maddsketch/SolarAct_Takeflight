// <summary>
// Drives TakeFlight/URP/ScreenSpaceRefractionBubble material settings via MaterialPropertyBlock on a Renderer.
// - Animator / Animation window: keyframe the public field <c>blend</c> (0 = start snapshot, 1 = end snapshot).
// - Script / Animation event: call <see cref="PlayForward"/> or <see cref="PlayReverse"/> to run a timed curve (do not keyframe <c>blend</c> on the same clip while a coroutine plays).
// - Physics: enable <see cref="useColliderTrigger"/> and use a trigger Collider on this GameObject.
// Shader: Assets/Shaders/URP_ScreenSpaceRefractionBubble.shader
// </summary>
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BubbleMaterialTriggerAnimator : MonoBehaviour
{
    const string ExpectedShaderName = "TakeFlight/URP/ScreenSpaceRefractionBubble";

    static readonly int BubbleCenterId = Shader.PropertyToID("_BubbleCenter");
    static readonly int BubbleRadiusId = Shader.PropertyToID("_BubbleRadius");
    static readonly int BubbleStrengthId = Shader.PropertyToID("_BubbleStrength");
    static readonly int EdgeSoftnessId = Shader.PropertyToID("_EdgeSoftness");
    static readonly int NormalBlendId = Shader.PropertyToID("_NormalBlend");
    static readonly int TintId = Shader.PropertyToID("_Tint");
    static readonly int ChromaticAberrationId = Shader.PropertyToID("_ChromaticAberration");
    static readonly int RimPowerId = Shader.PropertyToID("_RimPower");
    static readonly int RimColorId = Shader.PropertyToID("_RimColor");
    static readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");
    static readonly int RimAlphaFalloffId = Shader.PropertyToID("_RimAlphaFalloff");

    [Flags]
    public enum BubbleAnimatedFields
    {
        None = 0,
        BubbleCenter = 1 << 0,
        BubbleRadius = 1 << 1,
        BubbleStrength = 1 << 2,
        EdgeSoftness = 1 << 3,
        NormalBlend = 1 << 4,
        Tint = 1 << 5,
        ChromaticAberration = 1 << 6,
        RimPower = 1 << 7,
        RimColor = 1 << 8,
        RimIntensity = 1 << 9,
        RimAlphaFalloff = 1 << 10,
        All = ~0
    }

    [Serializable]
    public class BubbleMaterialSnapshot
    {
        public Vector4 bubbleCenter = new Vector4(0.5f, 0.5f, 0f, 0f);
        public float bubbleRadius = 0.42f;
        public float bubbleStrength = 0.14f;
        public float edgeSoftness = 0.12f;
        public float normalBlend = 0.04f;
        public Color tint = new Color(1f, 1f, 1f, 0.35f);
        public float chromaticAberration = 0.005f;
        public float rimPower = 3.5f;
        public Color rimColor = new Color(0.85f, 0.95f, 1f, 1f);
        public float rimIntensity = 0.35f;
        public float rimAlphaFalloff = 0.4f;
    }

    [Header("Target")]
    [SerializeField] Renderer targetRenderer;
    [SerializeField] [Min(0)] int materialIndex;

    [Header("Snapshots")]
    [SerializeField] BubbleMaterialSnapshot startSnapshot = new BubbleMaterialSnapshot();
    [SerializeField] BubbleMaterialSnapshot endSnapshot = new BubbleMaterialSnapshot();
    [SerializeField] BubbleAnimatedFields animatedFields = BubbleAnimatedFields.All;

    [Header("Blend (Animator / script)")]
    [Range(0f, 1f)] [SerializeField] float blend;

    [Header("Coroutine playback")]
    [SerializeField] float duration = 0.5f;
    [SerializeField] AnimationCurve blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Optional physics trigger")]
    [SerializeField] bool useColliderTrigger;
    [SerializeField] string requiredTag = "";
    [SerializeField] bool oneShot;

    MaterialPropertyBlock _mpb;
    Coroutine _playRoutine;
    bool _shaderCheckDone;
    bool _triggerFired;

    public float Blend
    {
        get => blend;
        set => blend = Mathf.Clamp01(value);
    }

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        _mpb = new MaterialPropertyBlock();

        if (useColliderTrigger && GetComponent<Collider>() == null)
            Debug.LogWarning($"[{nameof(BubbleMaterialTriggerAnimator)}] useColliderTrigger is enabled but no Collider is on this GameObject.", this);
    }

    void LateUpdate()
    {
        Apply();
    }

    void OnValidate()
    {
        blend = Mathf.Clamp01(blend);
        if (blendCurve == null || blendCurve.length == 0)
            blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useColliderTrigger || _triggerFired)
            return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        PlayForward();
        if (oneShot)
            _triggerFired = true;
    }

    /// <summary>Animation event / UI / script: play start → end over <see cref="duration"/> using <see cref="blendCurve"/>.</summary>
    public void PlayForward()
    {
        if (_playRoutine != null)
            StopCoroutine(_playRoutine);
        _playRoutine = StartCoroutine(PlayRoutine(forward: true));
    }

    /// <summary>Play end → start (curve time reversed).</summary>
    public void PlayReverse()
    {
        if (_playRoutine != null)
            StopCoroutine(_playRoutine);
        _playRoutine = StartCoroutine(PlayRoutine(forward: false));
    }

    IEnumerator PlayRoutine(bool forward)
    {
        blend = forward ? 0f : 1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float c = blendCurve.Evaluate(forward ? p : 1f - p);
            blend = Mathf.Clamp01(c);
            yield return null;
        }

        blend = forward ? 1f : 0f;
        _playRoutine = null;
    }

    void Apply()
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        Material mat = mats[materialIndex];
        if (mat == null)
            return;

        if (!_shaderCheckDone)
        {
            _shaderCheckDone = true;
            if (mat.shader != null)
            {
                string n = mat.shader.name;
                if (n != ExpectedShaderName && n.IndexOf("ScreenSpaceRefractionBubble", StringComparison.Ordinal) < 0)
                {
                    Debug.LogWarning(
                        $"[{nameof(BubbleMaterialTriggerAnimator)}] Material '{mat.name}' uses shader '{n}', expected '{ExpectedShaderName}' (or name containing ScreenSpaceRefractionBubble). Property IDs may not match.",
                        this);
                }
            }
        }

        BubbleAnimatedFields f = animatedFields;

        Vector4 bubbleCenter = startSnapshot.bubbleCenter;
        if ((f & BubbleAnimatedFields.BubbleCenter) != 0)
            bubbleCenter = Vector4.Lerp(startSnapshot.bubbleCenter, endSnapshot.bubbleCenter, blend);

        float bubbleRadius = PickFloat(f, BubbleAnimatedFields.BubbleRadius, startSnapshot.bubbleRadius, endSnapshot.bubbleRadius, blend);
        float bubbleStrength = PickFloat(f, BubbleAnimatedFields.BubbleStrength, startSnapshot.bubbleStrength, endSnapshot.bubbleStrength, blend);
        float edgeSoftness = PickFloat(f, BubbleAnimatedFields.EdgeSoftness, startSnapshot.edgeSoftness, endSnapshot.edgeSoftness, blend);
        float normalBlend = PickFloat(f, BubbleAnimatedFields.NormalBlend, startSnapshot.normalBlend, endSnapshot.normalBlend, blend);
        Color tint = PickColor(f, BubbleAnimatedFields.Tint, startSnapshot.tint, endSnapshot.tint, blend);
        float chromatic = PickFloat(f, BubbleAnimatedFields.ChromaticAberration, startSnapshot.chromaticAberration, endSnapshot.chromaticAberration, blend);
        float rimPower = PickFloat(f, BubbleAnimatedFields.RimPower, startSnapshot.rimPower, endSnapshot.rimPower, blend);
        Color rimColor = PickColor(f, BubbleAnimatedFields.RimColor, startSnapshot.rimColor, endSnapshot.rimColor, blend);
        float rimIntensity = PickFloat(f, BubbleAnimatedFields.RimIntensity, startSnapshot.rimIntensity, endSnapshot.rimIntensity, blend);
        float rimAlphaFalloff = PickFloat(f, BubbleAnimatedFields.RimAlphaFalloff, startSnapshot.rimAlphaFalloff, endSnapshot.rimAlphaFalloff, blend);

        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        _mpb.SetVector(BubbleCenterId, bubbleCenter);
        _mpb.SetFloat(BubbleRadiusId, bubbleRadius);
        _mpb.SetFloat(BubbleStrengthId, bubbleStrength);
        _mpb.SetFloat(EdgeSoftnessId, edgeSoftness);
        _mpb.SetFloat(NormalBlendId, normalBlend);
        _mpb.SetColor(TintId, tint);
        _mpb.SetFloat(ChromaticAberrationId, chromatic);
        _mpb.SetFloat(RimPowerId, rimPower);
        _mpb.SetColor(RimColorId, rimColor);
        _mpb.SetFloat(RimIntensityId, rimIntensity);
        _mpb.SetFloat(RimAlphaFalloffId, rimAlphaFalloff);
        targetRenderer.SetPropertyBlock(_mpb, materialIndex);
    }

    static float PickFloat(BubbleAnimatedFields mask, BubbleAnimatedFields bit, float a, float b, float t)
    {
        if ((mask & bit) == 0)
            return a;
        return Mathf.LerpUnclamped(a, b, t);
    }

    static Color PickColor(BubbleAnimatedFields mask, BubbleAnimatedFields bit, Color a, Color b, float t)
    {
        if ((mask & bit) == 0)
            return a;
        return Color.LerpUnclamped(a, b, t);
    }
}
