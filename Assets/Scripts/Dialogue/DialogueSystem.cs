using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Place this in the Conclusion scene. It runs the Heart Gate interrogation sequence.
public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public DialogueUI dialogueUI;
    public QuestionAnswerHandler qaHandler;

    [Header("Fade")]
    public Image fadeOverlay;
    public float fadeDuration = 0.8f;

    [Header("Response Panel")]
    public GameObject responsePanel;
    public TextMeshProUGUI responseText;
    public float responseDuration = 2f;

    private int currentQuestionIndex = 0;
    private Dictionary<string, string> selectedCombos;
    private List<string> collectedClues;
    private List<string> comboKeys;

    void Start()
    {
        if (fadeOverlay != null)
            StartCoroutine(FadeInThenBegin());
        else
            BeginScene();
    }

    IEnumerator FadeInThenBegin()
    {
        SetFadeAlpha(1f);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(1f - (elapsed / fadeDuration));
            yield return null;
        }
        SetFadeAlpha(0f);
        BeginScene();
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = Mathf.Clamp01(alpha);
        fadeOverlay.color = c;
    }

    void BeginScene()
    {
        if (responsePanel != null) responsePanel.SetActive(false);
        // Pre-dialogue suspicion gate check
        float currentSuspicion = SuspicionManager.Instance.GetSuspicion();
        if (currentSuspicion >= 100f)
        {
            GameManager.Instance.TriggerCaughtAtGate();
            return;
        }

        selectedCombos = GameStateManager.Instance.GetSelectedCombos();
        collectedClues = GameStateManager.Instance.GetCollectedClues();
        comboKeys = new List<string>(selectedCombos.Keys);

        GameManager.Instance.SetState(GameState.DialogueSequence);
        PresentQuestion(0);
    }

    void PresentQuestion(int index)
    {
        if (index >= comboKeys.Count)
        {
            GameManager.Instance.TriggerWin();
            return;
        }

        string comboID = comboKeys[index];
        string correctAnswer = selectedCombos[comboID];

        ClueCombo combo = ClueDatabaseManager.Instance.GetComboByID(comboID);
        if (combo == null)
        {
            Debug.LogError($"Combo not found: {comboID}");
            GameManager.Instance.TriggerWin();
            return;
        }

        string[] options = qaHandler.GenerateOptions(correctAnswer, comboID);

        dialogueUI.PresentMultipleChoice(
            combo.questionText,
            options,
            null,
            OnAnswerSelected
        );
    }

    void OnAnswerSelected(string selectedAnswer)
    {
        string comboID = comboKeys[currentQuestionIndex];
        string correctAnswer = selectedCombos[comboID];
        bool isCorrect = selectedAnswer == correctAnswer;

        if (!isCorrect)
            SuspicionManager.Instance.AddSuspicion(15f);

        StartCoroutine(ShowResponseThenAdvance(isCorrect));
    }

    IEnumerator ShowResponseThenAdvance(bool isCorrect)
    {
        if (responsePanel != null)
        {
            responsePanel.SetActive(true);
            if (responseText != null)
            {
                responseText.text = isCorrect ? "Correct" : "Wrong";
                responseText.color = isCorrect ? Color.green : Color.red;
            }
        }

        yield return new WaitForSeconds(responseDuration);

        if (responsePanel != null)
            responsePanel.SetActive(false);

        if (SuspicionManager.Instance.GetSuspicion() >= 100f)
        {
            GameManager.Instance.TriggerCaughtMidDialogue();
            yield break;
        }

        currentQuestionIndex++;
        PresentQuestion(currentQuestionIndex);
    }
}
