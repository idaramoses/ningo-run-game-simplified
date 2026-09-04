using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Generic selection modal controller for Language/Country/Region modals.
/// Auto-discovers UI elements from the modal hierarchy created by HomeUISetup.
/// </summary>
public class SelectionModalController : MonoBehaviour
{
    [Header("Modal References (auto-found if empty)")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button overlayButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Transform optionsContainer;

    [Header("Prefab & Data")]
    [Tooltip("Drag your OptionItem prefab here")]
    [SerializeField] private OptionItem optionPrefab;
    [SerializeField] private List<OptionData> optionDataList = new List<OptionData>();

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("PlayerPrefs Key")]
    [Tooltip("Key used to save the selected option (e.g. Settings_Language, Settings_Country)")]
    [SerializeField] private string playerPrefsKey;

    public event Action<int, string> OnConfirmed;

    private int selectedIndex = 0;
    private List<OptionItem> optionItems = new List<OptionItem>();
    private CanvasGroup canvasGroup;
    private RectTransform panelRectTransform;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        AutoDiscoverReferences();
        SetupListeners();
        BuildOptions();
        LoadSavedSelection();

        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            panelRectTransform = panel.GetComponent<RectTransform>();
        }
    }

    private void AutoDiscoverReferences()
    {
        if (panel == null)
        {
            var p = transform.Find("Panel");
            if (p != null) panel = p.gameObject;
        }

        if (closeButton == null && panel != null)
        {
            var c = panel.transform.Find("CloseButton");
            if (c != null) closeButton = c.GetComponent<Button>();
        }

        if (overlayButton == null)
        {
            var o = transform.Find("Overlay");
            if (o != null) overlayButton = o.GetComponent<Button>();
        }

        if (confirmButton == null && panel != null)
        {
            var c = panel.transform.Find("ConfirmButton");
            if (c != null) confirmButton = c.GetComponent<Button>();
        }

        if (optionsContainer == null && panel != null)
        {
            optionsContainer = panel.transform.Find("OptionsList");
        }

        // Derive default PlayerPrefs key from modal name
        if (string.IsNullOrEmpty(playerPrefsKey))
        {
            if (gameObject.name.Contains("Language"))
                playerPrefsKey = "Settings_Language";
            else if (gameObject.name.Contains("Country"))
                playerPrefsKey = "Settings_Country";
        }
    }

    private void BuildOptions()
    {
        if (optionsContainer == null || optionPrefab == null) return;

        // Clear existing
        optionItems.Clear();
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < optionDataList.Count; i++)
        {
            OptionData data = optionDataList[i];
            OptionItem item = Instantiate(optionPrefab, optionsContainer);
            item.name = $"Option_{i}_{data.label.Replace(" ", "")}";
            item.Setup(i, data.label, data.icon, OnOptionSelected);
            item.SetSelected(i == 0); // First one selected by default
            optionItems.Add(item);
        }
    }

    private void OnOptionSelected(int index)
    {
        SelectOption(index);
    }

    private void SetupListeners()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (overlayButton != null)
            overlayButton.onClick.AddListener(Hide);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
    }

    private void LoadSavedSelection()
    {
        if (string.IsNullOrEmpty(playerPrefsKey)) return;

        string saved = PlayerPrefs.GetString(playerPrefsKey, "");
        if (string.IsNullOrEmpty(saved)) return;

        for (int i = 0; i < optionDataList.Count; i++)
        {
            if (string.Equals(optionDataList[i].label, saved, StringComparison.OrdinalIgnoreCase))
            {
                SelectOption(i);
                return;
            }
        }
    }

    public void SelectOption(int index)
    {
        if (index < 0 || index >= optionItems.Count) return;
        selectedIndex = index;

        for (int i = 0; i < optionItems.Count; i++)
        {
            optionItems[i].SetSelected(i == index);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(BounceIn());
    }

    private IEnumerator BounceIn()
    {
        if (panelRectTransform == null || canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.interactable = false;
        panelRectTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            // Elastic bounce: overshoot then settle
            float scale = Mathf.Lerp(0f, 1.15f, t);
            if (t > 0.6f)
            {
                // Settle back from overshoot
                float settleT = (t - 0.6f) / 0.4f;
                scale = Mathf.Lerp(1.15f, 1f, settleT);
            }
            panelRectTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        panelRectTransform.localScale = Vector3.one;
        canvasGroup.interactable = true;
    }

    private IEnumerator ShrinkOut()
    {
        if (panelRectTransform == null || canvasGroup == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        canvasGroup.interactable = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (fadeDuration * 0.5f);
            float scale = 1f - t;
            panelRectTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        panelRectTransform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(ShrinkOut());
    }


    public void Confirm()
    {
        if (selectedIndex < 0 || selectedIndex >= optionDataList.Count)
        {
            Hide();
            return;
        }

        string chosen = optionDataList[selectedIndex].label;
        Debug.Log($"[Modal] Confirmed selection [{selectedIndex}]: {chosen}");

        if (!string.IsNullOrEmpty(playerPrefsKey))
        {
            PlayerPrefs.SetString(playerPrefsKey, chosen);
            PlayerPrefs.Save();
        }

        OnConfirmed?.Invoke(selectedIndex, chosen);
        Hide();
    }

    public string GetSelectedLabel()
    {
        if (selectedIndex < 0 || selectedIndex >= optionDataList.Count) return "";
        return optionDataList[selectedIndex].label;
    }
}
