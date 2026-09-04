using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRowItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image avatarImage;

    private void Awake()
    {
        if (rankText == null)
            rankText = transform.Find("Rank")?.GetComponent<TMP_Text>();
        if (playerNameText == null)
            playerNameText = transform.Find("Name")?.GetComponent<TMP_Text>();
        if (scoreText == null)
            scoreText = transform.Find("Score")?.GetComponent<TMP_Text>();
        if (avatarImage == null)
            avatarImage = transform.Find("Avatar")?.GetComponent<Image>();
    }

    public void Setup(int rank, string playerName, int score, Sprite avatar = null)
    {
        if (rankText != null) rankText.text = $"#{rank}";
        if (playerNameText != null) playerNameText.text = playerName;
        if (scoreText != null) scoreText.text = score.ToString("N0");
        if (avatarImage != null && avatar != null) avatarImage.sprite = avatar;
    }
}
