using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps an Image's sprite in sync with the selected city's header/badge
/// sprite (used by the CityPlaque on Canvas_Level, which starts inactive -
/// so subscription happens in OnEnable rather than Start).
/// </summary>
[RequireComponent(typeof(Image))]
public class CityHeaderImage : MonoBehaviour
{
    [SerializeField] private CityManager cityManager;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (cityManager == null) cityManager = CityManager.Instance;
        if (cityManager == null) cityManager = FindFirstObjectByType<CityManager>(FindObjectsInactive.Include);
        if (cityManager != null) cityManager.CityChanged += OnCityChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (cityManager != null) cityManager.CityChanged -= OnCityChanged;
    }

    private void OnCityChanged(int index) => Refresh();

    private void Refresh()
    {
        if (image == null || cityManager == null) return;
        CityDefinition city = cityManager.GetCity(cityManager.CurrentIndex);
        if (city != null && city.headerSprite != null) image.sprite = city.headerSprite;
    }
}
