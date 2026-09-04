using UnityEngine;
using UnityEngine.UI;

public class InfoButtonHandler : MonoBehaviour
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
            button.onClick.AddListener(OnInfoButtonClicked);
        }
    }
    
    private void OnInfoButtonClicked()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ShowInfo();
            Debug.Log("[InfoButtonHandler] Info button clicked");
        }
        else
        {
            Debug.LogWarning("[InfoButtonHandler] GameFlowController.Instance not found!");
        }
    }
}
