using UnityEngine;
using System.Collections;

public class VehicleMover : MonoBehaviour
{
    public enum MoveDir { AgainstPlayer, WithPlayer }

    [Header("Direction")]
    public MoveDir direction = MoveDir.AgainstPlayer;

    [Header("Speed")]
    public float minSpeed = 6f;
    public float maxSpeed = 12f;
    public bool addPlayerSpeed = true; // makes it feel faster when player speed increases

    [Header("Optional Auto Destroy")]
    public Transform player;
    public float destroyBehindDistance = 40f; // if far behind player, destroy

    [Header("Pooling")]
    [Tooltip("If set, the vehicle returns to ObjectPoolManager with this tag instead of being destroyed.")]
    public string poolTag;

    [Header("Triggered Start")]
    [Tooltip("If true, the vehicle will not move until StartMoving() is called.")]
    public bool triggerStart = false;

    [Header("Headlight & Horn Alerts")]
    [Tooltip("Distance at which the vehicle will flash lights and honk horn")]
    [SerializeField] private float alertDistance = 25f;
    [SerializeField] private float flashInterval = 0.08f;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private AudioClip customHornSound;
    [SerializeField] private float hornVolume = 0.65f;

    private float mySpeed;
    private bool isMoving = true;
    private bool alertTriggered = false;
    private GameObject[] headlightObjects;
    private AudioSource hornAudioSource;

    void Start()
    {
        mySpeed = Random.Range(minSpeed, maxSpeed);

        // Auto find player if not set (AmakaModel)
        if (!player)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }

        // Cache child headlight objects under the "Headlights" container for high-performance toggling!
        Transform container = transform.Find("Headlights");
        if (container != null)
        {
            // Force activate the parent container itself so it's always enabled
            container.gameObject.SetActive(true);

            headlightObjects = new GameObject[container.childCount];
            for (int i = 0; i < container.childCount; i++)
            {
                headlightObjects[i] = container.GetChild(i).gameObject;
            }
        }

        // Setup spatial 3D audio source for realistic panning as the vehicle drives past!
        hornAudioSource = gameObject.AddComponent<AudioSource>();
        hornAudioSource.playOnAwake = false;
        hornAudioSource.loop = false;
        hornAudioSource.spatialBlend = 1.0f; // 100% 3D spatial sound!
        hornAudioSource.minDistance = 3f;
        hornAudioSource.maxDistance = 45f;
        hornAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Force headlights ON by default immediately when the vehicle spawns!
        SetHeadlightsState(true);

        // Ground-snapping once on spawn to align vehicle physically on the road or pavement
        Vector3 snapOrigin = transform.position + Vector3.up * 5f;
        RaycastHit hit;
        if (Physics.Raycast(snapOrigin, Vector3.down, out hit, 15f))
        {
            if (!hit.collider.isTrigger && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Obstacle"))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }

        isMoving = !triggerStart;
    }

    void OnEnable()
    {
        // Reset runtime state when this vehicle is pulled from the object pool.
        mySpeed = Random.Range(minSpeed, maxSpeed);
        alertTriggered = false;
        isMoving = !triggerStart;
        if (headlightObjects != null)
        {
            SetHeadlightsState(true);
        }
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public float GetSpeed()
    {
        return mySpeed;
    }

    public void SetCruiseSpeed(float speed)
    {
        mySpeed = speed;
        minSpeed = speed;
        maxSpeed = speed;
    }

    void Update()
    {
        // Only move when game is playing
        if (GameStateController.Instance == null || !GameStateController.Instance.IsPlaying())
            return;

        if (!isMoving)
            return;

        float speed = mySpeed;

        Vector3 dir = (direction == MoveDir.AgainstPlayer) ? Vector3.back : Vector3.forward;
        transform.position += dir * speed * Time.deltaTime;

        // Trigger warning alert when player gets close
        if (player != null && !alertTriggered)
        {
            float distanceZ = transform.position.z - player.position.z;
            // Only trigger if the vehicle is in front of the player and within threshold distance
            if (distanceZ > 0f && distanceZ < alertDistance)
            {
                TriggerVehicleAlert();
            }
        }

        // Optional cleanup (helps performance)
        if (player && (player.position.z - transform.position.z) > destroyBehindDistance)
        {
            if (!string.IsNullOrEmpty(poolTag) && ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void TriggerVehicleAlert()
    {
        alertTriggered = true;
        StartCoroutine(FlashAndHonkCoroutine());
    }

    private IEnumerator FlashAndHonkCoroutine()
    {
        // 1. Play Horn (spatialized) if a custom sound is assigned
        if (hornAudioSource != null && customHornSound != null)
        {
            hornAudioSource.clip = customHornSound;
            hornAudioSource.volume = hornVolume;
            hornAudioSource.Play();
        }

        // 2. High-speed flashing headlights alert!
        if (headlightObjects != null && headlightObjects.Length > 0)
        {
            for (int i = 0; i < flashCount; i++)
            {
                // Headlights OFF
                SetHeadlightsState(false);
                yield return new WaitForSeconds(flashInterval);

                // Headlights ON
                SetHeadlightsState(true);
                yield return new WaitForSeconds(flashInterval * 1.5f); // Keep on slightly longer for organic look
            }

            // Ensure they remain ON permanently after warning flash
            SetHeadlightsState(true);
        }
    }

    private void SetHeadlightsState(bool state)
    {
        if (headlightObjects != null)
        {
            foreach (var go in headlightObjects)
            {
                if (go != null)
                {
                    go.SetActive(state);
                }
            }
        }
    }
}
