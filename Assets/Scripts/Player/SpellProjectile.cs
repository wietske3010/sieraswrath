using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    private Transform target;
    private Vector3 direction;
    private float speed;
    private float maxDistance;
    private float stunDuration;
    private Vector3 startPosition;
    private bool initialised = false;

    public void Init(Transform target, Vector3 direction, float speed, float maxDistance, float stunDuration)
    {
        this.target = target;
        this.direction = direction;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.stunDuration = stunDuration;
        startPosition = transform.position;
        initialised = true;
    }

    void Update()
    {
        if (!initialised) return;

        transform.position += direction * speed * Time.deltaTime;

        // Destroy if exceeded max distance
        if (Vector3.Distance(transform.position, startPosition) >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        // Hit check against target
        if (target != null && Vector2.Distance(transform.position, target.position) < 0.6f)
        {
            EnemyPatrol enemy = target.GetComponent<EnemyPatrol>();
            enemy?.Stun(stunDuration);
            Destroy(gameObject);
        }
    }
}
