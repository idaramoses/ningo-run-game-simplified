using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// A high-performance, beautiful UI-based falling confetti controller.
/// Spawns and animates colorful, 3D-tumbling confetti pieces directly on a UI Canvas.
/// Works perfectly in any Canvas render mode, including Screen Space - Overlay.
/// </summary>
public class UIConfettiController : MonoBehaviour
{
    [Header("Confetti Count & Timing")]
    [SerializeField] private int maxConfettiPieces = 80;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Speeds")]
    [SerializeField] private float minFallSpeed = 150f;
    [SerializeField] private float maxFallSpeed = 350f;
    [SerializeField] private float minSwaySpeed = 1.5f;
    [SerializeField] private float maxSwaySpeed = 4f;
    [SerializeField] private float minSwayWidth = 20f;
    [SerializeField] private float maxSwayWidth = 60f;

    [Header("Rotation Speeds (3D Tumbling)")]
    [SerializeField] private float minRotationSpeedX = 50f;
    [SerializeField] private float maxRotationSpeedX = 200f;
    [SerializeField] private float minRotationSpeedY = 100f;
    [SerializeField] private float maxRotationSpeedY = 300f;
    [SerializeField] private float minRotationSpeedZ = 30f;
    [SerializeField] private float maxRotationSpeedZ = 150f;

    [Header("Dimensions")]
    [SerializeField] private float minWidth = 10f;
    [SerializeField] private float maxWidth = 24f;
    [SerializeField] private float minHeight = 14f;
    [SerializeField] private float maxHeight = 30f;

    [Header("Colors")]
    [SerializeField] private Color[] confettiColors = new Color[]
    {
        new Color(1f, 0.22f, 0.22f),    // Vibrant Red
        new Color(0.22f, 0.57f, 1f),    // Vibrant Blue
        new Color(1f, 0.84f, 0f),       // Vibrant Gold/Yellow
        new Color(0.18f, 0.8f, 0.44f),   // Vibrant Green
        new Color(1f, 0.2f, 0.6f),      // Vibrant Pink
        new Color(1f, 0.5f, 0f),        // Vibrant Orange
        new Color(0.6f, 0.2f, 1f),      // Vibrant Purple
        new Color(0.12f, 0.9f, 0.9f)     // Vibrant Cyan
    };

    private class ConfettiPiece
    {
        public RectTransform rectTransform;
        public UnityEngine.UI.Image image;
        public float fallSpeed;
        public float swaySpeed;
        public float swayWidth;
        public float swayOffset;
        public float rotSpeedX;
        public float rotSpeedY;
        public float rotSpeedZ;

        public Vector2 currentPos;
        public float curRotX;
        public float curRotY;
        public float curRotZ;
        public float currentSwayTime;
    }

    private List<ConfettiPiece> activePieces = new List<ConfettiPiece>();
    private RectTransform containerRect;
    private bool isPlaying = false;

    private void Awake()
    {
        containerRect = GetComponent<RectTransform>();
        if (containerRect == null)
        {
            containerRect = gameObject.AddComponent<RectTransform>();
        }
        
        // Stretch container to fill parent Canvas by default
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    /// <summary>
    /// Starts playing/spawning confetti.
    /// </summary>
    public void Play()
    {
        if (isPlaying) return;
        isPlaying = true;

        ClearPieces();
        SpawnConfettiPool();
    }

    /// <summary>
    /// Stops playing and clears all confetti pieces.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        ClearPieces();
    }

    private void ClearPieces()
    {
        foreach (var piece in activePieces)
        {
            if (piece != null && piece.rectTransform != null)
            {
                Destroy(piece.rectTransform.gameObject);
            }
        }
        activePieces.Clear();
    }

    private void SpawnConfettiPool()
    {
        float width = containerRect.rect.width > 0 ? containerRect.rect.width : Screen.width;
        float height = containerRect.rect.height > 0 ? containerRect.rect.height : Screen.height;

        // Ensure we spawn them nicely across the width and staggered vertically
        for (int i = 0; i < maxConfettiPieces; i++)
        {
            CreateSinglePiece(width, height, isInitialSpawn: true);
        }
    }

    private void CreateSinglePiece(float canvasWidth, float canvasHeight, bool isInitialSpawn)
    {
        GameObject go = new GameObject("ConfettiPiece", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();

        // Set dimensions
        float pieceWidth = Random.Range(minWidth, maxWidth);
        float pieceHeight = Random.Range(minHeight, maxHeight);
        rt.sizeDelta = new Vector2(pieceWidth, pieceHeight);

        // Set pivot and anchors to bottom-left so calculations are unified
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Position
        float randomX = Random.Range(0f, canvasWidth);
        // If initial spawn, stagger them across the vertical range of the screen so they are already falling
        // If spawning after running, spawn them just above the top of the canvas
        float randomY = isInitialSpawn ? Random.Range(0f, canvasHeight * 1.2f) : Random.Range(canvasHeight, canvasHeight * 1.1f);
        
        rt.anchoredPosition = new Vector2(randomX, randomY);

        // Set random color
        Color pieceColor = confettiColors[Random.Range(0, confettiColors.Length)];
        img.color = pieceColor;

        // Custom properties
        ConfettiPiece piece = new ConfettiPiece
        {
            rectTransform = rt,
            image = img,
            fallSpeed = Random.Range(minFallSpeed, maxFallSpeed),
            swaySpeed = Random.Range(minSwaySpeed, maxSwaySpeed),
            swayWidth = Random.Range(minSwayWidth, maxSwayWidth),
            swayOffset = Random.Range(0f, Mathf.PI * 2f),
            rotSpeedX = Random.Range(minRotationSpeedX, maxRotationSpeedX) * (Random.value > 0.5f ? 1f : -1f),
            rotSpeedY = Random.Range(minRotationSpeedY, maxRotationSpeedY) * (Random.value > 0.5f ? 1f : -1f),
            rotSpeedZ = Random.Range(minRotationSpeedZ, maxRotationSpeedZ) * (Random.value > 0.5f ? 1f : -1f),
            currentPos = new Vector2(randomX, randomY),
            curRotX = Random.Range(0f, 360f),
            curRotY = Random.Range(0f, 360f),
            curRotZ = Random.Range(0f, 360f),
            currentSwayTime = Random.Range(0f, 100f)
        };

        // Initialize rotation
        rt.localRotation = Quaternion.Euler(piece.curRotX, piece.curRotY, piece.curRotZ);

        activePieces.Add(piece);
    }

    private void Update()
    {
        if (!isPlaying) return;

        float canvasWidth = containerRect.rect.width > 0 ? containerRect.rect.width : Screen.width;
        float canvasHeight = containerRect.rect.height > 0 ? containerRect.rect.height : Screen.height;
        float dt = Time.unscaledDeltaTime; // Unaffected by slow motion / pause

        for (int i = 0; i < activePieces.Count; i++)
        {
            ConfettiPiece piece = activePieces[i];
            if (piece == null || piece.rectTransform == null) continue;

            // 1. Vertical Movement (Gravity)
            piece.currentPos.y -= piece.fallSpeed * dt;

            // 2. Horizontal Swaying (Wind/Floating effect)
            piece.currentSwayTime += dt * piece.swaySpeed;
            float swayValue = Mathf.Sin(piece.currentSwayTime + piece.swayOffset) * piece.swayWidth * dt;
            piece.currentPos.x += swayValue;

            // Keep within horizontal bounds of the canvas
            if (piece.currentPos.x < -50f) piece.currentPos.x = canvasWidth + 50f;
            else if (piece.currentPos.x > canvasWidth + 50f) piece.currentPos.x = -50f;

            // Apply position
            piece.rectTransform.anchoredPosition = piece.currentPos;

            // 3. 3D Tumbling Rotation
            piece.curRotX += piece.rotSpeedX * dt;
            piece.curRotY += piece.rotSpeedY * dt;
            piece.curRotZ += piece.rotSpeedZ * dt;
            piece.rectTransform.localRotation = Quaternion.Euler(piece.curRotX, piece.curRotY, piece.curRotZ);

            // 4. Wrap around or recycle if piece goes below the screen
            if (piece.currentPos.y < -50f)
            {
                // Recycle: put back to top
                piece.currentPos.y = Random.Range(canvasHeight, canvasHeight * 1.1f);
                piece.currentPos.x = Random.Range(0f, canvasWidth);
                piece.fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
                piece.rectTransform.anchoredPosition = piece.currentPos;
            }
        }
    }
}