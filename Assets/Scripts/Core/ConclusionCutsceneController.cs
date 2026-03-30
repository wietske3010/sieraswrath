using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// =============================================================================
// ConclusionCutsceneController.cs
// =============================================================================
// Drives the final cutscene after the Conclusion / Heart Gate scene.
// Identical to CutsceneController except:
//   - No nextSceneName field — always routes back to MainMenu.
//   - Last panel button reads "End" instead of "Continue".
//
// Setup: attach to a "CutsceneManager" GameObject in the conclusion cutscene
// scene. Fill the Panels array in the Inspector. Assign all UI references.
// =============================================================================

public class ConclusionCutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutscenePanel
    {
        public Sprite backgroundSprite;
        public string speakerName;
        [TextArea(4, 8)]
        public string bodyText;
        public Sprite characterPortrait;
    }

    [Header("Panel Data")]
    public CutscenePanel[] panels;

    [Header("UI References")]
    public Image backgroundImage;
    public Image fadeOverlay;
    public Image characterPortraitImage;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI bodyText;
    public Button continueButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Settings")]
    public float fadeDuration = 0.6f;

    private int currentPanelIndex = 0;

    void Start()
    {
        SetFadeAlpha(1f);
        ShowPanel(0);
        StartCoroutine(FadeIn());
    }

    void ShowPanel(int index)
    {
        if (index >= panels.Length) return;

        CutscenePanel panel = panels[index];

        backgroundImage.sprite = panel.backgroundSprite;
        speakerNameText.text = panel.speakerName;
        bodyText.text = panel.bodyText;

        if (characterPortraitImage != null)
        {
            bool hasPortrait = panel.characterPortrait != null;
            characterPortraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait)
                characterPortraitImage.sprite = panel.characterPortrait;
        }

        bool isLast = index == panels.Length - 1;
        if (continueButtonText != null)
            continueButtonText.text = isLast ? "End" : "Next";
    }

    public void OnContinuePressed()
    {
        AudioManager.Instance?.PlayUIClick();
        continueButton.interactable = false;
        currentPanelIndex++;

        if (currentPanelIndex >= panels.Length)
            StartCoroutine(TransitionToMainMenu());
        else
            StartCoroutine(CrossfadeToPanel(currentPanelIndex));
    }

    IEnumerator CrossfadeToPanel(int index)
    {
        yield return StartCoroutine(FadeOut());
        ShowPanel(index);
        yield return StartCoroutine(FadeIn());
        continueButton.interactable = true;
    }

    IEnumerator TransitionToMainMenu()
    {
        yield return StartCoroutine(FadeOut());

        if (GameManager.Instance != null)
            GameManager.Instance.OnConclusionCutsceneComplete();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

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
