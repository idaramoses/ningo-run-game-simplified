using UnityEngine;
using System.Collections.Generic;

public class CoinRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 180f;
    
    [Tooltip("Rotation axis (default: Y-axis for spinning)")]
    public Vector3 rotationAxis = Vector3.up;
    
    private static readonly List<CoinRotation> activeRotations = new List<CoinRotation>(128);
    private static CoinRotationManager managerInstance;

    private void OnEnable()
    {
        activeRotations.Add(this);
        EnsureManagerExists();
    }

    private void OnDisable()
    {
        activeRotations.Remove(this);
    }

    private static void EnsureManagerExists()
    {
        if (managerInstance != null) return;

        managerInstance = Object.FindFirstObjectByType<CoinRotationManager>();
        if (managerInstance == null)
        {
            GameObject go = new GameObject("CoinRotationManager");
            managerInstance = go.AddComponent<CoinRotationManager>();
            Object.DontDestroyOnLoad(go);
        }
    }

    private class CoinRotationManager : MonoBehaviour
    {
        private void Update()
        {
            float dt = Time.deltaTime;
            int count = activeRotations.Count;
            for (int i = 0; i < count; i++)
            {
                CoinRotation coin = activeRotations[i];
                if (coin != null)
                {
                    coin.transform.Rotate(coin.rotationAxis, coin.rotationSpeed * dt, Space.World);
                }
            }
        }
    }
}
