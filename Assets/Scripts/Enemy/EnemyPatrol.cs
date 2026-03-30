using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform waypointsParent;
    public float moveSpeed;
    public Animator animator;

    [SerializeField] private float scanSweepAngle;
    [SerializeField] private float scanSpeed;

    [Header("FOV")]
    [SerializeField] public Transform sieraTransform;
    [SerializeField] private Transform pfFieldOfView;
    [SerializeField] private float fov;
    [SerializeField] private float viewDistance;
    [SerializeField] private LayerMask wallMask;
    private FieldOfView fieldOfView;

    [Header("Detection")]
    [SerializeField] private float detectionFillTime;
    [SerializeField] private Material undetectedMaterial;
    [SerializeField] private Material detectedMaterial;

    [Header("Audio")]
    [SerializeField] private EnemySoundManager enemySoundManager;

    [Header("Debug")]
    public bool ignoreGameStateForTesting = false;

    private int currentWaypointIndex = 0;
    private float waypointPauseTimer = 0f;
    private bool isPausing = false;
    private Rigidbody2D rb;
    private float detectionProgress = 0f;
    private bool playerVisibleNow = false;
    private WaypointMarker[] waypoints;
    private float scanAngle = 0f;
    private float scanDirection = 1f;
    private Vector3 currentFacingDir;
    private Vector3 patrolFacingDir = Vector3.up;

    private float detectionCooldown = 0f;
    private const float DETECTION_COOLDOWN_TIME = 1f;
    private bool isStunned = false;

    private void Start()
    {
        fieldOfView = Instantiate(pfFieldOfView, null).GetComponent<FieldOfView>();
        fieldOfView.SetFOV(fov);
        fieldOfView.SetViewDistance(viewDistance);

        waypoints = waypointsParent.GetComponentsInChildren<WaypointMarker>();

        if (wallMask.value == 0)
            wallMask = LayerMask.GetMask("Walls");
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("EnemyAI requires a Rigidbody2D component.", this);

        if (rb != null)
        {
            rb.useFullKinematicContacts = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void FixedUpdate()
    {
        if (!ShouldRunAI()) return;

        if (detectionCooldown > 0f)
        {
            detectionCooldown -= Time.fixedDeltaTime;
            resetDetection();
            return;
        }

        Patrol();
        playerVisibleNow = IsPlayerVisible();
        enemySoundManager?.UpdateBreathing(Time.fixedDeltaTime);

        if (playerVisibleNow)
        {
            fieldOfView.GetComponent<MeshRenderer>().material = detectedMaterial;

            if (detectionFillTime <= 0f)
                detectionProgress = 1f;
            else
                detectionProgress += Time.fixedDeltaTime / detectionFillTime;

            if (detectionProgress >= 1f)
                playerCaught();
        }
        else
        {
            resetDetection();
        }
    }

    bool ShouldRunAI()
    {
        if (ignoreGameStateForTesting) return true;
        if (isStunned) return false;
        if (GameManager.Instance == null) return false;
        return GameManager.Instance.CurrentState == GameState.LevelPlaying;
    }

    public void Stun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        if (fieldOfView != null)
            fieldOfView.GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(duration);

        isStunned = false;
        if (fieldOfView != null)
            fieldOfView.GetComponent<MeshRenderer>().enabled = true;
    }

    void Patrol()
    {
        if (rb == null || waypoints == null || waypoints.Length == 0) return;

        if (isPausing)
        {
            waypointPauseTimer -= Time.fixedDeltaTime;
            if (waypointPauseTimer <= 0f)
            {
                isPausing = false;
                scanAngle = 0f;
                scanDirection = 1f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        Transform target = waypoints[currentWaypointIndex].transform;
        Vector2 currentPos = rb.position;
        Vector2 toTarget = (Vector2)target.position - currentPos;

        if (toTarget.sqrMagnitude < 0.000001f)
        {
            ArriveAtWaypoint();
            return;
        }

        float stepDistance = moveSpeed * Time.fixedDeltaTime;
        Vector2 nextPos = Vector2.MoveTowards(currentPos, target.position, stepDistance);
        rb.MovePosition(nextPos);

        if (toTarget.sqrMagnitude > 0.001f)
            patrolFacingDir = toTarget.normalized;

        currentFacingDir = patrolFacingDir;

        enemySoundManager?.UpdateFootsteps(Time.fixedDeltaTime);

        if (Vector2.Distance(nextPos, target.position) < 0.05f)
            ArriveAtWaypoint();
    }

    private void ArriveAtWaypoint()
    {
        if (waypoints[currentWaypointIndex].type == WaypointType.Search)
        {
            isPausing = true;
            scanAngle = 0f;
            scanDirection = 1f;
            waypointPauseTimer = scanSweepAngle * 2f / scanSpeed;
            enemySoundManager?.ResetFootstepTimer();
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private bool IsPlayerVisible()
    {
        if (sieraTransform == null) return false;

        Vector2 origin = GetVisionOrigin();
        Vector2 playerCenter = GetPlayerCenter();
        Vector2 toPlayer = playerCenter - origin;

        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer > viewDistance) return false;
        if (distanceToPlayer <= 0.0001f) return true;

        Vector2 dirToPlayer = toPlayer.normalized;
        if (!IsInsideViewCone(dirToPlayer)) return false;

        return HasLineOfSight(origin);
    }

    private Vector2 GetVisionOrigin()
    {
        Vector2 basePosition = rb != null ? rb.position : (Vector2)transform.position;

        Vector2 facing = currentFacingDir.sqrMagnitude > 0.0001f
            ? ((Vector2)currentFacingDir).normalized
            : ((Vector2)patrolFacingDir).normalized;

        return basePosition + facing * 0.2f;
    }

    private Vector2 GetPlayerCenter()
    {
        Collider2D playerCollider = sieraTransform.GetComponentInChildren<Collider2D>();
        if (playerCollider != null)
            return playerCollider.bounds.center;

        return sieraTransform.position;
    }

    private bool IsInsideViewCone(Vector2 dirToPlayer)
    {
        Vector2 facing = currentFacingDir.sqrMagnitude > 0.0001f
            ? ((Vector2)currentFacingDir).normalized
            : ((Vector2)patrolFacingDir).normalized;

        float minDot = Mathf.Cos(fov * 0.5f * Mathf.Deg2Rad);
        return Vector2.Dot(facing, dirToPlayer) >= minDot;
    }

    private bool HasLineOfSight(Vector2 origin)
    {
        Collider2D playerCollider = sieraTransform.GetComponentInChildren<Collider2D>();
        if (playerCollider == null)
            return Physics2D.Linecast(origin, sieraTransform.position, wallMask).collider == null;

        Bounds bounds = playerCollider.bounds;
        Vector2 center = bounds.center;
        Vector2 upOffset = Vector2.up * bounds.extents.y * 0.5f;
        Vector2 rightOffset = Vector2.right * bounds.extents.x * 0.5f;

        Vector2[] samplePoints =
        {
            center,
            center + upOffset,
            center - upOffset,
            center + rightOffset,
            center - rightOffset
        };

        for (int index = 0; index < samplePoints.Length; index++)
        {
            if (Physics2D.Linecast(origin, samplePoints[index], wallMask).collider == null)
                return true;
        }

        return false;
    }

    private void resetDetection()
    {
        if (fieldOfView != null)
            fieldOfView.GetComponent<MeshRenderer>().material = undetectedMaterial;

        detectionProgress = 0f;
    }

    private void playerCaught()
    {
        Debug.Log("Enemy " + name + " sees Siera! Detection confirmed.");
        SuspicionManager.Instance.AddSuspicion(12f);
        GameManager.Instance.OnLifeLost();
        resetDetection();
        detectionCooldown = DETECTION_COOLDOWN_TIME;
    }

    void LateUpdate()
    {
        if (fieldOfView == null) return;

        fieldOfView.SetOrigin(GetVisionOrigin());

        if (isPausing)
        {
            scanAngle += scanDirection * scanSpeed * Time.deltaTime;
            if (Mathf.Abs(scanAngle) >= scanSweepAngle * 0.5f)
                scanDirection *= -1f;

            currentFacingDir = Quaternion.Euler(0, 0, scanAngle) * patrolFacingDir;
        }
        else
        {
            currentFacingDir = patrolFacingDir;
        }

        fieldOfView.SetFOVDirection(currentFacingDir);

        if (animator != null)
        {
            if (isPausing)
            {
                animator.speed = 0f;
            }
            else
            {
                animator.speed = 1f;
                bool horizontal = Mathf.Abs(currentFacingDir.x) >= Mathf.Abs(currentFacingDir.y);
                animator.SetInteger("right", horizontal && currentFacingDir.x > 0 ? 1 : 0);
                animator.SetInteger("left", horizontal && currentFacingDir.x < 0 ? 1 : 0);
                animator.SetInteger("up", !horizontal && currentFacingDir.y > 0 ? 1 : 0);
                animator.SetInteger("down", !horizontal && currentFacingDir.y < 0 ? 1 : 0);
            }
        }
    }
}
