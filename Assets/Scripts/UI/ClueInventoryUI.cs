using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClueInventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class ClueSlot
    {
        public Button orbButton;           // The orb — is a Button so player can review clue
        public CanvasGroup orbCanvasGroup; // Controls fade-in (alpha 0 → 1)
        public TextMeshProUGUI slotNumber; // Label "1" through "6"
        [HideInInspector] public string assignedComboID;
        [HideInInspector] public bool isCollected;
    }

    [Header("Slots")]
    public ClueSlot[] slots; // Assign 6 in Inspector

    [Header("Dialogue")]
    public DialogueUI dialogueUI; // Assign the scene's DialogueUI to show clue on click

    [Header("Settings")]
    public float fadeInDuration = 0.5f;

    void OnDisable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnClueCollected -= OnClueCollected;
    }

    void Start()
    {
        // Subscribe here (after all Awakes complete) to avoid timing issues with DebugBootstrap
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnClueCollected += OnClueCollected;

        // All orbs start invisible
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].orbCanvasGroup != null)
            {
                slots[i].orbCanvasGroup.alpha = 0f;
                slots[i].orbCanvasGroup.interactable = false;
                slots[i].orbCanvasGroup.blocksRaycasts = false;
            }

            if (slots[i].slotNumber != null)
                slots[i].slotNumber.text = (i + 1).ToString();

            // Disable button interaction until collected
            if (slots[i].orbButton != null)
                slots[i].orbButton.interactable = false;

            int index = i;
            slots[i].orbButton?.onClick.AddListener(() => OnOrbClicked(index));
            Debug.Log($"[ClueInventoryUI] Slot {i} listener added. orbButton null: {slots[i].orbButton == null}");
        }

        // Restore any clues already collected (e.g. returning to scene)
        if (GameStateManager.Instance != null)
        {
            foreach (string comboID in GameStateManager.Instance.GetCollectedClues())
                OnClueCollected(comboID);
        }
    }

    void OnClueCollected(string comboID)
    {
        // Find the first empty slot and assign this clue to it
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isCollected)
            {
                slots[i].assignedComboID = comboID;
                slots[i].isCollected = true;
                slots[i].orbButton.interactable = true;
                StartCoroutine(FadeInOrb(slots[i]));
                return;
            }
        }
    }

    IEnumerator FadeInOrb(ClueSlot slot)
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            slot.orbCanvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        slot.orbCanvasGroup.alpha = 1f;
        slot.orbCanvasGroup.interactable = true;
        slot.orbCanvasGroup.blocksRaycasts = true;
    }

    public void ReviewClues()
    {
        AudioManager.Instance?.PlayUIClick();
        var lines = new System.Collections.Generic.List<string>();
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isCollected) continue;
            ClueCombo combo = ClueDatabaseManager.Instance?.GetComboByID(slots[i].assignedComboID);
            if (combo != null)
                lines.Add($"Clue {i + 1}: {combo.clueText}");
        }

        if (lines.Count == 0) return;

        Time.timeScale = 0f;
        if (dialogueUI != null)
            dialogueUI.StartDialogue(lines.ToArray(), () => Time.timeScale = 1f);
    }

    void OnOrbClicked(int index)
    {
        Debug.Log($"[ClueInventoryUI] Orb {index} clicked.");
        ClueSlot slot = slots[index];
        AudioManager.Instance?.PlayUIClick();

        if (!slot.isCollected) { Debug.Log($"[ClueInventoryUI] Slot {index} not collected — returning."); return; }

        ClueCombo combo = ClueDatabaseManager.Instance.GetComboByID(slot.assignedComboID);
        if (combo == null) { Debug.Log($"[ClueInventoryUI] No combo found for ID '{slot.assignedComboID}' — returning."); return; }

        Debug.Log($"[ClueInventoryUI] Opening clue: {combo.clueText}");

        string[] lines = new string[]
        {
            $"Clue {index + 1}: {combo.clueText}"
        };

        Time.timeScale = 0f;

        if (dialogueUI != null)
            dialogueUI.StartDialogue(lines, () => Time.timeScale = 1f);
        else
            Debug.Log("[ClueInventoryUI] dialogueUI is null!");
    }
}
