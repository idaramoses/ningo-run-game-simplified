using UnityEngine;

/// <summary>
/// Lightweight credential storage used by the standalone LoginForm.
/// NOTE: This is NOT cryptographically secure. It stores values in PlayerPrefs
/// for auto re-login convenience only. The integrated onboarding flow in
/// UserInfoController does not rely on this class.
/// </summary>
public static class SecureCredentials
{
    private const string EmailKey = "secure_saved_email";
    private const string PasswordKey = "secure_saved_password";
    private const string IsGuestKey = "secure_saved_is_guest";

    public static void SaveCredentials(string email, string password, bool isGuest)
    {
        if (string.IsNullOrEmpty(email)) return;

        PlayerPrefs.SetString(EmailKey, email);
        PlayerPrefs.SetString(PasswordKey, password ?? "");
        PlayerPrefs.SetInt(IsGuestKey, isGuest ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[SecureCredentials] Credentials saved for {email} (guest={isGuest})");
    }

    public static bool HasCredentials()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString(EmailKey, ""));
    }

    public static string GetEmail()
    {
        return PlayerPrefs.GetString(EmailKey, "");
    }

    public static string GetPassword()
    {
        return PlayerPrefs.GetString(PasswordKey, "");
    }

    public static bool IsGuest()
    {
        return PlayerPrefs.GetInt(IsGuestKey, 0) == 1;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(EmailKey);
        PlayerPrefs.DeleteKey(PasswordKey);
        PlayerPrefs.DeleteKey(IsGuestKey);
        PlayerPrefs.Save();
    }
}
