using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TapEffectManager : MonoBehaviour
{
    [Header("Paw Settings")]
    [SerializeField] private Sprite pawSprite;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private int maxSimultaneousPaws = 5;
    
    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float endScale = 1.2f;
    [SerializeField] private float rotationVariance = 30f;
    [SerializeField] private Color pawColor = new Color(1f, 1f, 1f, 0.8f);

    [Header("Tap Detection")]
    [SerializeField] private float dragThreshold = 10f; // Pixels moved before considered a drag

    private Queue<GameObject> pawPool = new Queue<GameObject>();
    private List<GameObject> activePaws = new List<GameObject>();
    private Transform poolParent;
    private Canvas effectCanvas;

    private Vector2 touchStartPos;
    private bool isDragging;

    private static TapEffectManager instance;
    public static TapEffectManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Create a dedicated overlay canvas for tap effects with highest sorting order
        CreateEffectCanvas();

        // Create pool parent
        poolParent = new GameObject("PawPool").transform;
        poolParent.SetParent(effectCanvas.transform);
        poolParent.gameObject.SetActive(false);

        // Initialize pool
        InitializePool();
    }

    private void CreateEffectCanvas()
    {
        GameObject canvasObj = new GameObject("TapEffectCanvas");
        canvasObj.transform.SetParent(transform);
        
        effectCanvas = canvasObj.AddComponent<Canvas>();
        effectCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        effectCanvas.sortingOrder = 9999; // Highest order to render on top of everything
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.matchWidthOrHeight = 0.5f;

        // No GraphicRaycaster needed - we don't want to block clicks
    }

    private void Update()
    {
        // Track mouse/touch down
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isDragging = false;
        }

        // Check if user is dragging/swiping
        if (Input.GetMouseButton(0))
        {
            float distance = Vector2.Distance(touchStartPos, Input.mousePosition);
            if (distance > dragThreshold)
            {
                isDragging = true;
            }
        }

        // On release, show paw only if it was a tap (not a drag/swipe)
        if (Input.GetMouseButtonUp(0))
        {
            if (!isDragging)
            {
                Vector2 screenPos = Input.mousePosition;
                ShowPawEffect(screenPos);
            }
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject paw = CreatePawObject();
            paw.transform.SetParent(poolParent);
            paw.SetActive(false);
            pawPool.Enqueue(paw);
        }
    }

    private GameObject CreatePawObject()
    {
        GameObject paw = new GameObject("Paw");
        RectTransform rect = paw.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);

        Image img = paw.AddComponent<Image>();
        img.sprite = pawSprite;
        img.color = pawColor;
        img.raycastTarget = false;

        CanvasGroup cg = paw.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        return paw;
    }

    private void ShowPawEffect(Vector2 screenPosition)
    {
        // Limit simultaneous paws
        if (activePaws.Count >= maxSimultaneousPaws)
            return;

        GameObject paw = GetPawFromPool();
        if (paw == null) return;

        RectTransform rect = paw.GetComponent<RectTransform>();
        rect.SetParent(effectCanvas.transform);
        rect.SetAsLastSibling();

        // Convert screen position to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            effectCanvas.transform as RectTransform,
            screenPosition,
            effectCanvas.worldCamera,
            out Vector2 localPos
        );

        rect.anchoredPosition = localPos;
        paw.SetActive(true);

        activePaws.Add(paw);
        StartCoroutine(AnimatePaw(paw));
    }

    private GameObject GetPawFromPool()
    {
        if (pawPool.Count > 0)
            return pawPool.Dequeue();

        // Pool exhausted, create new one
        return CreatePawObject();
    }

    private void ReturnPawToPool(GameObject paw)
    {
        activePaws.Remove(paw);
        paw.SetActive(false);
        paw.transform.SetParent(poolParent);
        pawPool.Enqueue(paw);
    }

    private IEnumerator AnimatePaw(GameObject paw)
    {
        RectTransform rect = paw.GetComponent<RectTransform>();
        CanvasGroup cg = paw.GetComponent<CanvasGroup>();

        // Random rotation for variety
        float randomRotation = Random.Range(-rotationVariance, rotationVariance);
        rect.localRotation = Quaternion.Euler(0, 0, randomRotation);

        // Randomly flip horizontally for left/right paw variation
        if (Random.value > 0.5f)
            rect.localScale = new Vector3(-1, 1, 1);
        else
            rect.localScale = Vector3.one;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Scale up then slightly down (ease out)
            float scale = Mathf.Lerp(startScale, endScale, t);
            rect.localScale = new Vector3(
                rect.localScale.x > 0 ? scale : -scale,
                scale,
                1
            );

            // Fade out
            cg.alpha = 1f - t;

            yield return null;
        }

        ReturnPawToPool(paw);
    }

    // Public method to manually trigger paw effect
    public void TriggerPawAt(Vector2 screenPosition)
    {
        ShowPawEffect(screenPosition);
    }
}
