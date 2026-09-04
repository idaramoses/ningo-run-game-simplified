using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
// Main world generator.
// Simple mental model:
//   1) starterRoad      - one road piece already placed in the scene where the player begins.
//   2) roadTilePrefabs  - prefab pieces that are cloned endlessly to make the road ahead.
//   3) obstaclePrefabs  - cars, trucks, barriers, etc. placed on the road.
//   4) item prefabs     - coins and powerups.
//   5) Optional special sections (waterfall, tunnel, bonus) can be left empty.
public class Makesupway : MonoBehaviour {

    // tạo map
    [Header("Spawned Road Tile Prefabs")]
    [Tooltip("Prefabs that are cloned endlessly to build the road ahead of the player. Index 0 = base road, 1..n = visual variants.")]
    public GameObject[] roadTilePrefabs;

    // Legacy single fields - kept for migration only; assign via roadTilePrefabs[]
    [HideInInspector, System.Obsolete("Use roadTilePrefabs array instead")]
    public GameObject baseRoadPrefab; // đường ray -> base road tile
    [HideInInspector, System.Obsolete("Use roadTilePrefabs array instead")]
    public GameObject roadVariant1Prefab;
    [HideInInspector, System.Obsolete("Use roadTilePrefabs array instead")]
    public GameObject roadVariant2Prefab;
    [HideInInspector, System.Obsolete("Use roadTilePrefabs array instead")]
    public GameObject roadVariant3Prefab;
    enum stylecreate { left, betten, righ }
    stylecreate newstyle;
    // tạo vật cản
    [Header("Obstacle Prefabs")]
    [Tooltip("Indices: 0=carVariantA, 1=carVariantB, 2=oncomingCar, 3=truck, 4=lowBarrier, 5=highBarrier, 6=extraHighBarrier, 7=overpassMid, 8=overpassLegLeft, 9=overpassLegRight, 10=overpassLegBoth, 11=streetLight")]
    public GameObject[] obstaclePrefabs;

    // Legacy single fields - kept for migration only; assign via obstaclePrefabs[]
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[0] instead")]
    public GameObject carObstaclePrefabA; // toa tàu -> parked/stalled car variant A
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[1] instead")]
    public GameObject carObstaclePrefabB; // toa tàu -> parked/stalled car variant B
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[2] instead")]
    public GameObject oncomingCarPrefab; // toa tàu chạy lao về player -> oncoming car rushing at the player
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[3] instead")]
    public GameObject truckObstaclePrefab; // toa tàu có cát -> truck obstacle
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[4] instead")]
    public GameObject lowBarrierPrefab; // thanh chắn thấp
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[5] instead")]
    public GameObject highBarrierPrefab; // thanh chắn cao
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[6] instead")]
    public GameObject extraHighBarrierPrefab; // thanh chắn lớn
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[7] instead")]
    public GameObject overpassMidPrefab; // cầu chân giữa -> overpass, middle support
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[8] instead")]
    public GameObject overpassLegLeftPrefab; // cầu chân phải -> overpass, left support
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[9] instead")]
    public GameObject overpassLegRightPrefab; // cầu chân trái -> overpass, right support
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[10] instead")]
    public GameObject overpassLegBothPrefab; // cầu chân 2 bên -> overpass, both supports
    [HideInInspector, System.Obsolete("Use obstaclePrefabs[11] instead")]
    public GameObject streetLightPrefab; // cột điện -> street light / road sign pole
    Vector3 locationemty = new Vector3(0, 0, 0); // lưu vị trí tạo vật cản
    [Header("Item/Powerup Prefabs")]
    public GameObject coinPrefab; //  coin chính
    public GameObject magnetCoinPrefab; //  hút coin (magnet powerup)
    public GameObject jetpackShortPrefab; //  bay 1 đoạn (short flight powerup)
    public GameObject jetpackLongPrefab; //  bay 1 đoạn dài (long flight powerup)
    public GameObject springShoesPrefab; // giày nhảy cao (jump boost powerup)
    public GameObject coinX2Prefab; // x2  coin
    public GameObject hoverboardPrefab; // ván trượt
    public GameObject mysteryBoxPrefab; // hôp quà
    public GameObject keyPrefab; // chìa khóa
    GameObject bettwen;
    int randummap;
    int randumemty;
    Vector3 location = new Vector3(0,0,0);
    enum stylecreatecoin { line, backtolef, backtoright, up } 
    enum stylecreatecoinposisition { left, bettwen, right, leftup, betwenup, rightup ,foritemfle} 
    public enum createitemposition { lef, righ, betten, lefup, righup, bettenup, jumpleft, jupmrifht, jumpbetten, jumpupleft, jumpupright, jumpupbetten }
    // Active spawned road tile instances (created from roadTilePrefabs).
    // map2/map21/map22 are simply 3 recycled groups of spawned road tiles.
    List<GameObject> map = new List<GameObject>();
    List<GameObject> map1 = new List<GameObject>();
    List<GameObject> map2 = new List<GameObject>();
    List<GameObject> map21 = new List<GameObject>();
    List<GameObject> map22 = new List<GameObject>();
    List<GameObject> supwaylist = new List<GameObject>();
    // lưu từng đoạn vật cản
    private List<GameObject> mapemty1 = new List<GameObject>();
    private List<GameObject> mapemty2 = new List<GameObject>();
    private List<GameObject> mapemty3 = new List<GameObject>();
    private List<GameObject> mapemty4 = new List<GameObject>();
    private List<GameObject> mapemty5 = new List<GameObject>();
    private List<GameObject> mapemty6 = new List<GameObject>();
    private List<GameObject> mapemty7 = new List<GameObject>();
    private List<GameObject> mapemty8 = new List<GameObject>();
    private List<GameObject> mapemty9 = new List<GameObject>();
    private List<GameObject> mapemty10 = new List<GameObject>();
    private List<GameObject> mapemty11 = new List<GameObject>();
    private List<GameObject> mapemty12 = new List<GameObject>();
    private List<GameObject> mapemty13 = new List<GameObject>();
    private List<GameObject> mapemty14 = new List<GameObject>();
    private List<GameObject> mapemty15 = new List<GameObject>();
    private List<GameObject> mapemty16 = new List<GameObject>();
    private List<GameObject> mapemty17 = new List<GameObject>();
    private List<GameObject> mapemty18 = new List<GameObject>();
    private List<GameObject> mapemty19 = new List<GameObject>();
    private List<GameObject> mapemty20 = new List<GameObject>();
    public static Makesupway makemap;
    public GameObject MapHowToPlay;
    [Header("Starter Road (scene object already in the scene)")]
    [Tooltip("The road piece already placed in the scene where the player starts running.")]
    [FormerlySerializedAs("roadSection1")]
    public GameObject starterRoad;

    [Header("Optional Special Sections (can be left empty)")]
    [Tooltip("Optional second starter section. Leave empty to use only starterRoad + spawned prefabs.")]
    public GameObject roadSection2;
    [Tooltip("Optional waterfall area. Leave empty if not used.")]
    public GameObject waterfallSection;
    [Tooltip("Optional tunnel/cave section. Leave empty if not used.")]
    public GameObject tunnelSection;
    [Tooltip("Optional bonus/branch section. Leave empty if not used.")]
    public GameObject bonusSection;
    // tạo map randum
    int valueofmap;
  public static  bool isnewgame = false; // kieemr tra laanf chowi dâud tieen
    // Use this for initialization
    void Start () {
        //  Time.timeScale = 0.2f;

        makemap = this;
        valueofmap = 0;
        MigrateLegacyPrefabs();
        InitializeObjectPools();
        Getthac();
        StartCoroutine(CheckshowGameOject(0));
        checkshowx = 100;
        AlowlcreatecoinforFly = true;
        if (PlayerPrefs.HasKey("hd") == false)
        {
            isnewgame = true;
            if (isnewgame && MapHowToPlay != null)
            {
                MapHowToPlay.SetActive(true);
            }

        }
    }
    float distinct = 0;
    float distinctplayer = 30;
    public GameObject createdow;

    #region Prefab Helpers
#if UNITY_EDITOR
    void OnValidate()
    {
        MigrateLegacyPrefabs();
    }
#endif

    /// <summary>
    /// Copies values from the legacy single fields into the new arrays.
    /// Run this in the editor or at start so existing inspector assignments aren't lost.
    /// </summary>
    private void MigrateLegacyPrefabs()
    {
        // Only copy from legacy single fields once, when the array is still empty.
        // After that the array can be any size you want in the Inspector.
        if (roadTilePrefabs == null || roadTilePrefabs.Length == 0)
        {
            roadTilePrefabs = new GameObject[]
            {
                baseRoadPrefab,
                roadVariant1Prefab,
                roadVariant2Prefab,
                roadVariant3Prefab
            };
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            obstaclePrefabs = new GameObject[]
            {
                carObstaclePrefabA,
                carObstaclePrefabB,
                oncomingCarPrefab,
                truckObstaclePrefab,
                lowBarrierPrefab,
                highBarrierPrefab,
                extraHighBarrierPrefab,
                overpassMidPrefab,
                overpassLegLeftPrefab,
                overpassLegRightPrefab,
                overpassLegBothPrefab,
                streetLightPrefab
            };
        }
    }

    private GameObject GetRoadTilePrefab(int index)
    {
        if (roadTilePrefabs != null && index >= 0 && index < roadTilePrefabs.Length && roadTilePrefabs[index] != null)
            return roadTilePrefabs[index];

        // Legacy fallbacks
        switch (index)
        {
            case 0: return baseRoadPrefab;
            case 1: return roadVariant1Prefab;
            case 2: return roadVariant2Prefab;
            case 3: return roadVariant3Prefab;
            default: return null;
        }
    }

    private GameObject GetObstaclePrefab(string name)
    {
        if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
        {
            switch (name)
            {
                case "makeemtyshipdie":
                    GameObject carVariant = (carVariantSwitchCounter <= 10)
                        ? (obstaclePrefabs.Length > 0 ? obstaclePrefabs[0] : null)
                        : (obstaclePrefabs.Length > 1 ? obstaclePrefabs[1] : null);
                    return carVariant ?? carObstaclePrefabA ?? carObstaclePrefabB;
                case "makeemtyshipdiemuving": return obstaclePrefabs.Length > 2 ? obstaclePrefabs[2] : oncomingCarPrefab;
                case "makeemtyship": return obstaclePrefabs.Length > 3 ? obstaclePrefabs[3] : truckObstaclePrefab;
                case "makeembars_smore": return obstaclePrefabs.Length > 4 ? obstaclePrefabs[4] : lowBarrierPrefab;
                case "makeembars_big": return obstaclePrefabs.Length > 5 ? obstaclePrefabs[5] : highBarrierPrefab;
                case "makeembars_biger": return obstaclePrefabs.Length > 6 ? obstaclePrefabs[6] : extraHighBarrierPrefab;
                case "makeembridge": return obstaclePrefabs.Length > 7 ? obstaclePrefabs[7] : overpassMidPrefab;
                case "makeembridgeleft": return obstaclePrefabs.Length > 8 ? obstaclePrefabs[8] : overpassLegLeftPrefab;
                case "makeembridgerigh": return obstaclePrefabs.Length > 9 ? obstaclePrefabs[9] : overpassLegRightPrefab;
                case "makeembridgebehig": return obstaclePrefabs.Length > 10 ? obstaclePrefabs[10] : overpassLegBothPrefab;
                case "powerpoles": return obstaclePrefabs.Length > 11 ? obstaclePrefabs[11] : streetLightPrefab;
            }
        }

        // Legacy fallbacks
        switch (name)
        {
            case "makeemtyshipdie": return (carVariantSwitchCounter <= 10 ? carObstaclePrefabA : carObstaclePrefabB);
            case "makeemtyship": return truckObstaclePrefab;
            case "makeembars_smore": return lowBarrierPrefab;
            case "makeembars_big": return highBarrierPrefab;
            case "makeembars_biger": return extraHighBarrierPrefab;
            case "makeembridge": return overpassMidPrefab;
            case "makeembridgeleft": return overpassLegLeftPrefab;
            case "makeembridgerigh": return overpassLegRightPrefab;
            case "makeembridgebehig": return overpassLegBothPrefab;
            case "powerpoles": return streetLightPrefab;
            case "makeemtyshipdiemuving": return oncomingCarPrefab;
            default: return null;
        }
    }

    private void InitializeObjectPools()
    {
        if (ObjectPoolManager.Instance == null) return;

        TryAddPool("coin", coinPrefab, 30);
        TryAddPool("x2coin", coinX2Prefab, 5);
        TryAddPool("giay", springShoesPrefab, 5);
        TryAddPool("hutcoin", magnetCoinPrefab, 5);
        TryAddPool("baycoin", jetpackShortPrefab, 5);
        TryAddPool("baylongcoin", jetpackLongPrefab, 5);
        TryAddPool("key", keyPrefab, 5);
        TryAddPool("box", mysteryBoxPrefab, 5);
        TryAddPool("van", hoverboardPrefab, 5);
    }

    private void TryAddPool(string tag, GameObject prefab, int size)
    {
        if (prefab == null || ObjectPoolManager.Instance.HasPool(tag)) return;
        ObjectPoolManager.Instance.AddPool(tag, prefab, size);
    }

    /// <summary>
    /// Spawns an object from the pool if one exists, otherwise instantiates it.
    /// </summary>
    private GameObject SpawnPooled(string tag, GameObject prefab, Vector3 position, Quaternion rotation, List<GameObject> managerList)
    {
        GameObject go = null;
        if (!string.IsNullOrEmpty(tag) && ObjectPoolManager.Instance != null && ObjectPoolManager.Instance.HasPool(tag))
        {
            go = ObjectPoolManager.Instance.SpawnFromPool(tag, position, rotation);
        }
        else if (prefab != null)
        {
            go = Instantiate(prefab, position, rotation);
        }

        if (go != null && managerList != null && !managerList.Contains(go))
            managerList.Add(go);

        return go;
    }

    /// <summary>
    /// Returns an object to the pool if one exists, otherwise destroys it.
    /// </summary>
    private void ReturnPooled(string tag, GameObject go)
    {
        if (go == null) return;

        if (!string.IsNullOrEmpty(tag) && ObjectPoolManager.Instance != null && ObjectPoolManager.Instance.HasPool(tag))
        {
            ObjectPoolManager.Instance.ReturnToPool(tag, go);
        }
        else
        {
            Destroy(go);
        }
    }

    /// <summary>
    /// Auto-detects pool tag from the object and returns it, or destroys it if no pool exists.
    /// </summary>
    private void ReturnPooled(GameObject go)
    {
        if (go == null) return;

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnToPool(go);
        else
            Destroy(go);
    }

    /// <summary>
    /// Maps an item prefab to its pool tag.
    /// </summary>
    private string GetItemTag(GameObject prefab)
    {
        if (prefab == coinPrefab) return "coin";
        if (prefab == coinX2Prefab) return "x2coin";
        if (prefab == springShoesPrefab) return "giay";
        if (prefab == magnetCoinPrefab) return "hutcoin";
        if (prefab == jetpackShortPrefab) return "baycoin";
        if (prefab == jetpackLongPrefab) return "baylongcoin";
        if (prefab == keyPrefab) return "key";
        if (prefab == mysteryBoxPrefab) return "box";
        if (prefab == hoverboardPrefab) return "van";
        return null;
    }

    /// <summary>
    /// Spawns an item/coin prefab using the appropriate pool tag.
    /// </summary>
    private GameObject SpawnItemPooled(GameObject prefab, Vector3 position, Quaternion rotation, List<GameObject> managerList)
    {
        return SpawnPooled(GetItemTag(prefab), prefab, position, rotation, managerList);
    }

    #endregion
    float addlocation;
    /// <summary>
    /// Builds the first road at game start:
    /// - activates the starterRoad,
    /// - then spawns a few prefab road tiles ahead.
    /// </summary>
    IEnumerator CheckshowGameOject(int value)
    {
        int m = 11;
        for (int i = 0; i < 5; i++)
        {
            value = i;
            // Select road tile prefab from array. Legacy mapping:
            // value 0 -> roadVariant1Prefab, 1 -> roadVariant2Prefab, 2/3/4 -> roadVariant3Prefab
            // Array layout: [0]=base, [1]=variant1, [2]=variant2, [3]=variant3
            int roadIndex = Mathf.Clamp(value + 1, 1, roadTilePrefabs.Length - 1);
            bettwen = GetRoadTilePrefab(roadIndex);
            if (i>=2)
            {
                m = 3;
            }
            for (int j = 1; j < m; j++)
            {
                yield return new WaitForSeconds(0.01f);

                if (bettwen != null)
                {
                    switch (value)
                    {
                        case 0:
                            if (i==0 && starterRoad != null && starterRoad.transform.position.z < 0)
                            {
                                starterRoad.SetActive(true);
                                starterRoad.transform.position = new Vector3(0, 0, location.z);
                            }
                            break;
                        case 1:
                            if (i == 1 && roadSection2 != null && roadSection2.transform.position.z < 0)
                            {
                                roadSection2.SetActive(true);
                                roadSection2.transform.position = new Vector3(0, 0, location.z);
                            }
                            
                            break;
                        case 2:
                            map2.Add(Instantiate(bettwen, location, transform.rotation) as GameObject);
                            break;
                        case 3:
                            map21.Add(Instantiate(bettwen, location, transform.rotation) as GameObject);
                            break;
                        case 4:
                            map22.Add(Instantiate(bettwen, location, transform.rotation) as GameObject);
                            break;
                        default:
                            break;
                    }
                    location.x = 0;
                    location.y = 0;
                    if (m!=3)
                    {
                        location.z = addlocation + 8 * j;
                    }
                    if (m==3)
                    {
                        location.z = addlocation + 40*j;
                    }
                }
            }
            if (i == 0)
            {
                StartCoroutine(emty4(location.z - 1000));
                yield return new WaitForSeconds(0.5f);
            }
            addlocation = location.z;
        }
        yield return new WaitForSeconds(3f);
        Enable();
   
        int randumvcc = Random.Range(0, 13);
        for (int j = 0; j < 2; j++)
        {
            while (lasrandum == randumvcc)
            {
                randumvcc = Random.Range(0, 13);
            }
            lasrandum = randumvcc;
            if (isnewgame == true)
            {
                if (j == 0)
                {
                    continue;
                }

                distinctplayer += 65;
            }
            switch (randumvcc)
            {
                case 0:
                    RandumMapInThestartGame(mapemty4,4);
                    break;
                case 1:
                    RandumMapInThestartGame(mapemty2, 2);
                    break;
                case 2:
                    RandumMapInThestartGame(mapemty3, 3);
                    break;
                case 3:
                    RandumMapInThestartGame(mapemty1, 1);
                    break;
                case 4:
                    RandumMapInThestartGame(mapemty5, 5);
                    break;
                case 5:
                    RandumMapInThestartGame(mapemty6, 6);
                    break;
                case 6:
                    RandumMapInThestartGame(mapemty7, 7);
                    break;
                case 7:
                    RandumMapInThestartGame(mapemty8, 8);
                    break;
                case 8:
                    RandumMapInThestartGame(mapemty9, 9);
                    break;
                case 9:
                    RandumMapInThestartGame(mapemty10, 10);
                    break;
                case 10:
                    RandumMapInThestartGame(mapemty11, 11);
                    break;
                case 11:
                    RandumMapInThestartGame(mapemty12, 12);
                    break;
                case 12:
                    RandumMapInThestartGame(mapemty13, 13);
                    break;
                default:
                    break;
            }
            distinctplayer += 80; // the main change stop core
        }

    }
    void RandumMapInThestartGame(List<GameObject> map , int value)
    {
        for (int i = 0; i < map.Count; i++)
        {
            map[i].gameObject.SetActive(true);
            if (i == 0)
            {
                distinct = Vector3.Distance(map[i].transform.position, transform.position) + distinctplayer;
            }
            map[i].transform.Translate(new Vector3(0, 0, distinct));
            if (i == 0)
            {
                StartCoroutine(Createcoinformap(value, map[i].transform.position.z));
            }
        }
    }
    /// <summary>
    /// load lại map khi chơi lại
    /// </summary>
    /// <returns></returns>
    public IEnumerator playagain()
    {
        yield return new WaitForSeconds(0);
    }

    int notshow;
    /// <summary>
    /// Recycles the starter road and spawned road tiles as the player runs forward.
    /// Also spawns the next obstacle pattern.
    /// </summary>
    IEnumerator randumallmap(int value)
    {
        switch (valueofmap)
        {
            case 0:
                if (starterRoad != null)
                {
                    starterRoad.SetActive(true);
                    starterRoad.transform.position = new Vector3(0, 0, location.z);
                }
                location.z += 80;
                break;
            case 1:
                if (roadSection2 != null)
                {
                    roadSection2.SetActive(true);
                    roadSection2.transform.position = new Vector3(0, 0, location.z);
                }
                location.z += 80;
                break;
            case 2:
                if (map2.Count > 0 && map2[0] != null && map2[0].gameObject.transform.position.z < checkshow-80)
                {
                    for (int i = 0; i < map2.Count; i++)
                    {
                        map2[i].SetActive(true);

                        map2[i].transform.position = new Vector3(0, 0, location.z);
                        yield return new WaitForSeconds(0.01f);
                        location.z += 40;
                    }
                }
                break;
            case 3:
                if (map21.Count > 0 && map21[0] != null && map21[0].gameObject.transform.position.z < checkshow - 80)
                {
                    for (int i = 0; i < map21.Count; i++)
                    {
                        map21[i].gameObject.SetActive(true);

                        map21[i].transform.position = new Vector3(0, 0, location.z);
                        yield return new WaitForSeconds(0.01f);
                        location.z += 40;
                    }
                }
                break;
            case 4:
                if (map22.Count > 0 && map22[0] != null && map22[0].gameObject.transform.position.z < checkshow - 80)
                {
                    for (int i = 0; i < map22.Count; i++)
                    {
                        map22[i].gameObject.SetActive(true);

                        map22[i].transform.position = new Vector3(0, 0, location.z);
                        yield return new WaitForSeconds(0.01f);
                        location.z += 40;
                    }
                }
                break;
            case 5:
                if (tunnelSection != null && tunnelSection.gameObject.transform.position.z < checkshow - 80)
                {
                    tunnelSection.SetActive(true);

                    tunnelSection.transform.position = new Vector3(0, 0, location.z);
                    tunnelSection.SetActive(true);
                    location.z += 80;
                }

                break;
            case 6:
               

                if (bonusSection != null && bonusSection.gameObject.transform.position.z < checkshow-80)
                {
                    bonusSection.SetActive(true);

                    bonusSection.transform.position = new Vector3(0, 0, location.z);
                    bonusSection.SetActive(true);
                    location.z += 80;
                }
                   
                break;
            case 7:
                int valuemap = Random.Range(0,1);
                switch (valuemap)
                {
                    case 0:
                        if (waterfallSection != null && waterfallSection.gameObject.transform.position.z < checkshow - 80)
                        {
                            waterfallSection.transform.position = new Vector3(0, 0, location.z);
                            waterfallSection.SetActive(true);
                            location.z += 80;
                        }
                        if (waterfallSection != null) Getthac();
                        break;
                    default:
                        break;
                }
              
                break;
            default:
                break;
        }
       
        valueofmap++;
        notshow++;
        randumtheemty();
        
        if (notshow == 5)
        {
            notshow = 0;
        }
        if (valueofmap == 8)
        {
            valueofmap = 0;
        }
        
    }
    private void Hidemap(List<GameObject> map)
    {
        for (int i = 0; i < map.Count; i++)
        {
            if (map[i] != null) map[i].SetActive(true);
        }
    }
    [Header("Waterfall Obstacle Variants")]
    public GameObject waterfallObstacleVariant1, waterfallObstacleVariant2, waterfallObstacleVariant3;

    /// <summary>
    /// randum vật cản trong thác nước
    /// </summary>
  public void Getthac()
    {
        // If waterfall obstacle variants are not assigned, do nothing.
        if (waterfallObstacleVariant1 == null && waterfallObstacleVariant2 == null && waterfallObstacleVariant3 == null)
            return;

        int randum = Random.Range(0,3);
        switch (randum)
        {
            case 0:
                if (waterfallObstacleVariant1 != null) waterfallObstacleVariant1.SetActive(true);
                if (waterfallObstacleVariant2 != null) waterfallObstacleVariant2.SetActive(false);
                if (waterfallObstacleVariant3 != null) waterfallObstacleVariant3.SetActive(false);
                break;
            case 1:
                if (waterfallObstacleVariant1 != null) waterfallObstacleVariant1.SetActive(false);
                if (waterfallObstacleVariant2 != null) waterfallObstacleVariant2.SetActive(true);
                if (waterfallObstacleVariant3 != null) waterfallObstacleVariant3.SetActive(false);
                break;
            case 2:
                if (waterfallObstacleVariant1 != null) waterfallObstacleVariant1.SetActive(false);
                if (waterfallObstacleVariant2 != null) waterfallObstacleVariant2.SetActive(false);
                if (waterfallObstacleVariant3 != null) waterfallObstacleVariant3.SetActive(true);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// hàm randum các emty
    /// </summary>
   public  void randumtheemty()
    {
        if (waterfallSection != null && (checkshow<waterfallSection.transform.position.z+10&& checkshow> waterfallSection.transform.position.z-120) ) // khong cho hiện  vật cản tong thác nước
        {
            Debug.Log("");
            Getthac();
            return;
        }
        if (Manageritem.baylongcoin)
        {
            if (coinend!= null)
            {
                if (coinend.transform.position.z-120 >Playermuving.player.transform.position.z)
                {
                    return;
                }
            }
        }
        while (Dontrandum)
        {

        }
        if (call)
        {
            call = false;
            StartCoroutine(whatcallbackmap());
            int nowrandum = Random.Range(0, 15);
            while (lasrandum == nowrandum)
            {
                nowrandum = Random.Range(0, 15);
            }


            lasrandum = nowrandum;
            switch (nowrandum)
            {
                case 0:
                    StartCoroutine(Randummap(mapemty4, 4));
                    break;
                case 1:
                    StartCoroutine(Randummap(mapemty2, 2));

                    break;
                case 2:
                    StartCoroutine(Randummap(mapemty3, 3));
                    break;
                case 3:
                    StartCoroutine(Randummap(mapemty1, 1));
                    break;
                case 4:
                    StartCoroutine(Randummap(mapemty5, 5));
                    break;
                case 5:
                    StartCoroutine(Randummap(mapemty6, 6));
                    break;
                case 6:
                    StartCoroutine(Randummap(mapemty7, 7));
                    break;
                case 7:
                     StartCoroutine(Randummap(mapemty8, 8));
                    break;
                case 8:
                    StartCoroutine(Randummap(mapemty9, 8));
                    break;
                case 9:
                    StartCoroutine(Randummap(mapemty10, 10));
                    break;
                case 10:
                    StartCoroutine(Randummap(mapemty11, 11));
                    break;
                case 11:
                    StartCoroutine(Randummap(mapemty12, 12));
                    break;
                case 12:
                    StartCoroutine(Randummap(mapemty13, 13));
                    break;
                case 13:
                    StartCoroutine(Randummap(mapemty14, 14));
                    break;
                case 14:
                    StartCoroutine(Randummap(mapemty15, 15));
                    break;
                case 15:
                    StartCoroutine(Randummap(mapemty17, 17));
                    break;
                case 16:
                    StartCoroutine(Randummap(mapemty17, 17));
                    break;
                default:
                    break;
            }
        }
      
    }
    bool call = true;
    IEnumerator whatcallbackmap()
    {
        yield return new WaitForSeconds(0.5f);
        call = true;
    }
    int lasrandum =0;  
    #region hệ thống tạo randumcacs đoạn vật cản
    float DistanceTranslate = 0; // khoảng cách cần đi
    public static float backtobehigh = 1;
 
    IEnumerator Randummap(List<GameObject> list,int mapformake)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            yield return new WaitForSeconds(0);
            list[i].gameObject.SetActive(true);

            if (i == 0)
            {
                Vector3 vt3 = new Vector3(0, 0, checkshowx + 16);
                DistanceTranslate = Vector3.Distance(list[i].transform.position, vt3);
            }
            list[i].transform.Translate(new Vector3(0, 0, DistanceTranslate * backtobehigh));
            if (i == 0)
            {
                if (Manageritem.baylongcoin == false)
                {
                    StartCoroutine(Createcoinformap(mapformake, list[i].transform.position.z));
                }
            }
        }
    }
    #endregion

    #region create randum emty
    Vector3 loationback = new Vector3(0,0,0);
    float delayy = 0;
    /// <summary>
    /// tạo map hưỡng dẫn chơi cho lần đầu tiên
    /// </summary>
    /// <param name="tranformz"></param>
    /// <returns></returns>
    IEnumerator emtyforhowtoplay(float tranformz)
    {
        yield return new WaitForSeconds(0);
        for (int i = 0; i < 20; i++)
        {
            loationback.z = tranformz + 8 * i;
            yield return new WaitForSeconds(0.01f);
            createemty("makeemtyshipdie", stylecreate.left, "mapemty1");
        }
    }
    IEnumerator emty1(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(1, locationemty.z));
                //createemty("makeembars_big", stylecreate.left, "mapemty3");
            }
            if (i<4)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty1");
                if (i==2)
                {
                    createemty("makeemtyshipdie", stylecreate.betten, "mapemty1");
                    createemty("makeembars_big", stylecreate.righ, "mapemty1");
                }
            }
            if (i==4)
            {
                createemty("makeembars_big", stylecreate.left, "mapemty1");
                createemty("makeembars_smore", stylecreate.righ, "mapemty1");
                createemty("makeembars_big", stylecreate.betten, "mapemty1");
            }
            if (i==6)
            {
                createemty("makeembars_big", stylecreate.left, "mapemty1");
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty1");
                createemty("makeemtyship", stylecreate.righ, "mapemty1");
            }
            if (i>=7 &&i<9)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty1");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty1");
            }

        }
        StartCoroutine(emty5(location.z-500));
    }
    IEnumerator emty2(float ztranform)
    {
        yield return new WaitForSeconds(0);
        for (int i = 1; i < 10; i++)
        {
            locationemty.z = ztranform + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i < 4)
            {
                    createemty("makeemtyshipdie", stylecreate.left, "mapemty2");
                if (i == 3)
                {
                    createemty("makeembars_big", stylecreate.betten, "mapemty2");
                }
            }
            if (i > 4 && i < 10)
            {
                if (i == 7 || i == 9)
                {
                    createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty2");
                    createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty2");
                }
                if (GetObstaclePrefab("makeemtyshipdie") != null)
                {
                    createemty("makeemtyshipdie", stylecreate.righ, "mapemty2");

                }
                if (i == 5)
                {
                    createemty("makeembars_big", stylecreate.betten, "mapemty2");
                }
            }
        }
        StartCoroutine(emty3(location.z - 500));
    }
    IEnumerator emty3(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i==1)
            {
                createemty("makeembars_big", stylecreate.left, "mapemty3");
                createemty("makeembars_big", stylecreate.righ, "mapemty3");

            }
            if (i>=1&&i<=4)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty3");
                if (i== 3)
                {
                    createemty("makeembars_biger", stylecreate.righ, "mapemty3");

                    createemty("makeemtyship", stylecreate.left, "mapemty3");
                } 
            }
            if (i<=6&&i>=4)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty3");
            }
            if (i== 6)
            {
                createemty("makeemtyship", stylecreate.betten, "mapemty3");
            }
            if (i>6&&i<9)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty3");
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty3");
            }
        }
       StartCoroutine(emty1(location.z-500));
    }
    IEnumerator emty4(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i==1)
            {
               // StartCoroutine(Createcoinformap(4, locationemty.z+25));
                createemty("makeemtyshipdie", stylecreate.left, "mapemty4");
                createemty("makeemtyship", stylecreate.betten, "mapemty4");
            }
            if (i==2)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty4");
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty4");
                createemty("makeemtyshipdie", stylecreate.left, "mapemty4");
            }
            if (i>=3&&i<9)
            {
                if (i==4)
                {
                   // StartCoroutine(Createcoinformap(4, locationemty.z));
                    continue;
                }
                if (i==6)
                {
                    createemty("makeemtyship", stylecreate.betten, "mapemty4");
                    StartCoroutine(createallitem(locationemty.z-15,createitemposition.jumpupbetten));
                    continue;
                }
                if (i==5)
                {
                    createemty("makeemtyshipdie", stylecreate.betten, "mapemty4");
                    createemty("makeemtyshipdie", stylecreate.righ, "mapemty4");
                    continue;
                }
                createemty("makeemtyshipdie", stylecreate.left, "mapemty4");
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty4");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty4");
            }
        }
        StartCoroutine(emty2(location.z-500));
    }
    IEnumerator emty5(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(5, locationemty.z));
                createemty("makeembars_big", stylecreate.left, "mapemty5");
            }
            if (i == 4)
            {
                createemty("makeembars_big", stylecreate.left, "mapemty5");
                createemty("makeembars_big", stylecreate.betten, "mapemty5");
                createemty("makeembars_big", stylecreate.righ, "mapemty5");

            }
            if (i == 5)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty5");
                createemty("makeembars_biger", stylecreate.left, "mapemty5");
                createemty("makeembars_smore", stylecreate.righ, "mapemty5");
            }
            if (i == 2)
            {
                createemty("makeembars_biger", stylecreate.righ, "mapemty5");
            }
            if (i == 6)
            {
                //createemty("makeembars_smore", stylecreate.righ, "mapemty5");
                //createemty("makeembars_smore", stylecreate.left, "mapemty5");
                //createemty("makeembars_smore", stylecreate.betten, "mapemty5");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty5");
                createemty("makeembars_biger", stylecreate.left, "mapemty5");
                createemty("makeembars_smore", stylecreate.righ, "mapemty5");
            }
        }
        StartCoroutine(emty6(location.z-500));
    }
    IEnumerator emty6(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
              //  StartCoroutine(Createcoinformap(6, locationemty.z));
            }
            if (i < 9)
            {
                if (i == 1 || i == 3 || i == 6)
                {
                    createemty("makeemtyshipdie", stylecreate.betten, "mapemty6");
                    createemty("makeemtyship", stylecreate.left, "mapemty6");
                    createemty("makeemtyship", stylecreate.righ, "mapemty6");
                    continue;
                }
                if (i == 5)
                {
                    continue;

                }
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty6");
                createemty("makeemtyshipdie", stylecreate.left, "mapemty6");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty6");
            }
        }
        StartCoroutine(emty7(location.z - 500));

    }
    IEnumerator emty7(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
                createemty("makeembars_smore", stylecreate.betten, "mapemty7");
             //   StartCoroutine(Createcoinformap(7, locationemty.z));

            }
            if (i == 2)
            {
                createemty("makeembridge", stylecreate.betten, "mapemty7");
            }
            if (i == 4)
            {
                createemty("powerpoles", stylecreate.left, "mapemty7");
                createemty("powerpoles", stylecreate.righ, "mapemty7");
            }
            if (i == 9)
            {
                createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty7");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdiemuving", stylecreate.left, "mapemty7");
            }

        }


        StartCoroutine(emty8(location.z - 500));

    }
    IEnumerator emty8(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
                createemty("makeembars_smore", stylecreate.betten, "mapemty8");
               // StartCoroutine(Createcoinformap(8, locationemty.z));

            }
            if (i == 2)
            {
                createemty("powerpoles", stylecreate.left, "mapemty8");
                createemty("powerpoles", stylecreate.righ, "mapemty8");
            }
            if (i == 7)
            {
                createemty("makeembridgerigh", stylecreate.betten, "mapemty8");
            }


        }
        StartCoroutine(emty9(location.z - 500));

    }
    IEnumerator emty9(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
                createemty("makeembars_smore", stylecreate.betten, "mapemty9");
                createemty("powerpoles", stylecreate.righ, "mapemty9");
                //StartCoroutine(Createcoinformap(9, locationemty.z));

            }
            if (i == 2)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty9");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty9");
            }
            if (i == 5)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty9");

            }
            if (i == 3)
            {
                createemty("makeembars_biger", stylecreate.betten, "mapemty9");
            }
            if (i == 7)
            {
                createemty("makeembridgeleft", stylecreate.betten, "mapemty9");

            }

        }
        StartCoroutine(emty10(location.z - 500));

    }
    IEnumerator emty10(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
                createemty("makeembars_smore", stylecreate.betten, "mapemty10");
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty10");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty10");
                createemty("makeembars_smore", stylecreate.left, "mapemty10");
                //StartCoroutine(Createcoinformap(10, locationemty.z));

            }
            if (i == 3)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty10");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty10");
                createemty("makeembars_biger", stylecreate.betten, "mapemty10");

            }
            if (i == 8)
            {
                createemty("makeembridge", stylecreate.betten, "mapemty10");

            }
            if (i == 7)
            {
                createemty("makeembars_smore", stylecreate.betten, "mapemty10");

            }
            if (i == 5)
            {
                createemty("powerpoles", stylecreate.left, "mapemty10");
                createemty("powerpoles", stylecreate.righ, "mapemty10");

            }

        }
        StartCoroutine(emty11(location.z - 500));
    }
    IEnumerator emty11(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(11, locationemty.z));
                createemty("makeembars_smore", stylecreate.betten, "mapemty11");
                createemty("makeembars_smore", stylecreate.righ, "mapemty11");
                createemty("makeembars_smore", stylecreate.left, "mapemty11");
            }
            if (i == 2)
            {
                createemty("makeembars_big", stylecreate.betten, "mapemty11");
                createemty("makeembars_big", stylecreate.left, "mapemty11");
            }
            if (i == 3)
            {
                createemty("makeembars_biger", stylecreate.righ, "mapemty11");
                createemty("makeembars_biger", stylecreate.left, "mapemty11");
            }
            if (i == 4)
            {
                createemty("makeembars_biger", stylecreate.righ, "mapemty11");
                createemty("makeembars_biger", stylecreate.left, "mapemty11");
            }
            if (i == 5)
            {
                createemty("makeembars_big", stylecreate.betten, "mapemty11");
                createemty("makeembars_biger", stylecreate.left, "mapemty11");
            }
            if (i == 6)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty11");
                createemty("makeemtyshipdie", stylecreate.left, "mapemty11");
            }
            if (i == 7)
            {
                createemty("makeembars_smore", stylecreate.righ, "mapemty11");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty11");
                createemty("makeembars_biger", stylecreate.betten, "mapemty11");
            }
        }
        StartCoroutine(emty12(location.z - 500));
    }
    IEnumerator emty12(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(12, locationemty.z));
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty12");
                createemty("makeemtyshipdie", stylecreate.left, "mapemty12");
                createemty("makeembars_smore", stylecreate.betten, "mapemty12");
            }
            if (i == 2)
            {
                //  createemty("powerpoles", stylecreate.betten, "mapemty12");
            }
            if (i == 3)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty12");
                createemty("powerpoles", stylecreate.left, "mapemty12");
            }
            if (i == 4)
            {
                createemty("powerpoles", stylecreate.righ, "mapemty12");
            }
            if (i == 5)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty12");
                createemty("makeembars_smore", stylecreate.betten, "mapemty12");
                createemty("makeembars_biger", stylecreate.left, "mapemty12");
            }
            if (i == 6)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty12");
                createemty("makeembars_biger", stylecreate.left, "mapemty12");
            }
            if (i == 7)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty12");
                createemty("makeemtyshipdie", stylecreate.left, "mapemty12");
            }
            if (i == 8)
            {
                createemty("powerpoles", stylecreate.betten, "mapemty12");
            }
        }
        StartCoroutine(emty13(location.z - 500));
    }
    IEnumerator emty13(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(13, locationemty.z));
                createemty("makeemtyshipdie", stylecreate.left, "mapemty13");
                createemty("makeembars_smore", stylecreate.betten, "mapemty13");
                createemty("makeembars_biger", stylecreate.righ, "mapemty13");
            }
            if (i == 2)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty13");
            }
            if (i == 3)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty13");
                createemty("makeemtyship", stylecreate.left, "mapemty13");
            }
            if (i == 4)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty13");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty13");
            }
            if (i == 5)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty13");
            }
            if (i == 6)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty13");
            }
            if (i == 7)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty13");
                createemty("makeemtyshipdie", stylecreate.left, "mapemty13");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty13");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty13");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty13");
            }
        }
        StartCoroutine(emty14(location.z - 500));

    }
    IEnumerator emty14(float tranformz)
    {

        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(14, locationemty.z));
                createemty("makeemtyship", stylecreate.left, "mapemty14");
                createemty("makeemtyship", stylecreate.righ, "mapemty14");
            }
            if (i == 2)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty14");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty14");
            }

            if (i == 4)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty14");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty14");
            }
            if (i == 5 || i == 6)
            {
                createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty14");
            }
            if (i == 6)
            {
                createemty("makeembars_big", stylecreate.left, "mapemty14");
                createemty("makeembars_biger", stylecreate.righ, "mapemty14");
            }
            if (i == 7)
            {
                createemty("powerpoles", stylecreate.left, "mapemty14");
                createemty("makeembars_biger", stylecreate.betten, "mapemty14");
                createemty("powerpoles", stylecreate.righ, "mapemty14");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty14");
            }
        }
        StartCoroutine(emty15(location.z - 500));

    }
    IEnumerator emty15(float tranformz)
    {
   
        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {//
               // StartCoroutine(Createcoinformap(15, locationemty.z));
                //createemty("makeembars_big", stylecreate.left, "mapemty15");
               // createemty("makeembars_biger", stylecreate.betten, "mapemty15");
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty15");
            }
            if (i == 2)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty15");
                createemty("makeembars_biger", stylecreate.left, "mapemty15");

            }

            if (i==3)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty15");
                //createemty("makeembars_smore", stylecreate.left, "mapemty15");
            }
            if (i == 4)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty15");
            }
            if (i == 5 )
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty15");
                //createemty("makeembars_biger", stylecreate.righ, "mapemty15");
            }
            if (i == 6)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty15");
            }
            if (i == 7)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty15");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty15");
               // createemty("makeembars_biger", stylecreate.left, "mapemty15");
            }
        }
       
        StartCoroutine(emty17(location.z - 500));
        //GetchildmapEmty();
    }
    IEnumerator emty16(float tranformz)
    {



        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
                createemty("makeembars_smore", stylecreate.righ, "mapemty16");
            }
            if (i>=3&&i<=6)
            {
                createemty("makeemtyshipdiemuving", stylecreate.righ, "mapemty16");
            }
            if (i == 7)
            {
                createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty16");
                createemty("makeembars_smore", stylecreate.left, "mapemty16");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty16");
            }
            if (i == 9)
            {
                createemty("makeemtyshipdiemuving", stylecreate.betten, "mapemty16");
            }
        }
        StartCoroutine(emty17(location.z - 500));
    }
    IEnumerator emty17(float tranformz)
    {
        for (int i = 1; i < 10; i++)
        {
            locationemty.z = tranformz + 8 * i;
            yield return new WaitForSeconds(delayy);
            if (i == 1)
            {
               // StartCoroutine(Createcoinformap(17, locationemty.z));
                createemty("makeemtyship", stylecreate.righ, "mapemty17");
            }
            if (i >= 2 && i <= 6)
            {
                createemty("makeemtyshipdie", stylecreate.righ, "mapemty17");
                if (i==4)
                {
                    createemty("makeembars_smore", stylecreate.betten, "mapemty17");
                }
                createemty("makeemtyshipdie", stylecreate.left, "mapemty17");
            }
            if (i == 7)
            {
                createemty("makeemtyshipdie", stylecreate.left, "mapemty17");
            }
            if (i == 8)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty17");
            }
            if (i == 9)
            {
                createemty("makeemtyshipdie", stylecreate.betten, "mapemty17");
            }
        }
        Addallitem(mapemty1);
        Addallitem(mapemty2);
        Addallitem(mapemty3);
        Addallitem(mapemty4);
        Addallitem(mapemty5);
        Addallitem(mapemty6);
        Addallitem(mapemty7);
        Addallitem(mapemty8);
        Addallitem(mapemty9);
        Addallitem(mapemty10);
        Addallitem(mapemty11);
        Addallitem(mapemty12);
        Addallitem(mapemty13);
        Addallitem(mapemty14);
        Addallitem(mapemty15);
        Addallitem(mapemty16);
        Addallitem(mapemty17);
    }
    float timedelay = 0.2f;
    /// <summary>
    /// TẠO COIN RIÊNG CHO TỪNG MAP
    /// </summary>
    /// <param name="map"></param>
    /// <param name="tranormzz"></param>
    /// <returns></returns>
    public IEnumerator Createcoinformap(int map , float tranormzz)
    {
       
        switch (map)
        {
            case 1:
                StartCoroutine(Createitem(3, tranormzz-6, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz , stylecreatecoin.backtoright, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, tranormzz+6, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz+16, stylecreatecoin.backtolef, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 35, stylecreatecoin.up, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 45, stylecreatecoin.up, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                break;
            case 2:
                StartCoroutine(Createitem(5, tranormzz, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, tranormzz+ 31, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz+ 25, stylecreatecoin.backtolef, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz+ 10, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz, createitemposition.righ));
                break;
            case 3:
                StartCoroutine(Createitem(8, tranormzz + 25, stylecreatecoin.line, stylecreatecoinposisition.leftup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(15, tranormzz , stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(8, tranormzz + 50, stylecreatecoin.line, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                break;
            case 4:
                tranormzz = tranormzz + 26;
                StartCoroutine(Createitem(8, tranormzz - 7, stylecreatecoin.up, stylecreatecoinposisition.betwenup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(6, tranormzz - 20, stylecreatecoin.line, stylecreatecoinposisition.betwenup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(6, tranormzz+20, stylecreatecoin.line, stylecreatecoinposisition.betwenup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz - 7, stylecreatecoin.up, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz - 7, stylecreatecoin.up, stylecreatecoinposisition.leftup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz, createitemposition.jumpupbetten));
                break;
            case 5:
                StartCoroutine(Createitem(4, tranormzz +15, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(4, tranormzz+15, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(10, tranormzz+3 , stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 25, stylecreatecoin.up, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz+18, createitemposition.betten));
                StartCoroutine(Createitem(8, tranormzz + 40, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                break;
            case 6:
                StartCoroutine(Createitem(8, tranormzz + 10, stylecreatecoin.line, stylecreatecoinposisition.betwenup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 10, stylecreatecoin.up, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 10, stylecreatecoin.up, stylecreatecoinposisition.leftup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 40, stylecreatecoin.line, stylecreatecoinposisition.betwenup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 44, stylecreatecoin.up, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 44, stylecreatecoin.up, stylecreatecoinposisition.leftup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz + 50, createitemposition.jumpupbetten));
                break;
            case 7: 
                StartCoroutine(Createitem(5,tranormzz +20,stylecreatecoin.line,stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(10, tranormzz + 35, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(12, tranormzz + 35, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz + 26, createitemposition.betten));
                break;
            case 8:
                StartCoroutine(Createitem(3, tranormzz + 20, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz + 20, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(6, tranormzz , stylecreatecoin.line+4, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz+25, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz + 26, createitemposition.betten));
                break;
            case 9:
                StartCoroutine(Createitem(3, tranormzz + 20, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz + 20, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz+6, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 25, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz + 26, createitemposition.betten));
                break;
            case 10:
                StartCoroutine(Createitem(3, tranormzz + 20, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz + 6, stylecreatecoin.backtoright, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz + 26, createitemposition.betten));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 30, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 30, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                break;
            case 11:
                StartCoroutine(Createitem(4, tranormzz+10, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 18, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 50, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                break;
            case 12:
                StartCoroutine(Createitem(3, tranormzz + 11, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 40, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 24, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                break;
            case 13:
                StartCoroutine(Createitem(4, tranormzz +30 , stylecreatecoin.line, stylecreatecoinposisition.betwenup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(4, tranormzz +6 , stylecreatecoin.backtolef, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(createallitem(tranormzz + 40, createitemposition.jumpupbetten));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz-6 , stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                break;
            case 14:    
                StartCoroutine(Createitem(4, tranormzz + 34, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz + 42, stylecreatecoin.backtolef, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(4, tranormzz + 34, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(8, tranormzz + 48, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz + 42, stylecreatecoin.backtoright, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz+4 , stylecreatecoin.line, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz + 4, stylecreatecoin.line, stylecreatecoinposisition.leftup));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(3, tranormzz + 4, stylecreatecoin.line, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                break;
            case 15:
                StartCoroutine(Createitem(3, tranormzz-4 , stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz +1 , stylecreatecoin.backtolef, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz +8, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz  +12 , stylecreatecoin.backtoright, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz + 18, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz + 22, stylecreatecoin.backtoright, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(6, tranormzz + 28, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz + 38, stylecreatecoin.backtolef, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(3, tranormzz + 44, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                StartCoroutine(createallitem(tranormzz + 50, createitemposition.lef));
                break;
            case 16:
                StartCoroutine(Createitem(10, tranormzz, stylecreatecoin.line, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);

                StartCoroutine(Createitem(4, tranormzz + 1, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                break;
            case 17:
                StartCoroutine(Createitem(8, tranormzz + 1, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 18, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 10, stylecreatecoin.line, stylecreatecoinposisition.rightup));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, tranormzz + 10, stylecreatecoin.line, stylecreatecoinposisition.leftup));
                yield return new WaitForSeconds(timedelay);
                break;
            default:
                break;
        }
  
    }


    int carVariantSwitchCounter = 0; 

    void createemty(string name , stylecreate style ,string namemap)
    {
        List<GameObject> listGameemty = new List<GameObject>();
              switch (namemap)
        {

            case "mapemty1":
                listGameemty = mapemty1;
                break;
            case "mapemty2":
                listGameemty = mapemty2;
                break;
            case "mapemty3":
                listGameemty = mapemty3;
                break;
            case "mapemty4":
                listGameemty = mapemty4;
                break;
            case "mapemty5":
                listGameemty = mapemty5;
                break;
            case "mapemty6":
                listGameemty = mapemty6;
                break;
            case "mapemty7":
                listGameemty = mapemty7;
                break;
            case "mapemty8":
                listGameemty = mapemty8;
                break;
            case "mapemty9":
                listGameemty = mapemty9;
                break;
            case "mapemty10":
                listGameemty = mapemty10;
                break;
            case "mapemty11":
                listGameemty = mapemty11;
                break;
            case "mapemty12":
                listGameemty = mapemty12;
                break;
            case "mapemty13":
                listGameemty = mapemty13;
                break;
            case "mapemty14":
                listGameemty = mapemty14;
                break;
            case "mapemty15":
                listGameemty = mapemty15;
                break;
            case "mapemty16":
                listGameemty = mapemty16;
                break;
            case "mapemty17":
                listGameemty = mapemty17;
                break;
            case "mapemty18":
                listGameemty = mapemty18;
                break;
            case "mapemty19":
                listGameemty = mapemty19;
                break;
            case "mapemty20":
                listGameemty = mapemty20;
                break;
            default:
                break;
        }
     
        if (name == "makeemtyshipdie")
        {
            carVariantSwitchCounter++;
            if (carVariantSwitchCounter >= 20)
                carVariantSwitchCounter = 0;
        }

        GameObject prefabToSpawn = GetObstaclePrefab(name);
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[Makesupway] createemty: no prefab found for '{name}'");
            return;
        }

        switch (name)
        {
            case "makeemtyshipdie":
                locationemty.y = 1.6f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -2.5f;
                        break;
                    case stylecreate.betten:
                        locationemty.x = 0f;
                        break;
                    case stylecreate.righ:
                        locationemty.x = 2.5f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeemtyship":
                locationemty.y = 0.6f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -2.5f;
                        break;
                    case stylecreate.betten:
                        locationemty.x = 0f;
                        break;
                    case stylecreate.righ:
                        locationemty.x = 2.5f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeembars_smore":
                locationemty.y = 0.5f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -2.50f;
                        break;
                    case stylecreate.betten:
                        locationemty.x = 0f;
                        break;
                    case stylecreate.righ:
                        locationemty.x = 2.5f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeembars_big":
                locationemty.y = 1.4f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -2.50f;
                        break;
                    case stylecreate.betten:
                        locationemty.x = 0f;
                        break;
                    case stylecreate.righ:
                        locationemty.x = 2.50f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeembars_biger":
                locationemty.y = 2f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -2.5f;
                        break;
                    case stylecreate.betten:
                        locationemty.x = 0;
                        break;
                    case stylecreate.righ:
                        locationemty.x = 2.5f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "powerpoles":
                locationemty.y = 2f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -1.25f;
                        break;
                    case stylecreate.betten:
                        locationemty.x = 1.25f;
                        break;
                    case stylecreate.righ:
                        locationemty.x = 1.25f;  
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeembridgeleft":
               // Debug.Log("goijjjjj");

                locationemty.x = 0f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.y = 6f;
                        break;
                    case stylecreate.betten:
                        locationemty.y = 2.4f;
                        break;
                    case stylecreate.righ:
                        locationemty.y = 6f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeembridgerigh":
                locationemty.x = 0f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.y = 6f;
                        break;
                    case stylecreate.betten:
                        locationemty.y = 2.4f;
                        break;
                    case stylecreate.righ:
                        locationemty.y = 6f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeembridge":
                locationemty.x = 0f;
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.y = 6f;
                        break;
                    case stylecreate.betten:
                        locationemty.y = 2.4f;
                        break;
                    case stylecreate.righ:
                        locationemty.y = 6f;
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            case "makeemtyshipdiemuving":
                switch (style)
                {
                    case stylecreate.left:
                        locationemty.x = -2.5f;
                    
                        break;
                    case stylecreate.betten:
                        locationemty.x = 0f;
                 
                        break;
                    case stylecreate.righ:
                        locationemty.x = 2.5f;
                
                        break;
                    default:
                        break;
                }
                listGameemty.Add(Instantiate(prefabToSpawn, locationemty, transform.rotation) as GameObject);
                break;
            default:
                break;
        }
    }

    #endregion

    Vector3 tranformcoin;

    IEnumerator Createitem(int value , float tranformz,stylecreatecoin style, stylecreatecoinposisition styleposison)
    {
        yield return new WaitForSeconds(0);
        tranformcoin.z = tranformz;
        int top;
        float limvalue;
        switch (styleposison)
        {
            case stylecreatecoinposisition.left:
                tranformcoin.x = -2.5f;
                tranformcoin.y = 0.5f;
                break;
            case stylecreatecoinposisition.bettwen:
                tranformcoin.x = 0;
                tranformcoin.y = 0.5f;
                break;
            case stylecreatecoinposisition.right:
                tranformcoin.x = 2.5f;
                tranformcoin.y = 0.5f;
                break;
            case stylecreatecoinposisition.leftup:
                tranformcoin.x = -2.5f;
                tranformcoin.y = 3.6f;
                break;
            case stylecreatecoinposisition.betwenup:
                tranformcoin.x = 0;
                tranformcoin.y = 3.6f;
                break;
            case stylecreatecoinposisition.rightup:
                tranformcoin.x = 2.5f;
                tranformcoin.y = 3.6f;
                break;
            default:
                break;
        }

        switch (style)
        {
            case stylecreatecoin.line:
                for (int i = 0; i < value+1; i++)
                {                 
                    if (i>0)
                    {
                        SpawnPooled("coin", coinPrefab, tranformcoin, transform.rotation, ManagerAllcoin);
                        tranformcoin.z += 2f;
                    }
                }
                break;
            case stylecreatecoin.backtolef:
                for (int i = 0; i < 4; i++)
                {
                    if (i > 0)
                    {
                      
                        SpawnPooled("coin", coinPrefab, tranformcoin, transform.rotation, ManagerAllcoin);
                        tranformcoin.z += 2f;
                        tranformcoin.x -= 1.25f;
                    }
                }
                break;
            case stylecreatecoin.backtoright:
                for (int i = 0; i < 4; i++)
                {
                    if (i > 0)
                    {
                      
                        SpawnPooled("coin", coinPrefab, tranformcoin, transform.rotation, ManagerAllcoin);
                        tranformcoin.z += 2f;
                        tranformcoin.x += 1.25f;
                    }
                }
                break;
            case stylecreatecoin.up:
                if ((value + 1) % 2 == 0)
                {
                    top = (value + 1) / 2;
                }
                else
                {
                    top = ((value + 1) / 2) + 1;
                }
                for (int i = 0; i < value+1; i++)
                {
                    if (i>0)
                    {
                        if (i<top)
                        {
                            SpawnPooled("coin", coinPrefab, tranformcoin, transform.rotation, ManagerAllcoin);
                            tranformcoin.z += 1.6f;
                            tranformcoin.y += 1f;
                        }
                        if (i>= top)
                        {
                            tranformcoin.y -= 1f;
                            SpawnPooled("coin", coinPrefab, tranformcoin, transform.rotation, ManagerAllcoin);
                            tranformcoin.z += 1.6f;
                        }
                    }
                }
                break;
            default:
                break;
        } 
    }
    Vector3 tranformiteflybt;
    Vector3 tranformiteflyl;
    Vector3 tranformiteflyr;
    bool alowshowitemaddcoin =  true;
    public List<GameObject> ManagerAllcoin = new List<GameObject>();
    public List<GameObject> ManagerAllitem = new List<GameObject>();

    /// <summary>
    /// tạo coin   cho item bay 1 đoạn
    /// </summary>
    /// <param name="tranformzst"></param>
    /// <returns></returns>
    public IEnumerator creacoinforitemfly(float tranformzst)
    {
      
        tranformiteflybt = new Vector3(0,14.5f, tranformzst);
        tranformiteflyl = new Vector3(-2.5f, 14.5f, tranformzst);
        tranformiteflyr = new Vector3(2.5f, 14.5f, tranformzst);
        List<GameObject> mbitem1 = new List<GameObject>();
        GameObject craate1 = null;
        GameObject craate2 = null;
        GameObject craate3 = null;
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.005f);
            if (i== 19)
            {
                int rann = Random.Range(0,4);
                switch (rann)
                {
                    case 0:
                        craate1 = magnetCoinPrefab;
                        craate2 = jetpackLongPrefab;
                        craate3 = coinX2Prefab;
                        break;
                    case 1:
                        craate1 = jetpackLongPrefab;
                        craate2 = coinX2Prefab;
                        craate3 = hoverboardPrefab;
                        break;
                    case 2:
                        craate1 = coinX2Prefab;
                        craate2 = hoverboardPrefab;
                        craate3 = mysteryBoxPrefab;
                        break;
                    case 3:
                        craate1 = hoverboardPrefab;
                        craate2 = mysteryBoxPrefab;
                        craate3 = keyPrefab;
                        break;
                    case 4:
                        craate1 = mysteryBoxPrefab;
                        craate2 = hoverboardPrefab;
                        craate3 = jetpackLongPrefab;
                        break;
                    case 5:
                        craate1 = mysteryBoxPrefab;
                        craate2 = magnetCoinPrefab;
                        craate3 = hoverboardPrefab;
                        break;
                    default:
                        break;
                }
                SpawnItemPooled(craate1, tranformiteflybt, transform.rotation, ManagerAllitem);
                SpawnItemPooled(craate2, tranformiteflyl, transform.rotation, ManagerAllitem);
                SpawnItemPooled(craate3, tranformiteflyr, transform.rotation, ManagerAllitem);
                
                continue;
            }
            SpawnPooled("coin", coinPrefab, tranformiteflybt, transform.rotation, mbitem1);
            SpawnPooled("coin", coinPrefab, tranformiteflyl, transform.rotation, mbitem1);
            SpawnPooled("coin", coinPrefab, tranformiteflyr, transform.rotation, mbitem1);
            if (i==18)
            {

            }
            tranformiteflybt.z += 2;
            tranformiteflyl.z += 2;
            tranformiteflyr.z += 2;
        }
        mbitem1[56].name = "coinend";
        mbitem1[55].name = "coinend";
        mbitem1[54].name = "coinend";
        if (mbitem1[54].GetComponent<BoxCollider>() == null)
            mbitem1[54].gameObject.AddComponent<BoxCollider>();
        mbitem1[54].GetComponent<BoxCollider>().isTrigger = true;
        mbitem1[54].GetComponent<BoxCollider>().center = new Vector3(-0.177f,-120,2.467f);
        mbitem1[54].GetComponent<BoxCollider>().size = new Vector3(18,273,4);

        for (int i = 0; i < mbitem1.Count; i++)
        {
            ManagerAllcoin.Add(mbitem1[i]);

        }
    }

    bool AlowlcreatecoinforFly = true;
    /// <summary>
    /// tạo coin item bay 1 đoạn dài
    /// </summary>
    /// <param name="tranformzst">tọa độ z</param>
    /// <returns></returns>
    public IEnumerator createcoinforitemflylong(float tranformzst)
    {
        List<GameObject> mb = new List<GameObject>();
        int maxvalue = 150 + managerdata.manager.GetDataItemFly() * 45;
        if (AlowlcreatecoinforFly)
        {
            AlowlcreatecoinforFly = false;
            tranformiteflybt = new Vector3(0, 14.5f, tranformzst);
            tranformiteflyl = new Vector3(-2.5f, 14.5f, tranformzst);
            tranformiteflyr = new Vector3(2.5f, 14.5f, tranformzst);
            GameObject craate1 = null, craate2 = null, craate3 = null;
            Vector3 newvector = tranformiteflybt;
            float lastposisonx = newvector.x;
            float posisonx = newvector.x;
            int randum = Random.Range(0, 3);
            
            for (int i = 0; i < maxvalue; i++)
            {
                if (i%10==0&&i>0)
                {

                    randum = Random.Range(0, 3);
                    lastposisonx = newvector.x;
                    switch (randum)
                    {
                        case 0:
                            newvector = tranformiteflyl;
                            break;
                        case 1:
                            newvector = tranformiteflybt;
                            break;
                        case 2:
                            newvector = tranformiteflyr;
                            break;
                        default:
                            break;
                    }
                }
                SpawnPooled("coin", coinPrefab, newvector, transform.rotation, mb);
                if (Manageritem.usingbayitembuy)
                {
                    if (i == maxvalue-1)
                    {
                        usingflyitemtranform = newvector.z + 2;
                    }
                }
                    if (i%10==0 && i > 0)
                    {

                        if (mb[i - 1].gameObject.transform.position.x == -2.5f)
                    {
                        if (mb[i].gameObject.transform.position.x == 0)
                        {
                            mb[i - 1].gameObject.transform.position = new Vector3(-1.25f, newvector.y, newvector.z - 2);
                        }
                        if (mb[i].gameObject.transform.position.x == 2.5f)
                        {
                            mb[i - 1].gameObject.transform.position = new Vector3(1.25f, newvector.y, newvector.z - 2);
                            mb[i - 2].gameObject.transform.position = new Vector3(0, newvector.y, newvector.z - 4);
                            mb[i - 3].gameObject.transform.position = new Vector3(-1.25f, newvector.y, newvector.z - 6);
                        }
                    }
                    if (mb[i - 1].gameObject.transform.position.x == 0)
                    {
                        if (mb[i].gameObject.transform.position.x == -2.5f)
                        {
                            mb[i - 1].gameObject.transform.position = new Vector3(-1.25f, newvector.y, newvector.z - 2);
                        }
                        if (mb[i].gameObject.transform.position.x == 2.5f)
                        {
                            mb[i - 1].gameObject.transform.position = new Vector3(1.25f, newvector.y, newvector.z - 2);
                        }
                    }
                    if (mb[i - 1].gameObject.transform.position.x == 2.5f)
                    {
                        if (mb[i].gameObject.transform.position.x == 0f)
                        {
                            mb[i - 1].gameObject.transform.position = new Vector3(1.25f, newvector.y, newvector.z - 2);
                        }
                        if (mb[i].gameObject.transform.position.x == -2.5f)
                        {
                            mb[i - 1].gameObject.transform.position = new Vector3(-1.25f, newvector.y, newvector.z - 2);
                            mb[i - 2].gameObject.transform.position = new Vector3(0, newvector.y, newvector.z - 4);
                            mb[i - 3].gameObject.transform.position = new Vector3(1.25f, newvector.y, newvector.z - 6);
                        }
                    }
                }
                tranformiteflybt.z += 2;
                tranformiteflyl.z += 2;
                tranformiteflyr.z += 2;
                newvector.z += 2;
                if (i == maxvalue-1)
                {
                    int rann = Random.Range(0, 4);
                    switch (rann)
                    {
                        case 0:
                            craate1 = magnetCoinPrefab;
                            craate2 = coinX2Prefab;
                            craate3 = coinX2Prefab;
                            break;
                        case 1:
                            craate1 = mysteryBoxPrefab;
                            craate2 = coinX2Prefab;
                            craate3 = hoverboardPrefab;
                            break;
                        case 2:
                            craate1 = coinX2Prefab;
                            craate2 = hoverboardPrefab;
                            craate3 = mysteryBoxPrefab;
                            break;
                        case 3:
                            craate1 = hoverboardPrefab;
                            craate2 = mysteryBoxPrefab;
                            craate3 = keyPrefab;
                            break;
                        case 4:
                            craate1 = mysteryBoxPrefab;
                            craate2 = hoverboardPrefab;
                            craate3 = hoverboardPrefab;
                            break;
                        case 5:
                            craate1 = mysteryBoxPrefab;
                            craate2 = magnetCoinPrefab;
                            craate3 = hoverboardPrefab;
                            break;
                        default:
                            break;
                    }
                    SpawnItemPooled(craate1, tranformiteflybt, transform.rotation, ManagerAllitem);
                    SpawnItemPooled(craate2, tranformiteflyl, transform.rotation, ManagerAllitem);
                    SpawnItemPooled(craate3, tranformiteflyr, transform.rotation, ManagerAllitem);
                }
                yield return new WaitForSeconds(0.01f);
            }
            mb[maxvalue-1].name = "coinendtem2";
            if (mb[maxvalue-1].GetComponent<BoxCollider>() == null)
                mb[maxvalue-1].gameObject.AddComponent<BoxCollider>();
            mb[maxvalue-1].GetComponent<BoxCollider>().center = new Vector3(-0.389f, -100, 9.8f);
            mb[maxvalue-1].GetComponent<BoxCollider>().isTrigger = true;
            mb[maxvalue-1].GetComponent<BoxCollider>().size = new Vector3(20, 307, 20.1f);
            mb[maxvalue-1].GetComponent<SphereCollider>().center = new Vector3(0,0,4.4f);
        }
        coinend = mb[maxvalue-1];
       
        for (int i = 0; i < mb.Count; i++)
        {
            ManagerAllcoin.Add(mb[i]);
        }
        AlowlcreatecoinforFly = true;
    }
    GameObject coinend = null;
    public static float usingflyitemtranform;
    Vector3 tranformitem;
    /// <summary>
    ///   tạo item tại vị trí cố định randum  loại item
    /// </summary>
    /// <param name="transformz"></param>
    /// <param name="stylecreate"></param>
    /// <returns></returns>
    public IEnumerator createallitem(float transformz,createitemposition stylecreate)
    {
        tranformitem.z = transformz;
        switch (stylecreate)
        {
            case createitemposition.lef:
                tranformitem.x = -2.5f;
                tranformitem.y = 0.7f;
                break;
            case createitemposition.righ:
                tranformitem.x = 2.5f;
                tranformitem.y = 0.7f;
                break;
            case createitemposition.betten:
                tranformitem.x = 0f;
                tranformitem.y = 0.7f;
                break;
            case createitemposition.lefup:
                tranformitem.x = -2.5f;
                tranformitem.y = 4f;
                break;
            case createitemposition.righup:
                tranformitem.y = 4f;
                tranformitem.x = 2.5f;
                break;
            case createitemposition.bettenup:
                tranformitem.y = 4f;
                tranformitem.x = 0f;
                break;
            case createitemposition.jumpleft:
                tranformitem.x = -2.5f;
                break;
            case createitemposition.jupmrifht:
                tranformitem.x = 2.5f;
                break;
            case createitemposition.jumpbetten:
                tranformitem.x = 0f;
                break;
            case createitemposition.jumpupleft:
                tranformitem.y = 6.7f;
                tranformitem.x = -2.5f;
                break;
            case createitemposition.jumpupright:
                tranformitem.y = 6.7f;
                tranformitem.x = 2.5f;
                break;
            case createitemposition.jumpupbetten:
                tranformitem.y = 6.7f;
                tranformitem.x = 0f;
                break;
            default:
                break;
        }
        string nameitem = "x2";
        int rancoin = Random.Range(0,16);
        switch (rancoin)
        {
            case 0:
                nameitem = "x2coin";
                break;
            case 1:
                nameitem = "giay";
                break;
            case 2:
                nameitem = "hutcoin";
                break;
            case 3:
                nameitem = "baycoin";
                break;
            case 4:
                nameitem = "baylongcoin";
                break;
            case 5:
                nameitem = "key";
                break;
            case 6:
                nameitem = "box";
                break;
            case 7:
                nameitem = "van";
                break;
            default:
                break;
        }
        switch (nameitem)
        {
            case "x2coin":
                SpawnItemPooled(coinX2Prefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "giay":
                SpawnItemPooled(springShoesPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "hutcoin":
                SpawnItemPooled(magnetCoinPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "baycoin":
                SpawnItemPooled(jetpackShortPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "baylongcoin":
                SpawnItemPooled(jetpackLongPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "key":
                SpawnItemPooled(keyPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "box":
                SpawnItemPooled(mysteryBoxPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            case "van":
                SpawnItemPooled(hoverboardPrefab, tranformitem, transform.rotation, ManagerAllitem);
                break;
            default:
                break;
        }
        yield return new WaitForSeconds(0);
    }
    float checkshow;
   public static float checkshowx = 100;
    public GameObject check;
    // Update is called once per frame
    void Update () {
        if (Playermuving.player!= null)
        {
            checkshow = Playermuving.player.gameObject.transform.position.z;

            if (checkshow >= checkshowx)
            {
                checkshowx += 80;
                randummap = Random.Range(0, 3);
                StartCoroutine(randumallmap(randummap));
            }

            // Every 30 frames (~0.5s at 60fps) disable objects far behind the player.
            // This keeps the active object count low without breaking the recycling logic.
            if (Time.frameCount % 30 == 0)
            {
                CullFarObjects();
            }
        }
      
    }

    #region Culling
    /// <summary>
    /// Disables objects that have fallen well behind the player.
    /// Segment spawners re-enable them when they are recycled to the front.
    /// </summary>
    void CullFarObjects()
    {
        if (Playermuving.player == null) return;

        float playerZ = Playermuving.player.transform.position.z;
        float cullZ = playerZ - cullDistanceBehindPlayer;

        if (allSegmentLists == null) CacheSegmentLists();

        foreach (List<GameObject> list in allSegmentLists)
        {
            CullGameObjectList(list, cullZ);
        }

        CullGameObjectList(ManagerAllcoin, cullZ);
        CullGameObjectList(ManagerAllitem, cullZ);
    }

    [Header("Optimization")]
    [Tooltip("Objects behind the player by more than this distance are disabled.")]
    public float cullDistanceBehindPlayer = 50f;

    private List<List<GameObject>> allSegmentLists;

    void CacheSegmentLists()
    {
        allSegmentLists = new List<List<GameObject>>
        {
            map, map1, map2, map21, map22, supwaylist,
            mapemty1, mapemty2, mapemty3, mapemty4, mapemty5,
            mapemty6, mapemty7, mapemty8, mapemty9, mapemty10,
            mapemty11, mapemty12, mapemty13, mapemty14, mapemty15,
            mapemty16, mapemty17, mapemty18, mapemty19, mapemty20
        };
    }

    void CullGameObjectList(List<GameObject> list, float cullZ)
    {
        if (list == null) return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            GameObject go = list[i];
            if (go == null)
            {
                list.RemoveAt(i);
                continue;
            }

            if (go.activeInHierarchy && go.transform.position.z < cullZ)
            {
                go.SetActive(false);
            }
        }
    }
    #endregion
    void Deletecoin()
    {
        StartCoroutine(Delaydestroi());

    }

    public List<GameObject> hidecoin = new List<GameObject>();
    IEnumerator Delaydestroi()
    {
        for (int i = 0; i < hidecoin.Count; i++)
        {
            hidecoin[i].SetActive(false);
        }
        yield return new WaitForSeconds(0);
        for (int i = 0; i < ManagerAllcoin.Count; i++)
        {
            ReturnPooled("coin", ManagerAllcoin[i]);
        }
        ManagerAllcoin.Clear();
        for (int i = 0; i < ManagerAllitem.Count; i++)
        {
            ReturnPooled(ManagerAllitem[i]);
        }
        ManagerAllitem.Clear();
        if (Dontrandum)
        {
            Dontrandum = false;
            randumtheemty();
        }
    }
    float muving  = -300;
    /// <summary>
    /// đưa toàn bộ các đối tượng về phía sau khi có item trượt hoặc key
    /// </summary>
    /// <returns></returns>
    public IEnumerator MuvingbackAllemtyWenhaveitemVan(bool checkvan)
    {
        if (alowback)
        {
            alowback = false;
            if (checkvan==false)
            {
                Playermuving.isplay = false;
            }
            Playermuving.backnowmuvingship = true;
            yield return new WaitForSeconds(0.02f);
            Playermuving.backnowmuvingship = false;

            for (int i = 0; i < allemty.Count; i++)
            {
                if (allemty[i] != null)
                    allemty[i].gameObject.transform.Translate(new Vector3(0, 0, muving));
            }
            yield return new WaitForSeconds(0.2f);
            Enable();
            Deletecoin();
            randumtheemty();
            Playermuving.isplay = true;
            if (MapHowToPlay != null) MapHowToPlay.SetActive(false);
            StartCoroutine( CreateCoinWenDie(Playermuving.player.gameObject.transform.position.z+16));
            StartCoroutine(delay(checkvan));
            Playermuving.player.StartCoroutine(Playermuving.player.backtotruposison());
            if (waterfallSection != null && Playermuving.player.gameObject.transform.position.z > waterfallSection.transform.position.z && Playermuving.player.gameObject.transform.position.z < waterfallSection.transform.position.z + 90)
            {
                Playermuving.player.gameObject.transform.position = new Vector3(0, Playermuving.player.gameObject.transform.position.y, waterfallSection.transform.position.z + 90);
            }
        }
    }
    bool alowback = true;
    bool Dontrandum = false;
    public void Backshipitem()
    {
        //Dontrandum = true;
        //for (int i = 0; i < allemty.Count; i++)
        //{
        //    allemty[i].gameObject.transform.Translate(new Vector3(0, 0, -160));
        //}
        //Deletecoin();

    }
    IEnumerator delay(bool cv)
    {
        yield return new WaitForSeconds(0.2f);
        alowback = true;
        if (cv == false)
        {
            Playermuving.isplay = true;
        }
        Playermuving.isplay = true;
    }

    /// <summary>
    /// back toàn bộ map về đằng sau khi chết - k đủ  - k chọn key
    /// </summary>
    /// <returns></returns>
    public IEnumerator MuvingbackAllemtyWendie()
    {
        
        if (callback)
        {
            Playermuving.isplay = false;
            yield return new WaitForSeconds(0);
            for (int i = 0; i < allemty.Count; i++)
            {
                if (allemty[i] != null)
                    allemty[i].gameObject.transform.Translate(new Vector3(0, 0, muving));
            }
            Enable();
            Deletecoin();
            randumtheemty();
            StartCoroutine(delaycallback());
            if (MapHowToPlay != null) MapHowToPlay.SetActive(false);
            StartCoroutine(CreateCoinWenDie(Playermuving.player.gameObject.transform.position.z + 16));
            Playermuving.player.StartCoroutine(Playermuving.player.backtotruposison());
            if (waterfallSection != null && (Playermuving.player.gameObject.transform.position.z> waterfallSection.transform.position.z&&Playermuving.player.gameObject.transform.position.z < waterfallSection.transform.position.z+90) 
                )
            {
               Playermuving.player.gameObject.transform.position = new Vector3(0, Playermuving.player.gameObject.transform.position.y, waterfallSection.transform.position.z + 85);
            } 
        }
    }

    bool callback = true;
   
    IEnumerator delaycallback()
    {
        yield return new WaitForSeconds(0.5f);
        callback = true; 
    }

    
    bool ismakecoin = true;

    /// <summary>
    /// Sinh Coin trong thác nước
    /// </summary>
    public IEnumerator CreaTeCoinInmap5()
    {
        if (waterfallSection == null) yield break;

        if (ismakecoin)
        {
            ismakecoin = false;
            StartCoroutine(Createitem(3, waterfallSection.transform.position.z + 30, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
            yield return new WaitForSeconds(timedelay);
            StartCoroutine(Createitem(3, waterfallSection.transform.position.z + 48, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
            yield return new WaitForSeconds(timedelay);
            StartCoroutine(Createitem(3, waterfallSection.transform.position.z + 60, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
            yield return new WaitForSeconds(10);
            ismakecoin = true;
        }
         
    }
    /// <summary>
    /// tạo
    /// </summary>
    /// <param name="ztranform"></param>
    private IEnumerator CreateCoinWenDie(float ztranform)
    {
        int stylecreate = Random.Range(0,6);
        switch (stylecreate)
        {
            case 0:
                StartCoroutine(Createitem(5, ztranform , stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, ztranform , stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, ztranform , stylecreatecoin.line, stylecreatecoinposisition.left));
                break;
            case 1:
                StartCoroutine(Createitem(5, ztranform, stylecreatecoin.line, stylecreatecoinposisition.right));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, ztranform, stylecreatecoin.line, stylecreatecoinposisition.left));
                break;
            case 2:
                StartCoroutine(Createitem(5, ztranform, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, ztranform, stylecreatecoin.line, stylecreatecoinposisition.left));
                break;
            case 3:
                StartCoroutine(Createitem(5, ztranform, stylecreatecoin.line, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(5, ztranform, stylecreatecoin.line, stylecreatecoinposisition.right));
                break;
            case 4:
                StartCoroutine(Createitem(8, ztranform, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, ztranform, stylecreatecoin.line, stylecreatecoinposisition.right));
                break;
            case 5:
                StartCoroutine(Createitem(8, ztranform, stylecreatecoin.up, stylecreatecoinposisition.bettwen));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, ztranform, stylecreatecoin.up, stylecreatecoinposisition.left));
                yield return new WaitForSeconds(timedelay);
                StartCoroutine(Createitem(8, ztranform, stylecreatecoin.up, stylecreatecoinposisition.right));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// ẩn hết map emty
    /// </summary>
    public void Enable()
    {
       for (int i = 0; i < allemty.Count; i++)
        {
            if (allemty[i] != null) allemty[i].gameObject.SetActive(false);
        }
    }
    private List<GameObject> allemty = new List<GameObject>();
    void Addallitem(List<GameObject> map)
    {
        for (int i = 0; i < map.Count; i++)
        {
            allemty.Add(map[i]);
        }
    }
}