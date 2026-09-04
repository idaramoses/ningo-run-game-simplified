using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Characters selection canvas: thumbnail switching, preview,
/// name/status display, equip/buy actions and back-to-home navigation.
/// </summary>
public class CharactersController : MonoBehaviour
{
    [Serializable]
    public class CharacterData
    {
        public string id;
        public string displayName;
        public int price;            // 0 means FREE
        public bool owned;
        public Color thumbColor;
        public Sprite portrait;      // Optional - assignable from Inspector
        public Sprite nameSprite;    // Stylized name logo image (e.g. WIKICAT, LUNA)
    }

    public enum CharacterState { Current, Owned, Free, Buy }

    private const string PrefsCurrentKey = "Characters_Current";
    private const string PrefsOwnedPrefix = "Characters_Owned_";

    [Header("Catalog")]
    [SerializeField] private List<CharacterData> characters = new List<CharacterData>();

    [Header("Auto-Discovered UI (optional overrides)")]
    [SerializeField] private UnityEngine.UI.RawImage previewImage;
    [SerializeField] private TMP_Text previewLabel;
    [SerializeField] private Image nameImage;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusIcon;

    [Header("Status icon sprites")]
    [SerializeField] private Sprite checkIconSprite;  // shown for Current / Owned
    [SerializeField] private Sprite coinIconSprite;   // shown for Buy (price)

    [Header("Footer state panels (siblings under Footer)")]
    [SerializeField] private GameObject footerCurrent; // BACK TO HOME centered
    [SerializeField] private GameObject footerFree;    // BACK TO HOME + EQUIP
    [SerializeField] private GameObject footerBuy;     // BACK TO HOME + BUY NOW

    private readonly List<Button> thumbButtons = new List<Button>();
    private readonly List<GameObject> thumbOutlines = new List<GameObject>();
    private int selectedIndex = 0;

    private UImanager homeController;

    private void Awake()
    {
        EnsureCatalog();
        AutoDiscoverUI();
        LoadOwnership();
        WireButtons();
    }

    private GameObject FindPreviewContainer()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "CharacterPreviewContainer")
            {
                return root;
            }
        }
        return null;
    }

    private void OnEnable()
    {
        // Activate 3D preview container
        var container = FindPreviewContainer();
        if (container != null)
        {
            container.SetActive(true);
        }

        // Default selected = currently equipped character from SelectedRunner index, fallback to PrefsCurrentKey
        int savedIndex = PlayerPrefs.GetInt("SelectedRunner", 0);
        if (savedIndex >= 0 && savedIndex < characters.Count)
        {
            SelectCharacter(savedIndex);
        }
        else
        {
            string current = PlayerPrefs.GetString(PrefsCurrentKey, characters.Count > 0 ? characters[0].id : "");
            int idx = characters.FindIndex(c => c.id == current);
            SelectCharacter(idx >= 0 ? idx : 0);
        }
    }

    private void OnDisable()
    {
        // Deactivate 3D preview container
        var container = FindPreviewContainer();
        if (container != null)
        {
            container.SetActive(false);
        }
    }

    private void EnsureCatalog()
    {
        if (characters != null && characters.Count > 0) return;
        characters = new List<CharacterData>
        {
            new CharacterData{ id = "wikicat", displayName = "WIKICAT", price = 0,     owned = true,  thumbColor = new Color(0.55f,0.78f,0.18f) },
            new CharacterData{ id = "loger",   displayName = "LOGER",   price = 0,     owned = false, thumbColor = new Color(0.40f,0.45f,0.50f) },
            new CharacterData{ id = "ferna",   displayName = "FERNA",   price = 25000, owned = false, thumbColor = new Color(0.95f,0.55f,0.75f) },
            new CharacterData{ id = "luna",    displayName = "LUNA",    price = 30000, owned = false, thumbColor = new Color(0.95f,0.55f,0.35f) }
        };
    }

    private void AutoDiscoverUI()
    {
        Transform stage = transform.Find("Stage");
        if (stage != null)
        {
            Transform p = stage.Find("CharacterPreview_Image");
            if (p != null) { previewImage = p.GetComponent<UnityEngine.UI.RawImage>(); previewLabel = p.GetComponentInChildren<TMP_Text>(); }
        }

        Transform info = transform.Find("CharacterInfo");
        if (info != null)
        {
            Transform plate = info.Find("NameImage");
            if (plate == null) plate = info.Find("NamePlate_Image"); // backwards-compat
            if (plate != null) nameImage = plate.GetComponent<Image>();
            Transform sRow = info.Find("StatusRow");
            if (sRow != null)
            {
                Transform si = sRow.Find("StatusIcon");
                Transform st = sRow.Find("StatusText");
                if (si != null) statusIcon = si.GetComponent<Image>();
                if (st != null) statusText = st.GetComponent<TMP_Text>();
            }
        }

        Transform footer = transform.Find("Footer");
        if (footer != null)
        {
            Transform fc = footer.Find("Footer_Current");
            Transform ff = footer.Find("Footer_Free");
            Transform fb = footer.Find("Footer_Buy");
            if (fc != null) footerCurrent = fc.gameObject;
            if (ff != null) footerFree    = ff.gameObject;
            if (fb != null) footerBuy     = fb.gameObject;
        }

        Transform thumbs = transform.Find("Thumbnails");
        thumbButtons.Clear();
        thumbOutlines.Clear();
        if (thumbs != null)
        {
            // Maintain definition order (matches data list)
            for (int i = 0; i < thumbs.childCount; i++)
            {
                Transform t = thumbs.GetChild(i);
                if (!t.name.StartsWith("Thumb_")) continue;
                Button b = t.GetComponent<Button>();
                Transform outline = t.Find("Selected_Outline");
                if (b != null)
                {
                    thumbButtons.Add(b);
                    thumbOutlines.Add(outline != null ? outline.gameObject : null);
                }
            }
        }
    }

    private void ShowFooter(GameObject which)
    {
        if (footerCurrent != null) footerCurrent.SetActive(footerCurrent == which);
        if (footerFree    != null) footerFree.SetActive   (footerFree    == which);
        if (footerBuy     != null) footerBuy.SetActive    (footerBuy     == which);
    }

    private void WireButtons()
    {
        // Thumbnail buttons are wired in code because they need per-index closures.
        for (int i = 0; i < thumbButtons.Count; i++)
        {
            int idx = i;
            thumbButtons[i].onClick.RemoveAllListeners();
            thumbButtons[i].onClick.AddListener(() => SelectCharacter(idx));
        }

        // NOTE: Footer buttons (BackToHome / Equip / BuyNow) are intentionally
        // NOT wired here. Use the Unity Inspector's OnClick() event list on each
        // Button component and assign the public methods below.
    }

    /// <summary>
    /// Assign this to the OnClick() event of every BackToHomeButton
    /// (in Footer_Current, Footer_Free, and Footer_Buy).
    /// </summary>
    public void OnBackPressed()
    {
        Debug.Log("[Characters] BACK TO HOME pressed");
        if (homeController == null)
            homeController = UImanager.uimanager;
        if (homeController != null)
            homeController.HideCharacters();
        else
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Assign this to the OnClick() event of EquipButton (in Footer_Free).
    /// </summary>
    public void OnEquipPressed()
    {
        if (selectedIndex < 0 || selectedIndex >= characters.Count) return;
        CharacterData c = characters[selectedIndex];
        c.owned = true;
        SaveOwnership(selectedIndex);
        PlayerPrefs.SetString(PrefsCurrentKey, c.id);
        
        // Synchronize with the RunnerSelectionManager / RunnerManager
        if (RunnerSelectionManager.Instance != null)
        {
            RunnerSelectionManager.Instance.SelectRunner(selectedIndex);
        }
        else
        {
            // Direct fallback if Instance is not running
            RunnerManager.SetSelectedRunnerIndex(selectedIndex);
            var prefab = RunnerManager.GetSelectedRunnerPrefab();
            if (prefab == null && RunnerManager.Instance != null && selectedIndex < RunnerManager.Instance.runners.Count)
            {
                RunnerManager.SetSelectedRunnerPrefab(RunnerManager.Instance.runners[selectedIndex].runnerGameObject);
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log("[Characters] Equipped " + c.displayName);
        UpdateUI();
    }

    /// <summary>
    /// Assign this to the OnClick() event of BuyNowButton (in Footer_Buy).
    /// </summary>
    public void OnBuyPressed()
    {
        if (selectedIndex < 0 || selectedIndex >= characters.Count) return;
        CharacterData c = characters[selectedIndex];
        int balance = PlayerPrefs.GetInt("LocalCoins", 0);
        if (balance >= c.price)
        {
            PlayerPrefs.SetInt("LocalCoins", balance - c.price);
            c.owned = true;
            SaveOwnership(selectedIndex);
            Debug.Log("[Characters] Purchased " + c.displayName + " for " + c.price);
            UpdateUI();
        }
        else
        {
            Debug.Log("[Characters] Not enough coins for " + c.displayName);
        }
    }

    private void LoadOwnership()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].price <= 0)
            {
                // free characters become "owned" only after equip; wikicat starts owned
            }
            int saved = PlayerPrefs.GetInt(PrefsOwnedPrefix + characters[i].id, characters[i].owned ? 1 : 0);
            characters[i].owned = saved == 1;
        }
    }

    private void SaveOwnership(int idx)
    {
        PlayerPrefs.SetInt(PrefsOwnedPrefix + characters[idx].id, characters[idx].owned ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        selectedIndex = index;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Toggle outlines
        for (int i = 0; i < thumbOutlines.Count; i++)
        {
            if (thumbOutlines[i] != null)
                thumbOutlines[i].SetActive(i == selectedIndex);
        }

        CharacterData c = characters[selectedIndex];
        string equippedId = PlayerPrefs.GetString(PrefsCurrentKey, characters[0].id);

        if (nameImage != null)
        {
            if (c.nameSprite != null)
            {
                nameImage.sprite = c.nameSprite;
                nameImage.color = Color.white;
                nameImage.enabled = true;
            }
            else
            {
                // No sprite assigned yet - hide the image so the slot stays clean.
                nameImage.sprite = null;
                nameImage.enabled = false;
            }
        }

        // Update 3D preview model
        if (CharacterPreviewController.Instance != null && RunnerSelectionManager.Instance != null)
        {
            var runners = RunnerSelectionManager.Instance.allRunners;
            if (selectedIndex >= 0 && selectedIndex < runners.Count)
            {
                var runnerData = runners[selectedIndex];
                if (runnerData != null && runnerData.runnerPrefab != null)
                {
                    CharacterPreviewController.Instance.UpdatePreview(runnerData.runnerPrefab);
                }
            }
        }

        if (previewImage != null)
        {
            if (previewLabel != null) previewLabel.gameObject.SetActive(false);
        }
        else if (previewLabel != null)
        {
            previewLabel.gameObject.SetActive(true);
            previewLabel.text = "[ " + c.displayName + " ]";
        }

        CharacterState state;
        if (c.id == equippedId) state = CharacterState.Current;
        else if (c.owned) state = CharacterState.Owned;
        else if (c.price <= 0) state = CharacterState.Free;
        else state = CharacterState.Buy;

        ApplyStateVisuals(state, c);
    }

    private void ApplyStateVisuals(CharacterState state, CharacterData c)
    {
        switch (state)
        {
            case CharacterState.Current:
                // Equipped/current: green check + CURRENT label; solo back button.
                SetStatusIcon(checkIconSprite, new Color(0.4f, 0.85f, 0.4f));
                if (statusText != null) { statusText.text = "CURRENT"; statusText.color = new Color(0.85f, 0.85f, 0.9f); }
                ShowFooter(footerCurrent);
                break;
            case CharacterState.Owned:
                // Already purchased, not equipped: re-use the Free footer (EQUIP).
                SetStatusIcon(checkIconSprite, new Color(0.4f, 0.85f, 0.4f));
                if (statusText != null) { statusText.text = "OWNED"; statusText.color = new Color(0.85f, 0.85f, 0.9f); }
                ShowFooter(footerFree);
                break;
            case CharacterState.Free:
                // Free unlock: hide icon, green FREE label, EQUIP footer
                SetStatusIcon(null, Color.white);
                if (statusText != null) { statusText.text = "FREE"; statusText.color = new Color(0.55f, 0.85f, 0.35f); }
                ShowFooter(footerFree);
                break;
            case CharacterState.Buy:
                // Locked: coin icon + price, BUY NOW footer
                SetStatusIcon(coinIconSprite, Color.white);
                if (statusText != null) { statusText.text = string.Format("{0:N0}", c.price); statusText.color = new Color(1f, 0.85f, 0.3f); }
                ShowFooter(footerBuy);
                break;
        }
    }

    private void SetStatusIcon(Sprite sprite, Color tint)
    {
        if (statusIcon == null) return;
        if (sprite == null)
        {
            statusIcon.sprite = null;
            statusIcon.gameObject.SetActive(false);
            return;
        }
        statusIcon.gameObject.SetActive(true);
        statusIcon.sprite = sprite;
        statusIcon.color = tint;
        statusIcon.enabled = true;
    }

}
