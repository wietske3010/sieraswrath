using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// =============================================================================
// PrologueController.cs
// =============================================================================
// Drives the prologue sequence. Each panel has a background image, a speaker
// name, body text, and a flag for whether Siera's portrait should show.
// The Continue button fades between panels. On the final panel, pressing
// Continue fades to black and hands off to GameManager to load Level 1.
//
// Setup: attach to an empty "PrologueManager" GameObject in the Prologue scene.
// Fill the Panels array in the Inspector with all 6 (+ optional 7th) panels.
// Assign all UI references in the Inspector.
// =============================================================================

public class PrologueController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Panel data — fill these in the Inspector
    // -------------------------------------------------------------------------
    [System.Serializable]
    public class ProloguePanel
    {
        public Sprite backgroundSprite;
        public string speakerName;
        [TextArea(4, 8)]
        public string bodyText;
        // Show Siera's portrait when she is speaking; hide it for Narrator panels
        public bool showSieraPortrait;
    }

    [Header("Panel Data")]
    public ProloguePanel[] panels;

    [Header("UI References")]
    public Image backgroundImage;       // Full-screen background, swaps per panel
    public Image fadeOverlay;           // Full-screen black Image for fade transitions
    public Image sieraPortrait;         // Siera_UI.png — shown/hidden per panel
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI bodyText;
    public Button continueButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Settings")]
    public float fadeDuration = 0.6f;   // Seconds for each fade in/out

    private int currentPanelIndex = 0;

    // -------------------------------------------------------------------------
    // Start — begin fully black, then fade into panel 0
    // -------------------------------------------------------------------------
    void Start()
    {
        SetFadeAlpha(1f);
        ShowPanel(0);
        StartCoroutine(FadeIn());
    }

    // -------------------------------------------------------------------------
    // ShowPanel — updates all UI elements for the given panel index
    // -------------------------------------------------------------------------
    void ShowPanel(int index)
    {
        if (index >= panels.Length) return;

        ProloguePanel panel = panels[index];

        backgroundImage.sprite = panel.backgroundSprite;
        speakerNameText.text = panel.speakerName;
        bodyText.text = panel.bodyText;

        // Show Siera's portrait only on panels where she is the speaker
        if (sieraPortrait != null)
            sieraPortrait.gameObject.SetActive(panel.showSieraPortrait);

        // Change button label on the final panel
        bool isLast = index == panels.Length - 1;
        if (continueButtonText != null)
            continueButtonText.text = isLast ? "Enter" : "Continue";
    }

    // -------------------------------------------------------------------------
    // OnContinuePressed — called by the Continue button's OnClick event
    // -------------------------------------------------------------------------
    public void OnContinuePressed()
    {
        // Disable button while transitioning to prevent double-clicks
        AudioManager.Instance?.PlayUIClick();
        continueButton.interactable = false;
        currentPanelIndex++;

        if (currentPanelIndex >= panels.Length)
            StartCoroutine(TransitionToLevel());
        else
            StartCoroutine(CrossfadeToPanel(currentPanelIndex));
    }

    // -------------------------------------------------------------------------
    // CrossfadeToPanel — fades out, swaps content, fades back in
    // -------------------------------------------------------------------------
    IEnumerator CrossfadeToPanel(int index)
    {
        yield return StartCoroutine(FadeOut());
        ShowPanel(index);
        yield return StartCoroutine(FadeIn());
        continueButton.interactable = true;
    }

    // -------------------------------------------------------------------------
    // TransitionToLevel — fades to black then tells GameManager to load Level 1
    // -------------------------------------------------------------------------
    IEnumerator TransitionToLevel()
    {
        yield return StartCoroutine(FadeOut());

        if (GameManager.Instance != null)
            GameManager.Instance.OnPrologueComplete();
        else
        {
            // Fallback for testing Prologue scene directly without GameManager
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level1_Catacombs");
        }
    }

    // -------------------------------------------------------------------------
    // Fade helpers
    // -------------------------------------------------------------------------
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(1f - (elapsed / fadeDuration));
            yield return null;
        }
        SetFadeAlpha(0f);
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(elapsed / fadeDuration);
            yield return null;
        }
        SetFadeAlpha(1f);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = Mathf.Clamp01(alpha);
        fadeOverlay.color = c;
    }
}
