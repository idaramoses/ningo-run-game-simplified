using UnityEngine;

[System.Serializable]
public class CityRadioEntry
{
    public string id;
    [TextArea(2, 5)] public string transcript;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "CityRadioBank", menuName = "Game/City Radio Bank")]
public class CityRadioBank : ScriptableObject
{
    [SerializeField] private string _cityKey;
    [SerializeField] private string _stationLabel;
    [SerializeField] private CityRadioEntry[] _entries = new CityRadioEntry[0];

    public string CityKey => _cityKey;
    public string StationLabel => _stationLabel;
    public CityRadioEntry[] Entries => _entries;
}
