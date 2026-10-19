using UnityEngine;
using UnityEditor;

public static class FixCoinMaterial
{
    [MenuItem("Tools/FX/Rebuild Collect FX")]
    public static void BuildHighQualityCollectFX()
    {
        // 1. Create a clean 128x128 4-point star / glow sparkle texture
        string texDir = "Assets/Texte";
        string sparkleTexPath = $"{texDir}/FX_Sparkle_Glow.png";
        int sz = 128;
        Texture2D sparkTex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        float half = (sz - 1) * 0.5f;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                float dx = (x - half) / half; // -1 to 1
                float dy = (y - half) / half;
                float r = Mathf.Sqrt(dx * dx + dy * dy);

                // Circular glow falloff
                float glow = Mathf.Clamp01(1f - r);
                glow = Mathf.Pow(glow, 2.5f);

                // 4-point cross streaks
                float streakX = Mathf.Clamp01(1f - Mathf.Abs(dx)) * Mathf.Clamp01(1f - Mathf.Abs(dy * 6f));
                float streakY = Mathf.Clamp01(1f - Mathf.Abs(dy)) * Mathf.Clamp01(1f - Mathf.Abs(dx * 6f));
                float streak = Mathf.Max(streakX, streakY);

                float intensity = Mathf.Clamp01(glow * 0.7f + streak * 0.9f);
                sparkTex.SetPixel(x, y, new Color(1f, 1f, 1f, intensity));
            }
        }
        sparkTex.Apply();
        System.IO.File.WriteAllBytes(sparkleTexPath, sparkTex.EncodeToPNG());
        AssetDatabase.ImportAsset(sparkleTexPath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(sparkleTexPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        // 2. Create Additive Materials
        Shader addShader = Shader.Find("Mobile/Particles/Additive");
        if (addShader == null) addShader = Shader.Find("Particles/Standard Unlit");

        string matDir = "Assets/Texte/Materials";
        string sparkleMatPath = $"{matDir}/M_FX_Sparkle_Gold.mat";
        Material sparkMat = AssetDatabase.LoadAssetAtPath<Material>(sparkleMatPath);
        if (sparkMat == null)
        {
            sparkMat = new Material(addShader);
            AssetDatabase.CreateAsset(sparkMat, sparkleMatPath);
        }
        sparkMat.shader = addShader;
        sparkMat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sparkleTexPath);
        sparkMat.SetColor("_TintColor", new Color(1f, 0.88f, 0.35f, 1f));
        EditorUtility.SetDirty(sparkMat);

        string flashMatPath = $"{matDir}/M_FX_RingFlash_Gold.mat";
        Material flashMat = AssetDatabase.LoadAssetAtPath<Material>(flashMatPath);
        if (flashMat == null)
        {
            flashMat = new Material(addShader);
            AssetDatabase.CreateAsset(flashMat, flashMatPath);
        }
        flashMat.shader = addShader;
        flashMat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sparkleTexPath);
        flashMat.SetColor("_TintColor", new Color(1f, 0.95f, 0.6f, 1f));
        EditorUtility.SetDirty(flashMat);

        // 3. Build the Coin & Item Collect Particle Prefab
        string prefabPath = "Assets/Prefab/patical/hieuungcoin.prefab";
        GameObject root = new GameObject("hieuungcoin");

        // Main Layer: Sparkle Stars Burst (12-16 golden glowing stars exploding outward)
        var psStars = root.AddComponent<ParticleSystem>();
        var renStars = root.GetComponent<ParticleSystemRenderer>();
        renStars.renderMode = ParticleSystemRenderMode.Billboard;
        renStars.sharedMaterial = sparkMat;

        var mainStars = psStars.main;
        mainStars.duration = 0.5f;
        mainStars.loop = false;
        mainStars.playOnAwake = true;
        mainStars.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.75f);
        mainStars.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7.0f);
        mainStars.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
        mainStars.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        mainStars.simulationSpace = ParticleSystemSimulationSpace.World;
        mainStars.gravityModifier = 1.2f; // nice natural gravity arc

        // Color over lifetime: bright gold fading out smoothly
        var colorModule = psStars.colorOverLifetime;
        colorModule.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.7f, 0.1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) }
        );
        colorModule.color = grad;

        // Size over lifetime: pop up then shrink
        var sizeModule = psStars.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.2f);
        sizeCurve.AddKey(0.15f, 1.0f);
        sizeCurve.AddKey(1f, 0.0f);
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Rotation over lifetime: spinning stars
        var rotModule = psStars.rotationOverLifetime;
        rotModule.enabled = true;
        rotModule.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

        // Emission: single burst of 14 particles
        var emissionStars = psStars.emission;
        emissionStars.rateOverTime = 0;
        emissionStars.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 14) });

        // Shape: outward sphere burst
        var shapeStars = psStars.shape;
        shapeStars.shapeType = ParticleSystemShapeType.Sphere;
        shapeStars.radius = 0.2f;

        // Sub Layer: Central Glow Flash Pop
        GameObject flashChild = new GameObject("CenterFlash");
        flashChild.transform.SetParent(root.transform, false);
        var psFlash = flashChild.AddComponent<ParticleSystem>();
        var renFlash = flashChild.GetComponent<ParticleSystemRenderer>();
        renFlash.renderMode = ParticleSystemRenderMode.Billboard;
        renFlash.sharedMaterial = flashMat;

        var mainFlash = psFlash.main;
        mainFlash.duration = 0.3f;
        mainFlash.loop = false;
        mainFlash.playOnAwake = true;
        mainFlash.startLifetime = 0.22f;
        mainFlash.startSpeed = 0f;
        mainFlash.startSize = 1.4f;
        mainFlash.simulationSpace = ParticleSystemSimulationSpace.World;

        var flashColor = psFlash.colorOverLifetime;
        flashColor.enabled = true;
        Gradient flashGrad = new Gradient();
        flashGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.85f, 0.3f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        flashColor.color = flashGrad;

        var flashSize = psFlash.sizeOverLifetime;
        flashSize.enabled = true;
        AnimationCurve flashSizeCurve = new AnimationCurve();
        flashSizeCurve.AddKey(0f, 0.5f);
        flashSizeCurve.AddKey(0.3f, 1.2f);
        flashSizeCurve.AddKey(1f, 0.0f);
        flashSize.size = new ParticleSystem.MinMaxCurve(1f, flashSizeCurve);

        var flashEmission = psFlash.emission;
        flashEmission.rateOverTime = 0;
        flashEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

        var flashShape = psFlash.shape;
        flashShape.enabled = false;

        // Sub Layer: Upward Floating Glow Flecks
        GameObject flecksChild = new GameObject("GlowFlecks");
        flecksChild.transform.SetParent(root.transform, false);
        var psFlecks = flecksChild.AddComponent<ParticleSystem>();
        var renFlecks = flecksChild.GetComponent<ParticleSystemRenderer>();
        renFlecks.renderMode = ParticleSystemRenderMode.Billboard;
        renFlecks.sharedMaterial = sparkMat;

        var mainFlecks = psFlecks.main;
        mainFlecks.duration = 0.4f;
        mainFlecks.loop = false;
        mainFlecks.playOnAwake = true;
        mainFlecks.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
        mainFlecks.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        mainFlecks.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        mainFlecks.simulationSpace = ParticleSystemSimulationSpace.World;
        mainFlecks.gravityModifier = -0.5f; // float upward!

        var flecksEmission = psFlecks.emission;
        flecksEmission.rateOverTime = 0;
        flecksEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var flecksShape = psFlecks.shape;
        flecksShape.shapeType = ParticleSystemShapeType.Cone;
        flecksShape.angle = 35f;
        flecksShape.radius = 0.1f;

        var flecksColor = psFlecks.colorOverLifetime;
        flecksColor.enabled = true;
        flecksColor.color = grad;

        var flecksSize = psFlecks.sizeOverLifetime;
        flecksSize.enabled = true;
        flecksSize.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        // Update SoundEffectsManager in scene if present
        var sfx = Object.FindAnyObjectByType<SoundEffectsManager>();
        if (sfx != null)
        {
            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            sfx.coinCollectEffectPrefab = newPrefab;
            sfx.letterCollectEffectPrefab = newPrefab;
            EditorUtility.SetDirty(sfx);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[FixCoinMaterial] Successfully upgraded hieuungcoin.prefab to a stunning multi-layer arcade star & sparkle burst!");
    }
}
