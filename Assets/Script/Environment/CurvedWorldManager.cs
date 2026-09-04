using UnityEngine;
using System.Collections.Generic;

public class CurvedWorldManager : MonoBehaviour
{
    public static CurvedWorldManager Instance { get; private set; }

    [Header("Curvature Settings")]
    [Range(-0.01f, 0.01f)]
    public float curveStrength = 0.0015f;

    [Header("Shaders")]
    public Shader curvedStandardShader;
    public Shader curvedStandardAlphaShader;
    public Shader curvedUnlitShader;
    public Shader curvedUnlitAlphaShader;

    private int curveStrengthID;

    // Cache: original shared material -> curved variant (or the original itself when no
    // conversion is needed). Each unique material is converted exactly ONCE and the
    // curved instance is SHARED by every renderer that used the original. This keeps
    // dynamic/static batching alive on device (one material = one draw-call bucket)
    // instead of cloning a unique material per renderer via r.materials, which
    // multiplied draw calls and GC pressure on mobile.
    private static readonly Dictionary<Material, Material> curvedMaterialCache = new Dictionary<Material, Material>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        curveStrengthID = Shader.PropertyToID("_CurveStrength");
        
        if (curvedStandardShader == null)
            curvedStandardShader = Shader.Find("Custom/CurvedStandard");
        if (curvedStandardAlphaShader == null)
            curvedStandardAlphaShader = Shader.Find("Custom/CurvedStandardAlpha");
        if (curvedUnlitShader == null)
            curvedUnlitShader = Shader.Find("Unlit/CurvedUnlit");
        if (curvedUnlitAlphaShader == null)
            curvedUnlitAlphaShader = Shader.Find("Unlit/CurvedUnlitAlpha");

        ApplyCurvatureToScene();
    }

    private float lastAppliedCurveStrength = float.NaN;

    private void Update()
    {
        // Update global shader variable only when the value actually changes
        if (curveStrength != lastAppliedCurveStrength)
        {
            lastAppliedCurveStrength = curveStrength;
            Shader.SetGlobalFloat(curveStrengthID, curveStrength);
        }
    }

    public void ApplyCurvatureToScene()
    {
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            ApplyCurvatureToRenderer(r);
        }
    }

    public void ApplyCurvatureToGameObject(GameObject go)
    {
        if (go == null) return;
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            ApplyCurvatureToRenderer(r);
        }
    }

    public void ApplyCurvatureToRenderer(Renderer r)
    {
        if (r == null || r.sharedMaterials == null) return;
        if (r is CanvasRenderer) return;

        // Auto-detect potholes to disable shadow-casting and snap them to the ground road surface
        if (r.gameObject.name.ToLower().Contains("pothole"))
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            
            GroundSnap snap = r.gameObject.GetComponent<GroundSnap>();
            if (snap == null)
            {
                snap = r.gameObject.AddComponent<GroundSnap>();
            }
            snap.yOffset = 0.015f;
            snap.Snap();
        }

        Material[] shared = r.sharedMaterials;
        Material[] newMats = null;

        for (int i = 0; i < shared.Length; i++)
        {
            Material m = shared[i];
            if (m == null || m.shader == null) continue;

            Material converted = GetCurvedMaterial(m);
            if (converted != m)
            {
                if (newMats == null) newMats = (Material[])shared.Clone();
                newMats[i] = converted;
            }
        }

        if (newMats != null)
        {
            r.sharedMaterials = newMats;
        }
    }

    /// <summary>
    /// Return the shared curved variant of a material, converting it once and caching
    /// the result. Returns the original material when no conversion is needed.
    /// </summary>
    private Material GetCurvedMaterial(Material original)
    {
        if (curvedMaterialCache.TryGetValue(original, out Material cached))
        {
            if (cached != null) return cached; // Unity null-check also catches destroyed materials
            curvedMaterialCache.Remove(original); // stale entry (destroyed on scene unload) - reconvert
        }

        Material result = ConvertMaterial(original);

        // Enable GPU instancing on the shared curved clone. CurvedStandard/CurvedStandardAlpha
        // declare multi_compile_instancing, so every renderer sharing this curved material can be
        // batched into a single instanced draw call - the biggest draw-call win for the city tiles
        // (some road prefabs have hundreds of renderers). Only applied to clones we own, never to
        // the original shared source material.
        if (result != null && result != original)
        {
            result.enableInstancing = true;
        }

        curvedMaterialCache[original] = result;
        return result;
    }

    private Material ConvertMaterial(Material m)
    {
        string shaderName = m.shader.name;

        // Handle Sprites/Default first before transparency check, since sprite shaders have transparent queues
        // but are not standard materials and would otherwise be skipped by the isTransparent block.
        if (shaderName == "Sprites/Default")
        {
            if (curvedUnlitAlphaShader != null)
            {
                Material clone = new Material(m);
                clone.shader = curvedUnlitAlphaShader;
                return clone;
            }
            return m;
        }

        // Check if the material is transparent (glass, windows, leaves, oceans, etc.)
        string lowerName = m.name.ToLower();
        bool isTransparent = m.renderQueue >= 3000 ||
                             lowerName.Contains("glass") ||
                             lowerName.Contains("transparent") ||
                             lowerName.Contains("leaf") ||
                             lowerName.Contains("leaves") ||
                             (m.HasProperty("_Mode") && m.GetFloat("_Mode") > 0);

        if (isTransparent)
        {
            if (shaderName == "Standard" || shaderName == "Standard (Specular setup)" || shaderName == "glTF/PbrMetallicRoughness" || shaderName.Contains("Transparent"))
            {
                if (curvedStandardAlphaShader != null)
                {
                    Material clone = new Material(m);
                    clone.shader = curvedStandardAlphaShader;
                    return clone;
                }
            }
            else if (shaderName.StartsWith("Unlit/") && shaderName != "Unlit/CurvedUnlitAlpha")
            {
                if (curvedUnlitAlphaShader != null)
                {
                    Material clone = new Material(m);
                    clone.shader = curvedUnlitAlphaShader;
                    return clone;
                }
            }
            return m;
        }

        if (shaderName == "Standard" || shaderName == "Standard (Specular setup)" || shaderName == "Legacy Shaders/Diffuse")
        {
            if (curvedStandardShader != null)
            {
                Material clone = new Material(m);
                clone.shader = curvedStandardShader;
                return clone;
            }
        }
        else if (shaderName == "glTF/PbrMetallicRoughness")
        {
            if (curvedStandardShader != null)
            {
                // Copy glTF specific texture properties to Standard shader properties so they don't turn white/gray
                Texture baseTex = m.GetTexture("baseColorTexture");
                Texture normalTex = m.GetTexture("normalTexture");
                Color baseColor = m.HasProperty("baseColorFactor") ? m.GetColor("baseColorFactor") : Color.white;

                Material clone = new Material(m);
                clone.shader = curvedStandardShader;

                if (baseTex != null) clone.SetTexture("_MainTex", baseTex);
                if (normalTex != null) clone.SetTexture("_BumpMap", normalTex);
                if (clone.HasProperty("_Color")) clone.SetColor("_Color", baseColor);

                return clone;
            }
        }
        else if (shaderName.StartsWith("Unlit/") && shaderName != "Unlit/CurvedUnlit" && shaderName != "Unlit/CurvedUnlitAlpha")
        {
            if (curvedUnlitShader != null)
            {
                Material clone = new Material(m);
                clone.shader = curvedUnlitShader;
                return clone;
            }
        }

        return m;
    }
}
