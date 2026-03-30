using UnityEngine;

public class SuspicionManager : MonoBehaviour
{
    public static SuspicionManager Instance;

    private float suspicion = 0f; // 0-100

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

    public void AddSuspicion(float amount)
    {
        suspicion += amount;
        suspicion = Mathf.Clamp(suspicion, 0f, 100f);
        OnSuspicionChanged?.Invoke(suspicion);
    }

    public float GetSuspicion()
    {
        return suspicion;
    }

    public void Reset()
    {
        suspicion = 0f;
        OnSuspicionChanged?.Invoke(suspicion);
    }

    public event System.Action<float> OnSuspicionChanged;
}
