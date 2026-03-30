using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    private int lives = 5;
    private int manaCharges = 0;
    private List<string> collectedClues = new List<string>();
    private Dictionary<string, string> selectedCombos = new Dictionary<string, string>(); // comboID -> correctAnswer

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetLives() => lives;

    public int GetManaCharges() => manaCharges;

    public void AddMana()
    {
        manaCharges++;
        OnManaChanged?.Invoke(manaCharges);
    }

    public void UseMana()
    {
        manaCharges = Mathf.Max(0, manaCharges - 1);
        OnManaChanged?.Invoke(manaCharges);
    }

    public void SetLives(int newLives)
    {
        lives = Mathf.Clamp(newLives, 0, 5);
        OnLivesChanged?.Invoke(lives);
    }

    public void CollectClue(string comboID)
    {
        if (!collectedClues.Contains(comboID))
        {
            collectedClues.Add(comboID);
            OnClueCollected?.Invoke(comboID);
        }
    }

    public List<string> GetCollectedClues() => new List<string>(collectedClues);

    public void SetSelectedCombos(Dictionary<string, string> combos)
    {
        selectedCombos = new Dictionary<string, string>(combos);
    }

    public Dictionary<string, string> GetSelectedCombos() => new Dictionary<string, string>(selectedCombos);

    public void Reset()
    {
        lives = 5;
        manaCharges = 0;
        collectedClues.Clear();
        selectedCombos.Clear();
        OnLivesChanged?.Invoke(lives);
        OnManaChanged?.Invoke(manaCharges);
    }

    public event System.Action<int> OnLivesChanged;
    public event System.Action<string> OnClueCollected;
    public event System.Action<int> OnManaChanged;
}
