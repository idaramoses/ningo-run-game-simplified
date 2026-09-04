using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCityModal : MonoBehaviour
{
    [Header("Modal UI")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cancelButton;

    [Header("City Buttons")]
    [SerializeField] private Button lagosButton;
    [SerializeField] private Button abujaButton;
    [SerializeField] private Button uyoButton;
    [SerializeField] private Button portHarcourtButton;
    [SerializeField] private Button accraButton;

    [Header("City Images (Optional)")]
    [SerializeField] private Image lagosImage;
    [SerializeField] private Image abujaImage;
    [SerializeField] private Image uyoImage;
    [SerializeField] private Image portHarcourtImage;
    [SerializeField] private Image accraImage;

    private string selectedCity = "Lagos";
    private const string SELECTED_CITY_KEY = "SelectedCity";

    private void Start()
    {
        // Load saved city preference
        selectedCity = PlayerPrefs.GetString(SELECTED_CITY_KEY, "Lagos");

        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseModal);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseModal);

        if (lagosButton != null)
            lagosButton.onClick.AddListener(() => SelectCity("Lagos"));
        
        if (abujaButton != null)
            abujaButton.onClick.AddListener(() => SelectCity("Abuja"));
        
        if (uyoButton != null)
            uyoButton.onClick.AddListener(() => SelectCity("Uyo"));
        
        if (portHarcourtButton != null)
            portHarcourtButton.onClick.AddListener(() => SelectCity("Port Harcourt"));
        
        if (accraButton != null)
            accraButton.onClick.AddListener(() => SelectCity("Accra"));

        // Hide modal initially
        if (modalPanel != null)
            modalPanel.SetActive(false);

        UpdateButtonStates();
    }

    public void OpenModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(true);
            UpdateButtonStates();
            Debug.Log($"[ChangeCityModal] Opened modal. Current city: {selectedCity}");
        }
    }

    public void CloseModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
            Debug.Log("[ChangeCityModal] Closed modal");
        }
    }

    private void SelectCity(string cityName)
    {
        selectedCity = cityName;
        
        // Save selection
        PlayerPrefs.SetString(SELECTED_CITY_KEY, cityName);
        PlayerPrefs.Save();

        Debug.Log($"[ChangeCityModal] Selected city: {cityName}");
        Debug.LogWarning("[ChangeCityModal] City switching is currently disabled - RoadSpawner uses simple prefab array");

        UpdateButtonStates();
        CloseModal();
    }

    private void UpdateButtonStates()
    {
        // Highlight selected city button
        UpdateButtonVisual(lagosButton, "Lagos");
        UpdateButtonVisual(abujaButton, "Abuja");
        UpdateButtonVisual(uyoButton, "Uyo");
        UpdateButtonVisual(portHarcourtButton, "Port Harcourt");
        UpdateButtonVisual(accraButton, "Accra");
    }

    private void UpdateButtonVisual(Button button, string cityName)
    {
        if (button == null) return;

        // Change button appearance based on selection
        ColorBlock colors = button.colors;
        
        if (cityName == selectedCity)
        {
            // Selected state - brighter/highlighted
            colors.normalColor = new Color(0.3f, 0.8f, 0.3f); // Green tint
        }
        else
        {
            // Normal state
            colors.normalColor = Color.white;
        }
        
        button.colors = colors;
    }

    public string GetSelectedCity()
    {
        return selectedCity;
    }

    public static string GetSavedCity()
    {
        return PlayerPrefs.GetString(SELECTED_CITY_KEY, "Lagos");
    }
}
