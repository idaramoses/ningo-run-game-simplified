using UnityEngine;

public class EnvironmentSwayManager : MonoBehaviour
{
    [Header("Auto-Setup")]
    [Tooltip("Automatically add sway to all children with these tags")]
    [SerializeField] private string[] tagsToSway = new string[] { "Tree", "Billboard", "Prop" };
    
    [Header("Global Wind Settings")]
    [SerializeField] private bool useGlobalWind = true;
    [SerializeField] private float globalWindStrength = 1f;
    [SerializeField] private float globalWindSpeed = 0.5f;
    
    [Header("Auto-Add Scripts")]
    [SerializeField] private bool autoAddTreeSway = true;
    [SerializeField] private bool autoAddPropSway = true;
    
    private TreeSway[] allTrees;
    private PropSway[] allProps;

    void Start()
    {
        if (autoAddTreeSway || autoAddPropSway)
        {
            AutoSetupSway();
        }
        
        allTrees = GetComponentsInChildren<TreeSway>();
        allProps = GetComponentsInChildren<PropSway>();
    }

    void AutoSetupSway()
    {
        // Find all objects with specified tags
        foreach (string tag in tagsToSway)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            
            foreach (GameObject obj in objects)
            {
                if (tag == "Tree" && autoAddTreeSway)
                {
                    if (obj.GetComponent<TreeSway>() == null)
                    {
                        obj.AddComponent<TreeSway>();
                        Debug.Log($"Added TreeSway to {obj.name}");
                    }
                }
                else if (autoAddPropSway)
                {
                    if (obj.GetComponent<PropSway>() == null)
                    {
                        PropSway propSway = obj.AddComponent<PropSway>();
                        Debug.Log($"Added PropSway to {obj.name}");
                    }
                }
            }
        }
    }

    public void SetGlobalWindStrength(float strength)
    {
        globalWindStrength = strength;
    }

    public void SetGlobalWindSpeed(float speed)
    {
        globalWindSpeed = speed;
    }
}
