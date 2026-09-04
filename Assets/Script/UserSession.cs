using UnityEngine;

public static class UserSession
{
    public static bool IsLoggedIn
    {
        get
        {
            var token = PlayerPrefs.GetString("auth_token", "");
            return !string.IsNullOrEmpty(token);
        }
    }

    public static string Token => PlayerPrefs.GetString("auth_token", "");
    public static string RefreshToken => PlayerPrefs.GetString("refresh_token", "");

    public static string UserId => PlayerPrefs.GetString("user_id", "");
    public static string Email => PlayerPrefs.GetString("user_email", "");
    public static string FirstName => PlayerPrefs.GetString("user_first_name", "");
    public static string LastName => PlayerPrefs.GetString("user_last_name", "");
    public static string FullName
    {
        get
        {
            var first = FirstName;
            var last = LastName;
            if (string.IsNullOrEmpty(first) && string.IsNullOrEmpty(last))
                return Email;
            return (first + " " + last).Trim();
        }
    }

    public static string Avatar => PlayerPrefs.GetString("user_avatar", "");
    public static string SelectedLanguage => PlayerPrefs.GetString("user_selected_language", "");
    public static string Points => PlayerPrefs.GetString("user_points", "0");
    public static string Role => PlayerPrefs.GetString("user_role", "");
    public static string OrganizationId => PlayerPrefs.GetString("user_org_id", "");

    public static void Clear()
    {
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.DeleteKey("refresh_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_email");
        PlayerPrefs.DeleteKey("user_first_name");
        PlayerPrefs.DeleteKey("user_last_name");
        PlayerPrefs.DeleteKey("user_avatar");
        PlayerPrefs.DeleteKey("user_selected_language");
        PlayerPrefs.DeleteKey("user_points");
        PlayerPrefs.DeleteKey("user_role");
        PlayerPrefs.DeleteKey("user_org_id");
        PlayerPrefs.Save();
    }
}
