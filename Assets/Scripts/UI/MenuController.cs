using UnityEngine;

// Attach to a manager GameObject in MainMenu, GameOver, and Victory scenes
public class MenuController : MonoBehaviour
{
    public void OnNewGamePressed()
    {
        AudioManager.Instance?.PlayUIClick();
        GameManager.Instance.NewGame();
    }

    public void OnRestartPressed()
    {
        AudioManager.Instance?.PlayUIClick();
        GameManager.Instance.Restart();
    }

    public void OnQuitPressed()
    {
        AudioManager.Instance?.PlayUIClick();
        GameManager.Instance.QuitGame();
    }

    public void OnMainMenuPressed()
    {
        AudioManager.Instance?.PlayUIClick();
        GameManager.Instance.GoToMainMenu();
    }

    // Called from Victory scene
    public void OnPlayAgainPressed()
    {
        AudioManager.Instance?.PlayUIClick();
        GameManager.Instance.Restart();
    }
}
