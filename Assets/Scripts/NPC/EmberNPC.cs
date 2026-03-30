using UnityEngine;

public class EmberNPC : MonoBehaviour
{
    [Header("Assignment")]
    public string assignedComboID;

    [Header("Visuals")]
    public SpriteRenderer npcSprite;

    [Header("Interaction")]
    public GameObject interactPrompt;
    public DialogueUI dialogueUI;
    public HUDController hudController;
    public ManaUI manaUI;

    private bool inRange = false;
    private bool collected = false;

    void Start()
    {
        if (npcSprite != null) npcSprite.enabled = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (hudController == null)
            hudController = FindFirstObjectByType<HUDController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = true;
        if (npcSprite != null) npcSprite.enabled = true;
        if (!collected && interactPrompt != null)
        {
            interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = false;
        if (npcSprite != null) npcSprite.enabled = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (inRange && !collected && Input.GetKeyDown(KeyCode.E))
        {
            AudioManager.Instance?.ClueOpened();
            Interact();
        }
    }

    void Interact()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);

        if (ClueDatabaseManager.Instance == null)
        {
            Debug.LogError("[EmberNPC] ClueDatabaseManager.Instance is null. Is PersistentManagers loaded?");
            return;
        }

        ClueCombo combo = ClueDatabaseManager.Instance.GetComboByID(assignedComboID);
        if (combo == null)
        {
            Debug.LogError($"[EmberNPC] No clue combo found for ID: '{assignedComboID}'");
            return;
        }

        string[] lines =
        {
            "Siera: I'm looking for information about Thornwall.",
            $"Ember Member: {combo.clueText}",
            "Ember Member: Take this mana charge. It will stun a Bounded long enough for you to escape. Use it wisely — I can only spare one."
        };

        if (dialogueUI != null)
        {
            hudController?.SetHUDVisible(false);
            dialogueUI.StartDialogue(lines, OnDialogueComplete);
        }
        else
        {
            Debug.LogError("[EmberNPC] dialogueUI is not assigned on this NPC.");
            OnDialogueComplete();
        }
    }

    void OnDialogueComplete()
    {
        collected = true;
        hudController?.SetHUDVisible(true);
        GameStateManager.Instance.CollectClue(assignedComboID);
        manaUI?.CollectMana();
    }
}
