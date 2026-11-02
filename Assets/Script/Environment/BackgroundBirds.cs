using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class BackgroundBirds : MonoBehaviour
{
    [Header("Sky Decoration")]
    [SerializeField] private Camera skyCamera;
    [SerializeField] private Transform[] birds;
    [SerializeField] private Transform[] leftWings;
    [SerializeField] private Transform[] rightWings;
    [SerializeField] private Vector2 skyHeight = new Vector2(0.72f, 0.84f);
    [SerializeField] private Vector2 crossingSeconds = new Vector2(18f, 28f);
    [SerializeField] private Vector2 depthRange = new Vector2(60f, 85f);
    [SerializeField] private Vector2 repeatDelay = new Vector2(5f, 12f);
    [SerializeField] private float flapSpeed = 4f;
    [SerializeField] private float flapAngle = 32f;
    [SerializeField] private float bobHeight = 0.006f;

    private float[] horizontal;
    private float[] height;
    private float[] depth;
    private float[] speed;
    private float[] phase;
    private float[] delay;
    private float clock;

    private void Awake()
    {
        int count = birds.Length;
        horizontal = new float[count];
        height = new float[count];
        depth = new float[count];
        speed = new float[count];
        phase = new float[count];
        delay = new float[count];
        for (int i = 0; i < count; i++)
        {
            ResetBird(i);
            horizontal[i] = Mathf.Lerp(0.12f, 0.88f, (i + 0.5f) / count);
        }
    }

    private void ResetBird(int index)
    {
        speed[index] = (Random.value < 0.5f ? -1f : 1f) / Random.Range(crossingSeconds.x, crossingSeconds.y);
        horizontal[index] = speed[index] > 0f ? -0.08f : 1.08f;
        height[index] = Random.Range(skyHeight.x, skyHeight.y);
        depth[index] = Random.Range(depthRange.x, depthRange.y);
        phase[index] = Random.Range(0f, Mathf.PI * 2f);
    }

    private void LateUpdate()
    {
        if (skyCamera == null) return;
        clock += Time.deltaTime;
        for (int i = 0; i < birds.Length; i++)
        {
            if (birds[i] == null) continue;
            if (delay[i] > 0f)
            {
                delay[i] -= Time.deltaTime;
                if (delay[i] > 0f) continue;
                birds[i].gameObject.SetActive(true);
            }
            horizontal[i] += speed[i] * Time.deltaTime;
            if (horizontal[i] < -0.1f || horizontal[i] > 1.1f)
            {
                ResetBird(i);
                delay[i] = Random.Range(repeatDelay.x, repeatDelay.y);
                birds[i].gameObject.SetActive(false);
                continue;
            }
            float flap = Mathf.Sin(clock * flapSpeed + phase[i]);
            birds[i].position = skyCamera.ViewportToWorldPoint(new Vector3(horizontal[i], height[i] + Mathf.Sin(clock + phase[i]) * bobHeight, depth[i]));
            birds[i].rotation = skyCamera.transform.rotation;
            if (i < leftWings.Length && leftWings[i] != null)
                leftWings[i].localRotation = Quaternion.Euler(0f, 0f, -flap * flapAngle);
            if (i < rightWings.Length && rightWings[i] != null)
                rightWings[i].localRotation = Quaternion.Euler(0f, 0f, flap * flapAngle);
        }
    }
}
