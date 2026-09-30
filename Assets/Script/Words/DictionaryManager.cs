using System;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryManager
{
    [Serializable]
    public class DictionaryWord
    {
        public string id;
        public string word;
        public string translation;
        public string category;
        public string language;

        public DictionaryWord() { }

        public DictionaryWord(string word, string translation, string category = "general", string language = "")
        {
            this.word = word;
            this.translation = translation;
            this.category = category;
            this.language = language;
        }
    }

    [Serializable]
    private class DictionaryResponseWrapper
    {
        public bool success;
        public List<DictionaryWord> words;
        public List<DictionaryWord> data;
        public List<DictionaryWord> dictionary;
    }

    [Serializable]
    private class NestedDataWrapper
    {
        public bool success;
        public NestedWordsData data;
    }

    [Serializable]
    private class NestedWordsData
    {
        public List<DictionaryWord> words;
    }

    [Serializable]
    private class DirectArrayWrapper
    {
        public List<DictionaryWord> items;
    }

    private static List<DictionaryWord> cachedWords = null;
    private static bool cacheLoadAttempted = false;

    public static void InvalidateMemoryCache()
    {
        cachedWords = null;
        cacheLoadAttempted = false;
        Debug.Log("[DictionaryManager] Memory cache invalidated.");
    }

    public static bool HasDictionary()
    {
        EnsureCacheLoaded();
        return cachedWords != null && cachedWords.Count > 0;
    }

    public static int GetTotalWordCount()
    {
        EnsureCacheLoaded();
        return cachedWords != null ? cachedWords.Count : 0;
    }

    public static List<DictionaryWord> GetAllWords()
    {
        EnsureCacheLoaded();
        return cachedWords != null ? new List<DictionaryWord>(cachedWords) : new List<DictionaryWord>();
    }

    public static List<DictionaryWord> GetWordsByCategory(string category)
    {
        EnsureCacheLoaded();
        if (cachedWords == null) return new List<DictionaryWord>();

        if (string.IsNullOrEmpty(category)) return GetAllWords();

        List<DictionaryWord> matches = new List<DictionaryWord>();
        foreach (var w in cachedWords)
        {
            if (string.Equals(w.category, category, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(w);
            }
        }

        return matches;
    }

    private static void EnsureCacheLoaded()
    {
        if (cachedWords != null || cacheLoadAttempted) return;
        cacheLoadAttempted = true;

        string json = PlayerPrefs.GetString("dictionary_data", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                // Try API format: {"success":true,"data":{"words":[...]}}
                NestedDataWrapper nested = JsonUtility.FromJson<NestedDataWrapper>(json);
                if (nested != null && nested.data != null && nested.data.words != null && nested.data.words.Count > 0)
                    cachedWords = nested.data.words;

                // Try wrapped format
                DictionaryResponseWrapper wrapper = cachedWords == null ? JsonUtility.FromJson<DictionaryResponseWrapper>(json) : null;
                if (wrapper != null)
                {
                    if (wrapper.words != null && wrapper.words.Count > 0)
                        cachedWords = wrapper.words;
                    else if (wrapper.data != null && wrapper.data.Count > 0)
                        cachedWords = wrapper.data;
                    else if (wrapper.dictionary != null && wrapper.dictionary.Count > 0)
                        cachedWords = wrapper.dictionary;
                }

                // Try array format if still null
                if (cachedWords == null)
                {
                    string wrappedArray = "{\"items\":" + json + "}";
                    DirectArrayWrapper direct = JsonUtility.FromJson<DirectArrayWrapper>(wrappedArray);
                    if (direct != null && direct.items != null && direct.items.Count > 0)
                    {
                        cachedWords = direct.items;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DictionaryManager] Failed to parse dictionary_data JSON: {ex.Message}");
            }
        }

        // No local fallback word bank — dictionary MUST come from the API.
        if (cachedWords == null || cachedWords.Count == 0)
        {
            Debug.LogError("[DictionaryManager] API dictionary not available (missing or unparseable 'dictionary_data').");
            cachedWords = null;
        }
    }
}