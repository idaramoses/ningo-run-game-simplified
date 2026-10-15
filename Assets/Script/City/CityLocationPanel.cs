using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Home location panel: next/previous arrows cycle through cities and
/// swap the plaque sprite. Add all plaque Images that should stay in sync.
/// </summary>
public class CityLocationPanel : MonoBehaviour
{
    [SerializeField] private CityManager cityManager;
    [SerializeField] private Image[] plaqueImages;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [Tooltip("Optional: clicking the plaque itself cycles to the next city.")]
    [SerializeField] private Button plaqueButton;

    [Header("Locked Cities")]
    [Tooltip("Lock icon shown for locked and coming-soon cities.")]
    [SerializeField] private GameObject lockIcon;
    [Tooltip("Locked cities: lock icon only.")]
    [SerializeField] private string[] lockedCityNames = { "Abuja", "PortHarcourt" };

    [Header("Coming Soon Cities")]
    [Tooltip("Dim overlay with 'Coming Soon' label. The lock icon also shows for these cities.")]
    [SerializeField] private GameObject comingSoonOverlay;
    [SerializeField] private string[] comingSoonCityNames = { "Uyo", "Ibadan", "Kano", "Umuahia", "Onitsha" };

    private void Start()
    {
        if (cityManager == null) cityManager = CityManager.Instance;
        if (cityManager == null) cityManager = FindFirstObjectByType<CityManager>();

        // Only add runtime listeners when no persistent ones are wired -
        // otherwise a click would fire twice.
        if (nextButton != null && nextButton.onClick.GetPersistentEventCount() == 0)
            nextButton.onClick.AddListener(Next);
        if (previousButton != null && previousButton.onClick.GetPersistentEventCount() == 0)
            previousButton.onClick.AddListener(Previous);
        if (plaqueButton != null && plaqueButton.onClick.GetPersistentEventCount() == 0)
            plaqueButton.onClick.AddListener(Next);

        if (cityManager != null) cityManager.CityChanged += OnCityChanged;
        UpdatePlaques();
    }

    private void OnDestroy()
    {
        if (cityManager != null) cityManager.CityChanged -= OnCityChanged;
    }

    public void Next() { if (cityManager != null) cityManager.NextCity(); }
    public void Previous() { if (cityManager != null) cityManager.PreviousCity(); }

    private void OnCityChanged(int index) => UpdatePlaques();

    private void UpdatePlaques()
    {
        if (cityManager == null || plaqueImages == null) return;
        CityDefinition city = cityManager.GetCity(cityManager.CurrentIndex);
        if (city == null || city.plaqueSprite == null) return;
        foreach (Image img in plaqueImages)
        {
            if (img != null) img.sprite = city.plaqueSprite;
        }
        UpdateLockState(city.cityName);
    }

    private void UpdateLockState(string cityName)
    {
        bool locked = IsInList(cityName, lockedCityNames);
        bool comingSoon = IsInList(cityName, comingSoonCityNames);

        if (lockIcon != null) lockIcon.SetActive(locked || comingSoon);
        if (comingSoonOverlay != null) comingSoonOverlay.SetActive(comingSoon);
    }

    private static bool IsInList(string cityName, string[] list)
    {
        if (list == null) return false;
        foreach (string entry in list)
        {
            if (string.Equals(entry, cityName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
