using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One level button on the winding level-map path (Canvas_Level).
/// Shows the level number, earned stars, or a padlock when locked.
/// </summary>
public class LevelMapNode : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public Image background;
    public TMP_Text numberText;
    public Image padlockIcon;
    public Image[] stars;

    private int levelNumber;
    private LevelMapCanvas owner;

    public RectTransform RectTransform
    {
        get { return transform as RectTransform; }
    }

    public void Configure(LevelMapCanvas canvas, int level, bool unlocked, bool completed, int starsEarned, bool isCurrent)
    {
        owner = canvas;
        levelNumber = level;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = unlocked;
            if (unlocked)
                button.onClick.AddListener(OnClicked);
        }

        if (numberText != null)
        {
            numberText.text = level.ToString();
            numberText.gameObject.SetActive(true);
        }

        if (background != null)
        {
            if (!unlocked && canvas.lockedLevelSprite != null)
                background.sprite = canvas.lockedLevelSprite;
            else if (isCurrent && canvas.currentLevelSprite != null)
                background.sprite = canvas.currentLevelSprite;
            else if (canvas.unlockedLevelSprite != null)
                background.sprite = canvas.unlockedLevelSprite;
        }

        if (padlockIcon != null)
        {
            padlockIcon.gameObject.SetActive(!unlocked);
            if (!unlocked && canvas.padlockSprite != null)
                padlockIcon.sprite = canvas.padlockSprite;
        }

        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null) continue;

                stars[i].gameObject.SetActive(unlocked);

                bool filled = unlocked && completed && i < starsEarned;
                if (filled && canvas.fullStarSprite != null)
                    stars[i].sprite = canvas.fullStarSprite;
                else if (canvas.emptyStarSprite != null)
                    stars[i].sprite = canvas.emptyStarSprite;
            }
        }
    }

    private void OnClicked()
    {
        if (owner != null)
            owner.OnLevelNodeClicked(levelNumber);
    }
}
