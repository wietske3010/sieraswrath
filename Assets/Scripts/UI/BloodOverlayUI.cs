using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodOverlayUI : MonoBehaviour
{
    public Image overlayImage;
    public float flashInDuration = 0.15f;
    public float flashOutDuration = 0.6f;

    void Start()
    {
        if (overlayImage != null) SetAlpha(0f);
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerCaught += Flash;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerCaught -= Flash;
    }

    void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        // Fade in fast
        float elapsed = 0f;
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(elapsed / flashInDuration);
            yield return null;
        }
        SetAlpha(1f);

        // Fade out slow
        elapsed = 0f;
        while (elapsed < flashOutDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - (elapsed / flashOutDuration));
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float alpha)
    {
        if (overlayImage == null) return;
        Color c = overlayImage.color;
        c.a = Mathf.Clamp01(alpha);
        overlayImage.color = c;
    }
}
