using UnityEngine;

public class RoadSideDecorator : MonoBehaviour
{
    public Transform leftBuildingAnchor;
    public Transform rightBuildingAnchor;
    public Transform leftDecorAnchor;
    public Transform rightDecorAnchor;
    public Transform roadCenter;

    public GameObject[] buildings;
    public GameObject[] trees;
    public GameObject[] flowers;
    public GameObject[] streetLights;

    public float minZ;
    public float maxZ;

    public float buildingChance;
    public float treeChance;
    public float flowerChance;
    public float lightChance;
}
