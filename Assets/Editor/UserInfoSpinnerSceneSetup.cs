using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UserInfoSpinnerSceneSetup
{
    [MenuItem("Tools/Ningo/Setup UserInfo Button Spinners")]
    public static void ApplyToOpenScene()
    {
        if (Application.isPlaying || EditorApplication.isCompiling) return;

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "UserInfo") return;

        UserInfoController controller = Object.FindFirstObjectByType<UserInfoController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            Debug.LogError("[UserInfoSpinnerSceneSetup] UserInfoController not found in the open UserInfo scene.");
            return;
        }

        SerializedObject serializedController = new SerializedObject(controller);
        Button loginButton = serializedController.FindProperty("loginButton").objectReferenceValue as Button;
        Button guestButton = serializedController.FindProperty("guestButton").objectReferenceValue as Button;
        Button languageNextButton = serializedController.FindProperty("languageNextButton").objectReferenceValue as Button;
        Button letsGoButton = serializedController.FindProperty("letsGoButton").objectReferenceValue as Button;
        GameObject loginPanel = serializedController.FindProperty("loginPanel").objectReferenceValue as GameObject;
        GameObject languagePanel = serializedController.FindProperty("languagePanel").objectReferenceValue as GameObject;
        GameObject welcomePanel = serializedController.FindProperty("welcomePanel").objectReferenceValue as GameObject;

        if (loginButton == null)
        {
            Debug.LogError("[UserInfoSpinnerSceneSetup] Login Button is not assigned.");
            return;
        }

        if (guestButton == null || guestButton == loginButton)
        {
            guestButton = FindButton(loginPanel, "guest", loginButton);
            serializedController.FindProperty("guestButton").objectReferenceValue = guestButton;
        }

        if (guestButton == null)
        {
            Debug.LogError("[UserInfoSpinnerSceneSetup] Guest Button could not be found.");
            return;
        }

        if (languageNextButton == null)
        {
            languageNextButton = FindButton(languagePanel, "next", null);
            serializedController.FindProperty("languageNextButton").objectReferenceValue = languageNextButton;
        }

        if (languageNextButton == null)
        {
            Debug.LogError("[UserInfoSpinnerSceneSetup] Language Next Button could not be found.");
            return;
        }

        if (letsGoButton == null)
        {
            letsGoButton = FindButton(welcomePanel, "go", null);
            serializedController.FindProperty("letsGoButton").objectReferenceValue = letsGoButton;
        }

        if (letsGoButton == null)
        {
            Debug.LogError("[UserInfoSpinnerSceneSetup] Let's Go Button could not be found.");
            return;
        }

        Sprite spinnerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/mainUI/loading_circle.png");
        GameObject loginSpinner = CreateSpinner(loginButton, "LoginSpinner", spinnerSprite);
        GameObject guestSpinner = CreateSpinner(guestButton, "GuestSpinner", spinnerSprite);
        GameObject languageNextSpinner = CreateSpinner(languageNextButton, "LanguageNextSpinner", spinnerSprite);
        GameObject letsGoSpinner = CreateSpinner(letsGoButton, "LetsGoSpinner", spinnerSprite);

        serializedController.FindProperty("loginButtonSpinner").objectReferenceValue = loginSpinner;
        serializedController.FindProperty("guestButtonSpinner").objectReferenceValue = guestSpinner;
        serializedController.FindProperty("languageNextButtonSpinner").objectReferenceValue = languageNextSpinner;
        serializedController.FindProperty("letsGoButtonSpinner").objectReferenceValue = letsGoSpinner;
        serializedController.FindProperty("loginButtonText").objectReferenceValue = loginButton.GetComponentInChildren<TMP_Text>(true);
        serializedController.FindProperty("guestButtonText").objectReferenceValue = guestButton.GetComponentInChildren<TMP_Text>(true);
        serializedController.FindProperty("languageNextButtonText").objectReferenceValue = languageNextButton.GetComponentInChildren<TMP_Text>(true);
        serializedController.FindProperty("letsGoButtonText").objectReferenceValue = letsGoButton.GetComponentInChildren<TMP_Text>(true);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[UserInfoSpinnerSceneSetup] Added and wired LoginSpinner, GuestSpinner, LanguageNextSpinner, and LetsGoSpinner in UserInfo.unity.");
    }

    private static Button FindButton(GameObject panel, string text, Button excluded)
    {
        if (panel == null) return null;

        foreach (Button button in panel.GetComponentsInChildren<Button>(true))
        {
            if (button == excluded) continue;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (button.name.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                (label != null && label.text.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0))
                return button;
        }

        return null;
    }

    private static GameObject CreateSpinner(Button button, string spinnerName, Sprite sprite)
    {
        Transform existing = button.transform.Find(spinnerName);
        GameObject spinner = existing != null ? existing.gameObject : new GameObject(spinnerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LoadingSpinner));
        spinner.transform.SetParent(button.transform, false);

        RectTransform rect = spinner.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(42f, 42f);

        Image image = spinner.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;

        spinner.SetActive(false);
        EditorUtility.SetDirty(spinner);
        return spinner;
    }
}
