using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight confetti for UI canvases. Spawns colored rect pieces at the top
/// of this RectTransform and lets them fall, drift and spin. Runs on unscaled
/// time so it keeps animating while Time.timeScale = 0 (pause/complete panels).
/// Pieces recycle when they pass the bottom edge.
/// </summary>
public class UIConfetti : MonoBehaviour
{
    [Header("Emission")]
    public int pieceCount = 60;
    public float spawnWidth = 800f;          // pieces spawn across this width
    public float spawnHeight = 120f;         // random spawn band above the top

    [Header("Piece Look")]
    public Vector2 sizeRange = new Vector2(8f, 18f);
    public Color[] colors =
    {
        new Color(1f, 0.31f, 0.31f),   // red
        new Color(1f, 0.84f, 0.10f),   // yellow
        new Color(0.20f, 0.80f, 0.36f),// green
        new Color(0.20f, 0.56f, 1f),   // blue
        new Color(0.75f, 0.35f, 1f),   // purple
        new Color(1f, 0.55f, 0.15f),   // orange
    };

    [Header("Motion")]
    public Vector2 fallSpeed = new Vector2(120f, 260f);   // px/s down
    public Vector2 driftSpeed = new Vector2(-60f, 60f);   // px/s sideways
    public Vector2 spinSpeed = new Vector2(-360f, 360f);  // deg/s
    public float swayAmount = 40f;                        // sinus sway amplitude
    public float swayFrequency = 2f;

    private class Piece
    {
        public RectTransform rt;
        public float speed;
        public float drift;
        public float spin;
        public float phase;
        public float baseX;
    }

    private readonly List<Piece> pieces = new List<Piece>();
    private RectTransform area;
    private Sprite sprite;

    private void OnEnable()
    {
        area = (RectTransform)transform;
        if (sprite == null) sprite = GetWhiteSprite();
        SpawnAll();
    }

    private void OnDisable()
    {
        Clear();
    }

    private void SpawnAll()
    {
        Clear();
        float halfW = spawnWidth * 0.5f;
        float top = area.rect.height * 0.5f;

        for (int i = 0; i < pieceCount; i++)
        {
            var go = new GameObject("confetti", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(area, false);

            var p = new Piece
            {
                rt = rt,
                speed = Random.Range(fallSpeed.x, fallSpeed.y),
                drift = Random.Range(driftSpeed.x, driftSpeed.y),
                spin = Random.Range(spinSpeed.x, spinSpeed.y),
                phase = Random.Range(0f, Mathf.PI * 2f),
                baseX = Random.Range(-halfW, halfW),
            };

            float size = Random.Range(sizeRange.x, sizeRange.y);
            rt.sizeDelta = new Vector2(size, size * Random.Range(0.6f, 1.4f));
            rt.anchoredPosition = new Vector2(p.baseX, top + Random.Range(0f, spawnHeight) + (float)i / pieceCount * area.rect.height);
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = colors[Random.Range(0, colors.Length)];
            img.raycastTarget = false;

            pieces.Add(p);
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        float bottom = -area.rect.height * 0.5f - 30f;
        float top = area.rect.height * 0.5f + 30f;
        float halfW = spawnWidth * 0.5f;

        foreach (var p in pieces)
        {
            Vector2 pos = p.rt.anchoredPosition;
            pos.y -= p.speed * dt;

            if (pos.y < bottom)
            {
                // recycle to the top
                pos.y = top + Random.Range(0f, spawnHeight);
                p.baseX = Random.Range(-halfW, halfW);
            }

            p.phase += swayFrequency * dt;
            pos.x = p.baseX + Mathf.Sin(p.phase) * swayAmount + p.drift * 0.02f;
            p.rt.anchoredPosition = pos;
            p.rt.Rotate(0f, 0f, p.spin * dt);
        }
    }

    private void Clear()
    {
        foreach (var p in pieces)
            if (p.rt != null) Destroy(p.rt.gameObject);
        pieces.Clear();
    }

    private static Sprite whiteSprite;
    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;
        var tex = Texture2D.whiteTexture;
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return whiteSprite;
    }
}
