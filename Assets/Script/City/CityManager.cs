using UnityEngine;

[System.Serializable]
public class CityDefinition
{
    public string cityName;
    [Tooltip("Plaque sprite shown on the Home location panel.")]
    public Sprite plaqueSprite;
    [Tooltip("Starter road group activated for this city.")]
    public GameObject starterRoadGroup;
    [Tooltip("Road tiles spawned for this city. Leave empty to reuse the spawner's current tiles.")]
    public GameObject[] roadTiles;
}

/// <summary>
/// Tracks which city the player has selected and applies it:
/// activates that city's starter-road group and swaps the road tiles
/// the RoadSpawner uses. Selection persists across sessions.
/// </summary>
public class CityManager : MonoBehaviour
{
    public const string SelectedCityKey = "SelectedCity";

    [SerializeField] private CityDefinition[] cities;
    [SerializeField] private RoadSpawner roadSpawner;

    public static CityManager Instance { get; private set; }

    public int CurrentIndex { get; private set; }
    public string CurrentCityName => cities != null && cities.Length > 0 ? cities[CurrentIndex].cityName : "";
    public event System.Action<int> CityChanged;

    private void Awake()
    {
        Instance = this;
        if (roadSpawner == null) roadSpawner = FindFirstObjectByType<RoadSpawner>();

        int index = 0;
        string saved = PlayerPrefs.GetString(SelectedCityKey, "");
        for (int i = 0; i < cities.Length; i++)
        {
            if (cities[i].cityName == saved) { index = i; break; }
        }
        ApplyCity(index, false);
    }

    public void NextCity() => SetCity(CurrentIndex + 1);
    public void PreviousCity() => SetCity(CurrentIndex - 1);

    public void SetCity(int index)
    {
        if (cities == null || cities.Length == 0) return;
        index = (index % cities.Length + cities.Length) % cities.Length;
        ApplyCity(index, true);
    }

    public void SetCity(string cityName)
    {
        for (int i = 0; i < cities.Length; i++)
        {
            if (cities[i].cityName == cityName) { SetCity(i); return; }
        }
    }

    public CityDefinition GetCity(int index)
    {
        if (cities == null || index < 0 || index >= cities.Length) return null;
        return cities[index];
    }

    private void ApplyCity(int index, bool notify)
    {
        CurrentIndex = index;

        // Activate the new group first so ground colliders never disappear
        // under the player standing on the starter road.
        for (int i = 0; i < cities.Length; i++)
        {
            GameObject group = cities[i].starterRoadGroup;
            if (group != null && i == index) group.SetActive(true);
        }
        for (int i = 0; i < cities.Length; i++)
        {
            GameObject group = cities[i].starterRoadGroup;
            if (group != null && i != index) group.SetActive(false);
        }

        if (roadSpawner != null) roadSpawner.SetRoadTiles(cities[index].roadTiles);

        if (notify)
        {
            PlayerPrefs.SetString(SelectedCityKey, cities[index].cityName);
            PlayerPrefs.Save();
            Debug.Log($"[CityManager] Selected city: {cities[index].cityName}");
        }
        CityChanged?.Invoke(index);
    }
}
