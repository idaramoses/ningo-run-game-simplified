using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Canvas_Level level-select map in the Home scene.
/// Lays out level nodes along a winding path inside a ScrollRect.
/// Level 1 sits at the bottom; higher levels wind upward, like the reference map.
/// </summary>
public class LevelMapCanvas : MonoBehaviour
{
    public static LevelMapCanvas Instance { get; private set; }

    [Header("Scrolling Path")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public LevelMapNode nodeTemplate;

    [Header("Sprites")]
    public Sprite currentLevelSprite;
    public Sprite unlockedLevelSprite;
    public Sprite lockedLevelSprite;
    public Sprite padlockSprite;
    public Sprite fullStarSprite;
    public Sprite emptyStarSprite;

    [Header("Path Layout")]
    [Tooltip("0 = use LevelManager.GetMaxLevelCount()")]
    public int levelCount = 0;
    public float verticalSpacing = 200f;
    public float horizontalAmplitude = 150f;
    public float topPadding = 90f;
    public float bottomPadding = 160f;

    private readonly List<LevelMapNode> nodes = new List<LevelMapNode>();

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

        for (int i = 0; i < total; i++)
        {
            int levelNumber = i + 1;
            LevelData data = LevelManager.Instance.GetLevel(levelNumber);
            bool unlocked = data.isUnlocked || levelNumber <= highest;

            nodes[i].gameObject.SetActive(true);
            nodes[i].Configure(this, levelNumber, unlocked, data.isCompleted, data.starsEarned);

            float x = Mathf.Sin((levelNumber - 1) * 1.05f) * horizontalAmplitude;
            float y = -(height - bottomPadding - (levelNumber - 1) * verticalSpacing);
            nodes[i].RectTransform.anchoredPosition = new Vector2(x, y);
        }

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
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
