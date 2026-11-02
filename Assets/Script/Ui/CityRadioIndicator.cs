using UnityEngine;

public class CityRadioIndicator : MonoBehaviour
{
    [SerializeField] private CityRadioController _radio;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private RectTransform[] _bars;
    [SerializeField] private float _animationSpeed = 7f;
    private string _lastLabel;

    private void OnEnable()
    {
        if (_radio != null) _radio.StatusChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (_radio != null) _radio.StatusChanged -= Refresh;
        if (_group != null) _group.alpha = 0f;
    }

    private void Refresh()
    {
        if (_group != null)
        {
            _group.alpha = _radio != null && _radio.IsRunning ? 1f : 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
        string label = _radio != null ? _radio.StationLabel : "CITY RADIO";
        if (_label != null && label != _lastLabel)
        {
            _lastLabel = label;
            _label.text = label;
        }
        if (_radio == null || !_radio.IsSpeaking) ResetBars();
    }

    private void Update()
    {
        if (_radio == null || !_radio.IsRunning || !_radio.IsSpeaking || _bars == null) return;
        for (int i = 0; i < _bars.Length; i++)
            if (_bars[i] != null)
                _bars[i].localScale = new Vector3(1f, .35f + .65f * Mathf.Abs(Mathf.Sin(Time.time * _animationSpeed + i * 1.3f)), 1f);
    }

    private void ResetBars()
    {
        if (_bars == null) return;
        for (int i = 0; i < _bars.Length; i++)
            if (_bars[i] != null) _bars[i].localScale = new Vector3(1f, .3f, 1f);
    }
}
