using UnityEngine;

// Attach to an invisible GameObject with a BoxCollider2D (isTrigger = true) at the level exit
public class LevelExitTrigger : MonoBehaviour
{
    public event System.Action OnExitReached;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnExitReached?.Invoke();
        }
    }
}
