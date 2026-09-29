using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum LevelCardState
{
    Locked,
    UnlockedNotPlayed,
    OneStar,
    TwoStars,
    ThreeStars
}

public class LevelCard : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelNumberText;
    public Button cardButton;
    public Image cardImage; // Public for debugging

    [Header("Card State Sprites")]
    public Sprite lockedSprite;
    public Sprite zeroStarSprite;
    public Sprite oneStarSprite;
    public Sprite twoStarSprite;
    public Sprite threeStarSprite;

    [Header("Optional Effects")]
    public ParticleSystem sparkleEffect;
    public GameObject selectionHighlight;

    private LevelData levelData;
    private LevelCardState currentState;
    private bool isSelected = false;

    public void Initialize(LevelData level)
    {
        levelData = level;

        if (levelNumberText != null)
            levelNumberText.text = level.levelNumber.ToString();

        UpdateVisualState();

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
            cardButton.interactable = level.isUnlocked;
        }
    }

    public void UpdateVisualState()
    {
        if (levelData == null) return;

        if (!levelData.isUnlocked)
        {
            SetState(LevelCardState.Locked);
        }
        else if (levelData.starsEarned == 0)
        {
            SetState(LevelCardState.UnlockedNotPlayed);
        }
        else if (levelData.starsEarned == 1)
        {
            SetState(LevelCardState.OneStar);
        }
        else if (levelData.starsEarned == 2)
        {
            SetState(LevelCardState.TwoStars);
        }
        else if (levelData.starsEarned == 3)
        {
            SetState(LevelCardState.ThreeStars);
        }
    }

    private void SetState(LevelCardState state)
    {
        currentState = state;

        if (cardImage == null)
        {
            Debug.LogError($"[LevelCard] cardImage is NULL! Cannot set visual state.");
            return;
        }

        switch (state)
        {
            case LevelCardState.Locked:
                if (lockedSprite != null)
                {
                    cardImage.sprite = lockedSprite;
                }
                else
                {
                    Debug.LogWarning($"[LevelCard] lockedSprite is NULL! Using fallback color.");
                    cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
                }
                if (cardButton != null) cardButton.interactable = false;
                if (sparkleEffect != null) sparkleEffect.Stop();
                break;

            case LevelCardState.UnlockedNotPlayed:
                if (zeroStarSprite != null)
                {
                    cardImage.sprite = zeroStarSprite;
                }
                else
                {
                    Debug.LogWarning($"[LevelCard] zeroStarSprite is NULL! Using fallback color.");
                    cardImage.color = new Color(0.3f, 0.5f, 0.8f, 1f); // Blue
                }
                if (cardButton != null) cardButton.interactable = true;
                if (sparkleEffect != null) sparkleEffect.Stop();
                break;

            case LevelCardState.OneStar:
                if (oneStarSprite != null)
                {
                    cardImage.sprite = oneStarSprite;
                }
                else
                {
                    Debug.LogWarning($"[LevelCard] oneStarSprite is NULL! Using fallback color.");
                    cardImage.color = new Color(0.8f, 0.6f, 0.2f, 1f); // Bronze
                }
                if (cardButton != null) cardButton.interactable = true;
                if (sparkleEffect != null) sparkleEffect.Stop();
                break;

            case LevelCardState.TwoStars:
                if (twoStarSprite != null)
                {
                    cardImage.sprite = twoStarSprite;
                }
                else
                {
                    Debug.LogWarning($"[LevelCard] twoStarSprite is NULL! Using fallback color.");
                    cardImage.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Silver
                }
                if (cardButton != null) cardButton.interactable = true;
                if (sparkleEffect != null) sparkleEffect.Stop();
                break;

            case LevelCardState.ThreeStars:
                if (threeStarSprite != null)
                {
                    cardImage.sprite = threeStarSprite;
                }
                else
                {
                    Debug.LogWarning($"[LevelCard] threeStarSprite is NULL! Using fallback color.");
                    cardImage.color = new Color(1f, 0.84f, 0f, 1f); // Gold
                }
                if (cardButton != null) cardButton.interactable = true;
                if (sparkleEffect != null) sparkleEffect.Play();
                break;
        }

        UpdateSelectionHighlight();
        Debug.Log($"[LevelCard] Set to {state} state");
    }

    private void UpdateSelectionHighlight()
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSelectionHighlight();
    }

    private void OnCardClicked()
    {
        if (levelData == null || !levelData.isUnlocked) return;

        if (LevelSelectionPanel.Instance != null)
        {
            LevelSelectionPanel.Instance.OnLevelSelected(levelData.levelNumber);
        }

        Debug.Log($"[LevelCard] Selected Level {levelData.levelNumber} - Words: {levelData.GetWordCount()}");
    }

    public LevelData GetLevelData()
    {
        return levelData;
    }

    public int GetLevelNumber()
    {
        return levelData != null ? levelData.levelNumber : 0;
    }
}
