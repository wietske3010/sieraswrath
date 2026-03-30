using UnityEngine;
using UnityEngine.UI;

public class SuspicionBarUI : MonoBehaviour
{
    [Header("Bar")]
    public Image fillImage;

    [Header("Color Thresholds")]
    public Color lowColor = Color.yellow;
    public Color midColor = new Color(1f, 0.5f, 0f); // orange
    public Color highColor = Color.red;

    [Header("Animation")]
    public float fillSpeed = 2f;

    private float targetFill = 0f;

    void Start()
    {
        if (SuspicionManager.Instance != null)
        {
            SuspicionManager.Instance.OnSuspicionChanged += UpdateBar;
            UpdateBar(SuspicionManager.Instance.GetSuspicion());
        }
    }

    void OnDisable()
    {
        if (SuspicionManager.Instance != null)
            SuspicionManager.Instance.OnSuspicionChanged -= UpdateBar;
    }

    void Update()
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
    }

    public void UpdateBar(float suspicion)
    {
        targetFill = suspicion / 100f;

        if (fillImage != null)
        {
            if (targetFill < 0.5f)
                fillImage.color = Color.Lerp(lowColor, midColor, targetFill * 2f);
            else
                fillImage.color = Color.Lerp(midColor, highColor, (targetFill - 0.5f) * 2f);
        }
    }
}
