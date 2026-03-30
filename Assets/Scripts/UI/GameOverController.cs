using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [Header("Game Over Image")]
    public Image gameOverImage;

    [Header("Context Sprites (match GameOverContext order)")]
    public Sprite level1Sprite;
    public Sprite level2Sprite;
    public Sprite level3Sprite;
    public Sprite heartGateSprite;

    void Start()
    {
        if (gameOverImage == null || GameManager.Instance == null) return;

        gameOverImage.sprite = GameManager.Instance.LastGameOverContext switch
        {
            GameOverContext.Level1    => level1Sprite,
            GameOverContext.Level2    => level2Sprite,
            GameOverContext.Level3    => level3Sprite,
            GameOverContext.HeartGate => heartGateSprite,
            _                        => level1Sprite
        };
    }

    public void OnContinuePressed()
    {
        AudioManager.Instance?.PlayUIClick();
        GameManager.Instance.GoToMainMenu();
    }
}
