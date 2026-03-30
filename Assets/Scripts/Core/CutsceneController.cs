using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// =============================================================================
// CutsceneController.cs
// =============================================================================
// Drives between-level cutscene sequences. Works identically to
// PrologueController with one key difference: each panel carries its own
// character portrait sprite, so different characters can appear per panel
// (Siera, a Thrall, an Ember Witch, etc.).
//
// Also takes a configurable nextSceneName so the same script works for the
// cutscene after Level 1, after Level 2, and after Level 3.
//
// Setup: attach to a "CutsceneManager" GameObject in each cutscene scene.
// Fill the Panels array in the Inspector. Assign all UI references.
// Set Next Scene Name to the scene that should load after the final panel.
// =============================================================================

public class CutsceneController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Panel data — fill in the Inspector
    // -------------------------------------------------------------------------
    [System.Serializable]
    public class CutscenePanel
    {
        public Sprite backgroundSprite;
        public string speakerName;
        [TextArea(4, 8)]
        public string bodyText;
        // Each panel has its own portrait — assign whichever character is speaking.
        // Leave null to hide the portrait for that panel.
        public Sprite characterPortrait;
    }

    [Header("Panel Data")]
    public CutscenePanel[] panels;

    [Header("Scene Routing")]
    // The exact scene name to load after the final panel (e.g. "Level2_Gardens")
    public string nextSceneName;

    [Header("UI References")]
    public Image backgroundImage;           // Full-screen background, swaps per panel
    public Image fadeOverlay;               // Full-screen black Image for fade transitions
    public Image characterPortraitImage;    // Portrait frame — sprite swaps per panel
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI bodyText;
    public Button continueButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Settings")]
    public float fadeDuration = 0.6f;

    private int currentPanelIndex = 0;

    // -------------------------------------------------------------------------
    // Start — begin fully black, fade into panel 0
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

        CutscenePanel panel = panels[index];

        backgroundImage.sprite = panel.backgroundSprite;
        speakerNameText.text = panel.speakerName;
        bodyText.text = panel.bodyText;

        // Show the portrait for this panel's character, or hide if none assigned
        if (characterPortraitImage != null)
        {
            bool hasPortrait = panel.characterPortrait != null;
            characterPortraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait)
                characterPortraitImage.sprite = panel.characterPortrait;
        }

        // Last panel says "Continue" — feels right for between-level transitions
        bool isLast = index == panels.Length - 1;
        if (continueButtonText != null)
            continueButtonText.text = isLast ? "Continue" : "Next";
    }

    // -------------------------------------------------------------------------
    // OnContinuePressed — called by the Continue button's OnClick event
    // -------------------------------------------------------------------------
    public void OnContinuePressed()
    {
        AudioManager.Instance?.PlayUIClick();
        continueButton.interactable = false;
        currentPanelIndex++;

        if (currentPanelIndex >= panels.Length)
            StartCoroutine(TransitionToNextScene());
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
    // TransitionToNextScene — fades to black then loads nextSceneName
    // -------------------------------------------------------------------------
    IEnumerator TransitionToNextScene()
    {
        yield return StartCoroutine(FadeOut());

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("CutsceneController: nextSceneName is not set!");
            yield break;
        }

        // Tell GameManager the cutscene is done so state updates correctly,
        // then let it load the next scene — or load directly as fallback
        if (GameManager.Instance != null)
            GameManager.Instance.OnCutsceneComplete(nextSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
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
