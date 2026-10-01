using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Canvas_Level level-select map in the Home scene.
/// Lays out level nodes along a winding path inside a ScrollRect.
/// Level 1 sits at the top; higher levels wind downward.
/// </summary>
public class LevelMapCanvas : MonoBehaviour
{
    public static LevelMapCanvas Instance { get; private set; }

    [Header("Scrolling Path")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public LevelMapNode nodeTemplate;
    public RectTransform connectorContainer;

    [Header("Sprites")]
    public Sprite currentLevelSprite;
    public Sprite unlockedLevelSprite;
    public Sprite lockedLevelSprite;
    public Sprite padlockSprite;
    public Sprite fullStarSprite;
    public Sprite emptyStarSprite;
    public Sprite connectorSprite;

    [Header("Path Layout")]
    [Tooltip("0 = use LevelManager.GetMaxLevelCount()")]
    public int levelCount = 0;
    public float verticalSpacing = 200f;
    public float horizontalAmplitude = 150f;
    public float topPadding = 90f;
    public float bottomPadding = 160f;
    public int connectorDotCount = 3;
    public float connectorSize = 18f;

    private readonly List<LevelMapNode> nodes = new List<LevelMapNode>();
    private LevelMapNode currentNode;
    private int currentNodeLevel = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (LevelManager.Instance == null)
            new GameObject("LevelManager").AddComponent<LevelManager>();

        if (nodeTemplate == null || content == null || LevelManager.Instance == null)
            return;

        int total = levelCount > 0 ? levelCount : Mathf.Max(20, LevelManager.Instance.GetMaxLevelCount());
        total = Mathf.Clamp(total, 1, 100);

        EnsureNodeCount(total);

        float height = topPadding + (total - 1) * verticalSpacing + bottomPadding;
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);

        int highest = LevelManager.Instance.GetHighestUnlockedLevel();

        // The single "current" level = first unlocked level that hasn't been completed.
        int nextPlayable = -1;
        for (int i = 1; i <= total; i++)
        {
            LevelData d = LevelManager.Instance.GetLevel(i);
            bool unlocked = d.isUnlocked || i <= highest;
            if (unlocked && !d.isCompleted) { nextPlayable = i; break; }
        }
        if (nextPlayable < 0) nextPlayable = Mathf.Min(highest, total);

        Vector2[] positions = new Vector2[total];

        for (int i = 0; i < total; i++)
        {
            int levelNumber = i + 1;
            LevelData data = LevelManager.Instance.GetLevel(levelNumber);
            bool unlocked = data.isUnlocked || levelNumber <= highest;
            bool isCurrent = unlocked && levelNumber == nextPlayable;

            nodes[i].gameObject.SetActive(true);
            nodes[i].Configure(this, levelNumber, unlocked, data.isCompleted, data.starsEarned, isCurrent);
            if (isCurrent) { currentNode = nodes[i]; currentNodeLevel = levelNumber; }

            float x = Mathf.Sin((levelNumber - 1) * 1.05f) * horizontalAmplitude;
            float y = -topPadding - (levelNumber - 1) * verticalSpacing;
            positions[i] = new Vector2(x, y);
            nodes[i].RectTransform.anchoredPosition = positions[i];
        }

        BuildConnectors(positions);

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void BuildConnectors(Vector2[] positions)
    {
        if (connectorContainer == null || connectorSprite == null || connectorDotCount <= 0)
            return;

        for (int i = connectorContainer.childCount - 1; i >= 0; i--)
            Destroy(connectorContainer.GetChild(i).gameObject);

        for (int i = 0; i < positions.Length - 1; i++)
        {
            for (int d = 1; d <= connectorDotCount; d++)
            {
                float t = (float)d / (connectorDotCount + 1);
                Vector2 pos = Vector2.Lerp(positions[i], positions[i + 1], t);

                GameObject dot = new GameObject("Connector", typeof(RectTransform), typeof(Image));
                dot.transform.SetParent(connectorContainer, false);

                RectTransform rt = (RectTransform)dot.transform;
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = pos;
                rt.sizeDelta = new Vector2(connectorSize, connectorSize);

                Image img = dot.GetComponent<Image>();
                img.sprite = connectorSprite;
                img.preserveAspect = true;
                img.raycastTarget = false;
            }
        }
    }

    private void EnsureNodeCount(int total)
    {
        while (nodes.Count < total)
        {
            LevelMapNode node = Instantiate(nodeTemplate, content);
            nodes.Add(node);
        }
    }

    public void OnLevelNodeClicked(int levelNumber)
    {
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.StartLevelAndRun(levelNumber);
            return;
        }

        Debug.LogWarning("[LevelMapCanvas] UImanager not found - cannot start level");
    }

    /// <summary>Pulse the newly-unlocked "current" node, then auto-start that level.</summary>
    public void AnimateCurrentNodeThenStart(float pulseDuration = 0.9f)
    {
        if (currentNode == null) Refresh();
        if (currentNode == null) return;
        StartCoroutine(AnimateThenStartCoroutine(pulseDuration));
    }

    private System.Collections.IEnumerator AnimateThenStartCoroutine(float duration)
    {
        RectTransform rt = currentNode.RectTransform;
        float e = 0f;
        while (e < duration)
        {
            e += Time.unscaledDeltaTime;
            float s = 1f + Mathf.Sin((e / duration) * Mathf.PI * 2f) * 0.15f;
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;
        OnLevelNodeClicked(currentNodeLevel);
    }

    public void HidePanel()
    {
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.HideLevelCanvas();
            return;
        }

        gameObject.SetActive(false);
    }
}
