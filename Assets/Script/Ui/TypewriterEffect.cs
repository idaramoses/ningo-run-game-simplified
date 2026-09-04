using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float startDelay = 0.2f;
    [SerializeField] private bool playOnEnable = true;

    private TMP_Text textComponent;
    private string fullText;
    private Coroutine typeCoroutine;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        fullText = textComponent.text;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            StartEffect();
        }
    }

    private void OnDisable()
    {
        StopEffect();
    }

    public void StartEffect()
    {
        StopEffect();
        typeCoroutine = StartCoroutine(TypeText());
    }

    public void StopEffect()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }
    }

    private IEnumerator TypeText()
    {
        if (textComponent == null) yield break;
        
        textComponent.text = "";
        
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float delay = 1f / charactersPerSecond;
        int currentLength = 0;
        
        while (currentLength < fullText.Length)
        {
            // Check for rich text tags and skip them so they don't render typed out
            if (fullText[currentLength] == '<')
            {
                int tagCloseIndex = fullText.IndexOf('>', currentLength);
                if (tagCloseIndex != -1)
                {
                    currentLength = tagCloseIndex + 1;
                    textComponent.text = fullText.Substring(0, currentLength);
                    continue;
                }
            }

            currentLength++;
            textComponent.text = fullText.Substring(0, currentLength);
            yield return new WaitForSeconds(delay);
        }

        typeCoroutine = null;
    }
}
