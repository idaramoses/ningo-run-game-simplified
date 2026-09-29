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
    private class DirectArrayWrapper
    {
        public List<DictionaryWord> items;
    }

    private static List<DictionaryWord> cachedWords = null;

    public static void InvalidateMemoryCache()
    {
        cachedWords = null;
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
        if (cachedWords != null) return;

        string json = PlayerPrefs.GetString("dictionary_data", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                // Try wrapped format
                DictionaryResponseWrapper wrapper = JsonUtility.FromJson<DictionaryResponseWrapper>(json);
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

        // Fallback default word bank if empty
        if (cachedWords == null || cachedWords.Count == 0)
        {
            cachedWords = GetFallbackWords();
        }
    }

    private static List<DictionaryWord> GetFallbackWords()
    {
        string currentLang = PlayerPrefs.GetString("user_selected_language", "yoruba").ToLowerInvariant();

        List<DictionaryWord> list = new List<DictionaryWord>();

        if (currentLang == "hausa")
        {
            list.Add(new DictionaryWord("Head", "Kai", "body", "hausa"));
            list.Add(new DictionaryWord("Eye", "Ido", "body", "hausa"));
            list.Add(new DictionaryWord("Hand", "Hannu", "body", "hausa"));
            list.Add(new DictionaryWord("Leg", "Ƙafa", "body", "hausa"));
            list.Add(new DictionaryWord("Mouth", "Baki", "body", "hausa"));
            list.Add(new DictionaryWord("Ear", "Kunne", "body", "hausa"));
            list.Add(new DictionaryWord("Water", "Ruwa", "general", "hausa"));
            list.Add(new DictionaryWord("Food", "Abinci", "general", "hausa"));
            list.Add(new DictionaryWord("Run", "Gudu", "actions", "hausa"));
            list.Add(new DictionaryWord("Come", "Zo", "actions", "hausa"));
        }
        else if (currentLang == "igbo")
        {
            list.Add(new DictionaryWord("Head", "Isi", "body", "igbo"));
            list.Add(new DictionaryWord("Eye", "Anya", "body", "igbo"));
            list.Add(new DictionaryWord("Hand", "Aka", "body", "igbo"));
            list.Add(new DictionaryWord("Leg", "Ụkwụ", "body", "igbo"));
            list.Add(new DictionaryWord("Mouth", "Ọnụ", "body", "igbo"));
            list.Add(new DictionaryWord("Ear", "Ntị", "body", "igbo"));
            list.Add(new DictionaryWord("Water", "Mmiri", "general", "igbo"));
            list.Add(new DictionaryWord("Food", "Nri", "general", "igbo"));
            list.Add(new DictionaryWord("Run", "Gbaa", "actions", "igbo"));
            list.Add(new DictionaryWord("Come", "Bịa", "actions", "igbo"));
        }
        else if (currentLang == "ibibio")
        {
            list.Add(new DictionaryWord("Head", "Ibuot", "body", "ibibio"));
            list.Add(new DictionaryWord("Eye", "Enyin", "body", "ibibio"));
            list.Add(new DictionaryWord("Hand", "Ubok", "body", "ibibio"));
            list.Add(new DictionaryWord("Leg", "Ukod", "body", "ibibio"));
            list.Add(new DictionaryWord("Mouth", "Inua", "body", "ibibio"));
            list.Add(new DictionaryWord("Ear", "Utong", "body", "ibibio"));
            list.Add(new DictionaryWord("Water", "Mmọñ", "general", "ibibio"));
            list.Add(new DictionaryWord("Food", "Udia", "general", "ibibio"));
            list.Add(new DictionaryWord("Run", "Feñe", "actions", "ibibio"));
            list.Add(new DictionaryWord("Come", "Di", "actions", "ibibio"));
        }
        else // default Yoruba
        {
            list.Add(new DictionaryWord("Head", "Orí", "body", "yoruba"));
            list.Add(new DictionaryWord("Eye", "Ojú", "body", "yoruba"));
            list.Add(new DictionaryWord("Hand", "Ọwọ́", "body", "yoruba"));
            list.Add(new DictionaryWord("Leg", "Ẹsẹ̀", "body", "yoruba"));
            list.Add(new DictionaryWord("Mouth", "Ẹnu", "body", "yoruba"));
            list.Add(new DictionaryWord("Ear", "Etí", "body", "yoruba"));
            list.Add(new DictionaryWord("Water", "Omi", "general", "yoruba"));
            list.Add(new DictionaryWord("Food", "Oúnjẹ", "general", "yoruba"));
            list.Add(new DictionaryWord("Run", "Sáré", "actions", "yoruba"));
            list.Add(new DictionaryWord("Come", "Wá", "actions", "yoruba"));
        }

        return list;
    }
}
