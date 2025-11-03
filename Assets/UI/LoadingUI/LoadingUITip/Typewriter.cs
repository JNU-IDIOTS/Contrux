// Typewriter.cs
using System.Collections;
using TMPro;
using UnityEngine;

public class Typewriter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] float cps = 40f; // chars per second
    Coroutine _co;

    public void Play(string text)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Run(text));
    }

    IEnumerator Run(string s)
    {
        if (!label) { yield break; }
        label.text = s; label.maxVisibleCharacters = 0;
        int n = s.Length;
        float t = 0f;
        while (label.maxVisibleCharacters < n)
        {
            t += Time.unscaledDeltaTime * cps;
            label.maxVisibleCharacters = Mathf.Min(n, Mathf.FloorToInt(t));
            yield return null;
        }
    }
}
