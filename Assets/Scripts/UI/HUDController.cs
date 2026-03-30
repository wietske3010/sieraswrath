using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("Sub-Controllers")]
    public LivesDisplay livesDisplay;
    public SuspicionBarUI suspicionBar;
    public ClueInventoryUI clueInventory;

    [Header("Panels")]
    public GameObject pausePanel;

    [Header("HUD Elements to hide during NPC dialogue")]
    public GameObject[] hudElements;

    private bool[] hudElementsWereActive;

    public void OpenPause()
    {
        AudioManager.Instance?.PlayUIClick();
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePause()
    {
        AudioManager.Instance?.PlayUIClick();
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SetHUDVisible(bool visible)
    {
        Debug.Log($"[HUDController] SetHUDVisible({visible})\n{System.Environment.StackTrace}");
        if (visible)
        {
            for (int i = 0; i < hudElements.Length; i++)
                if (hudElements[i] != null && hudElementsWereActive != null && i < hudElementsWereActive.Length)
                    hudElements[i].SetActive(hudElementsWereActive[i]);
        }
        else
        {
            hudElementsWereActive = new bool[hudElements.Length];
            for (int i = 0; i < hudElements.Length; i++)
            {
                if (hudElements[i] != null)
                {
                    hudElementsWereActive[i] = hudElements[i].activeSelf;
                    hudElements[i].SetActive(false);
                }
            }
        }
    }

    void OnDisable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnLivesChanged -= livesDisplay.UpdateLives;
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnLivesChanged += livesDisplay.UpdateLives;

        if (GameStateManager.Instance != null)
            livesDisplay.UpdateLives(GameStateManager.Instance.GetLives());
    }
}
