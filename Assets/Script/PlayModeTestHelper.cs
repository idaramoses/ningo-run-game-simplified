using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayModeTestHelper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnPlayModeStart()
    {
#if UNITY_EDITOR
        string state = SessionState.GetString("PlayModeTest.State", "Idle");
        if (state == "EnteringPlayMode" || state == "InPlayMode")
        {
            SessionState.SetString("PlayModeTest.State", "InPlayMode");
            GameObject player = GameObject.Find("WikiCatPlayer");
            if (player != null)
            {
                player.AddComponent<PlayModeTestHelper>();
                Debug.Log("[PlayModeTest] Successfully attached PlayModeTestHelper via RuntimeInitializeOnLoad.");
            }
            else
            {
                Debug.LogWarning("[PlayModeTest] Could not find WikiCatPlayer to attach helper.");
            }
        }
#endif
    }

    private float timer = 0f;
    private Vector3 startPos;
    private GameObject spawnerObj;

    private void Start()
    {
        Debug.Log("[Test] Runtime Setup started.");
        startPos = transform.position;

        // Speed up the player for the test
        SimpleRunner runner = GetComponent<SimpleRunner>();
        if (runner != null)
        {
            runner.forwardSpeed = 35f;
            Debug.Log("[Test] Speed set to 35 for fast recycling.");
        }

        spawnerObj = GameObject.Find("[SimpleRoadSpawner]");
        if (spawnerObj != null)
        {
            Debug.Log("[Test] Spawner found. Children: " + spawnerObj.transform.childCount);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 5.0f)
        {
            // Test complete!
            float finalZ = transform.position.z;
            int childCount = spawnerObj != null ? spawnerObj.transform.childCount : 0;

            bool hasRecycled = false;
            if (spawnerObj != null)
            {
                foreach (Transform child in spawnerObj.transform)
                {
                    if (child.position.z >= 160f)
                    {
                        hasRecycled = true;
                        break;
                    }
                }
            }

            bool success = finalZ > 40f && childCount == 8 && hasRecycled;
            string error = null;
            if (!success)
            {
                if (finalZ <= 40f) error = "Player did not move far enough: Z=" + finalZ;
                else if (childCount != 8) error = "Expected 8 segments, got " + childCount;
                else if (!hasRecycled) error = "No segment was recycled ahead";
            }

            var result = new TestResult
            {
                success = success,
                error = error,
                finalDistanceZ = finalZ,
                activeSegmentCount = childCount,
                hasRecycledSegment = hasRecycled
            };

            #if UNITY_EDITOR
                        SessionState.SetString("PlayModeTest.Result", JsonUtility.ToJson(result));
                        SessionState.SetString("PlayModeTest.State", "Done");
            #endif

                        Debug.Log("[Test] Completed. Success: " + success + " FinalZ: " + finalZ + " Recycled: " + hasRecycled);

                        // Exit Play Mode
            #if UNITY_EDITOR
                        EditorApplication.isPlaying = false;
            #endif
                        Destroy(this);
        }
    }

    [System.Serializable]
    public class TestResult
    {
        public bool success;
        public string error;
        public float finalDistanceZ;
        public int activeSegmentCount;
        public bool hasRecycledSegment;
    }
}

