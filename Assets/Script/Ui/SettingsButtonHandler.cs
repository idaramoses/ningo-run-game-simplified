using UnityEngine;
using UnityEngine.UI;

public class SettingsButtonHandler : MonoBehaviour
{
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    
    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnSettingsButtonClicked);
        }
    }
    
    private void OnSettingsButtonClicked()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ShowSettings();
            Debug.Log("[SettingsButtonHandler] Settings button clicked");
        }
        else
        {
            Debug.LogWarning("[SettingsButtonHandler] GameFlowController.Instance not found!");
        }
    }
}
