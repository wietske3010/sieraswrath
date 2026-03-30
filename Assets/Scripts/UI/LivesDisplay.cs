using UnityEngine;
using UnityEngine.UI;

public class LivesDisplay : MonoBehaviour
{
    [Header("Life Icons")]
    public Image[] lifeIcons; // 5 icons, assign in Inspector

    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] == null) continue;
            lifeIcons[i].gameObject.SetActive(i < currentLives);
        }
    }
}
