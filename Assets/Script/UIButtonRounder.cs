using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UIButtonRounder : MonoBehaviour
{
    [Range(0, 100)] public float cornerRadius = 25f;
    private Image img;

    void OnValidate()
    {
        if (img == null) img = GetComponent<Image>();
        img.material = new Material(Shader.Find("UI/Default"));
        img.material.SetFloat("_Radius", cornerRadius);
    }
}
