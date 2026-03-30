using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConclusionClueReview : MonoBehaviour
{
    public DialogueUI dialogueUI;
    public Button clueButton;

    void Start()
    {
        bool hasClues = GameStateManager.Instance != null &&
                        GameStateManager.Instance.GetCollectedClues().Count > 0;
        if (clueButton != null)
            clueButton.gameObject.SetActive(hasClues);
    }

    public void ReviewClues()
    {
        List<string> collected = GameStateManager.Instance?.GetCollectedClues();
        if (collected == null || collected.Count == 0) return;

        List<string> lines = new List<string>();
        int index = 1;
        foreach (string comboID in collected)
        {
            ClueCombo combo = ClueDatabaseManager.Instance?.GetComboByID(comboID);
            if (combo != null)
                lines.Add($"Clue {index++}: {combo.clueText}");
        }

        if (lines.Count == 0) return;
        dialogueUI?.StartDialogue(lines.ToArray(), null);
    }
}
