using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LanguageSelectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform languageButtonsContainer;
    [SerializeField] private GameObject languageButtonPrefab;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;

    private LoginForm loginForm;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private string selectedLanguageCode;
    private GameObject selectedButton;
    private LoginForm.LanguageItem[] languages;

    private void Awake()
    {
        loginForm = FindObjectOfType<LoginForm>();
        
        if (titleText != null)
        {
            titleText.text = "Choose Your Language";
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            nextButton.interactable = false; // Disable until language is selected
        }
    }

    public void SetLanguages(LoginForm.LanguageItem[] languages)
    {
        Debug.Log($"[LanguageSelectionUI] SetLanguages called with {languages?.Length ?? 0} languages");
        
        // Store languages for highlighting logic
        this.languages = languages;
        
        ClearButtons();

        if (languageButtonsContainer == null)
        {
            Debug.LogError("[LanguageSelectionUI] Language buttons container is not assigned!");
            return;
        }

        if (languageButtonPrefab == null)
        {
            Debug.LogError("[LanguageSelectionUI] Language button prefab is not assigned!");
            return;
        }

        Debug.Log($"[LanguageSelectionUI] Creating {languages.Length} language buttons...");

        foreach (var lang in languages)
        {
            GameObject buttonObj = Instantiate(languageButtonPrefab, languageButtonsContainer);
            spawnedButtons.Add(buttonObj);

            var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                string displayText = string.IsNullOrEmpty(lang.native_name) ? lang.language : lang.native_name;
                buttonText.text = displayText;
                Debug.Log($"[LanguageSelectionUI] Created button for: {displayText}");
            }
            else
            {
                Debug.LogWarning($"[LanguageSelectionUI] Button prefab has no TMP_Text component for language: {lang.language}");
            }

            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                string languageCode = lang.language;
                button.onClick.AddListener(() => OnLanguageButtonClicked(languageCode));
            }
            else
            {
                Debug.LogWarning($"[LanguageSelectionUI] Button prefab has no Button component for language: {lang.language}");
            }
        }

        Debug.Log($"[LanguageSelectionUI] Successfully created {spawnedButtons.Count} language buttons");
    }

    private void OnLanguageButtonClicked(string languageCode)
    {
        // Store selected language and highlight button
        selectedLanguageCode = languageCode;
        
        // Remove highlight from previously selected button
        if (selectedButton != null)
        {
            var image = selectedButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white; // Reset to default
            }
        }
        
        // Find and highlight the clicked button
        foreach (var btn in spawnedButtons)
        {
            var buttonText = btn.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                var langData = languages.FirstOrDefault(l => 
                    (l.native_name != null && buttonText.text == l.native_name) || 
                    buttonText.text == l.language);
                
                if (langData != null && langData.language == languageCode)
                {
                    selectedButton = btn;
                    var image = btn.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = new Color(0.8f, 0.9f, 1f); // Highlight color
                    }
                    break;
                }
            }
        }
        
        // Enable next button
        if (nextButton != null)
        {
            nextButton.interactable = true;
        }
        
        Debug.Log($"[LanguageSelectionUI] Language selected: {languageCode} (waiting for Next button)");
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("[LanguageSelectionUI] Back button clicked - returning to login");
        
        SplashController.Instance.ShowLogin();
        
        // Restore loginFormPanel visibility inside the login view
        if (loginForm != null)
        {
            loginForm.ShowLoginPanel();
        }
    }

    private void OnNextButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedLanguageCode))
        {
            Debug.LogWarning("[LanguageSelectionUI] No language selected");
            return;
        }
        
        Debug.Log($"[LanguageSelectionUI] Next button clicked - proceeding with: {selectedLanguageCode}");
        
        // Proceed with language selection
        if (loginForm != null)
        {
            loginForm.OnLanguageSelected(selectedLanguageCode);
        }
    }

    private void ClearButtons()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null)
            {
                Destroy(btn);
            }
        }
        spawnedButtons.Clear();
    }
}
