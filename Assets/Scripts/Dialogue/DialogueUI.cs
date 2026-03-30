using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [System.Serializable]
    public class SpeakerPortrait
    {
        public string speakerName;
        public Sprite portrait;
    }
    [Header("Panel References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueBodyText;
    public Image avatarImage;
    public Button continueButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Speaker Portraits")]
    public SpeakerPortrait[] speakerPortraits;

    [Header("Per-Line Backgrounds")]
    public Image backgroundImage;
    public Sprite[] lineBackgrounds; // index matches line index; null = keep current

    [Header("Multiple Choice")]
    public GameObject choicePanel;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;

    private string[] currentLines;
    private int currentLineIndex;
    private System.Action onComplete;
    private System.Action<string> onChoiceSelected;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
    }

    // ─── Linear Dialogue ──────────────────────────────────────────────────────

    public void StartDialogue(string[] lines, System.Action onDialogueComplete)
    {
        Debug.Log($"[DialogueUI] StartDialogue called. dialoguePanel assigned: {dialoguePanel != null}");
        if (lines == null || lines.Length == 0)
        {
            onDialogueComplete?.Invoke();
            return;
        }

        currentLines = lines;
        currentLineIndex = 0;
        onComplete = onDialogueComplete;

        if (choicePanel != null) choicePanel.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        ShowLine();
    }

    void ShowLine()
    {
        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        bool isLast = currentLineIndex == currentLines.Length - 1;
        if (continueButtonText != null)
            continueButtonText.text = isLast ? "Close" : "Continue";

        if (backgroundImage != null && lineBackgrounds != null && currentLineIndex < lineBackgrounds.Length)
            if (lineBackgrounds[currentLineIndex] != null)
                backgroundImage.sprite = lineBackgrounds[currentLineIndex];

        string line = currentLines[currentLineIndex];
        int colonIndex = line.IndexOf(':');

        if (colonIndex >= 0)
        {
            string speaker = line.Substring(0, colonIndex).Trim();
            if (speakerNameText != null) speakerNameText.text = speaker;
            if (dialogueBodyText != null) dialogueBodyText.text = line.Substring(colonIndex + 1).Trim();
            SwapPortrait(speaker);
        }
        else
        {
            if (speakerNameText != null) speakerNameText.text = string.Empty;
            if (dialogueBodyText != null) dialogueBodyText.text = line;
        }
    }

    void OnContinueClicked()
    {
        currentLineIndex++;
        AudioManager.Instance?.PlayUIClick();
        ShowLine();
    }

    void SwapPortrait(string speakerName)
    {
        if (avatarImage == null || speakerPortraits == null) return;
        foreach (var sp in speakerPortraits)
        {
            if (sp.speakerName == speakerName)
            {
                avatarImage.sprite = sp.portrait;
                avatarImage.gameObject.SetActive(true);
                return;
            }
        }
        avatarImage.gameObject.SetActive(false);
    }

    void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Capture before clearing so a callback can safely start new dialogue
        System.Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    // ─── Multiple Choice ──────────────────────────────────────────────────────

    public void PresentMultipleChoice(string question, string[] options, string highlightAnswer, System.Action<string> onSelected)
    {
        onChoiceSelected = onSelected;

        if (speakerNameText != null) speakerNameText.text = "Heart Guard";
        if (dialogueBodyText != null) dialogueBodyText.text = question;
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i >= options.Length)
            {
                choiceButtons[i].gameObject.SetActive(false);
                continue;
            }

            choiceButtons[i].gameObject.SetActive(true);
            string option = options[i];

            if (choiceTexts[i] != null)
            {
                choiceTexts[i].text = option;
                choiceTexts[i].color = (highlightAnswer != null && option == highlightAnswer)
                    ? Color.green : Color.white;
            }

            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(option));
        }
    }

    void OnChoiceClicked(string choice)
    {
        AudioManager.Instance?.PlayUIClick();
        if (choicePanel != null) choicePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(true);

        System.Action<string> callback = onChoiceSelected;
        onChoiceSelected = null;
        callback?.Invoke(choice);
    }
}
