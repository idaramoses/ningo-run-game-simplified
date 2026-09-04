using UnityEngine;

[CreateAssetMenu(fileName = "RunnerData", menuName = "Game/Runner Data")]
public class RunnerData : ScriptableObject
{
    [Header("Runner Info")]
    public string runnerName;
    public GameObject runnerPrefab;
    public RuntimeAnimatorController animatorController;
    
    [Header("Stats")]
    [Range(1, 5)] public int speed = 2;
    [Range(1, 5)] public int control = 2;
    [Range(1, 5)] public int focus = 2;
    
    [Header("Unlock Requirements")]
    public UnlockType unlockType;
    public int unlockCost;
    public int unlockLevel;
    public bool isUnlockedByDefault;
    
    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
    public string role;
}

public enum UnlockType
{
    Free,
    Coins,
    Gems,
    Level,
    RewardedAd,
    Special
}
