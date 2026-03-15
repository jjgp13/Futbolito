using UnityEngine;

/// <summary>
/// Passive stuck ball detection and bump triggering for AI rods.
/// Monitors ball velocity and triggers BumpNudge movement mode on the closest
/// rod when the ball is stuck, applying an impulse to dislodge it before the
/// anti-stall respawn timer (5s) fires.
/// Added automatically by TeamRodsController.AddAIComponents().
/// </summary>
public class RodBumpEffect : MonoBehaviour
{
    [Header("Stuck Detection")]
    [Tooltip("Seconds of low velocity before triggering bump")]
    [SerializeField] private float stuckThreshold = 1.5f;
    [Tooltip("Ball speed below this is considered stuck (matches BallBehavior.minVelocityLimit)")]
    [SerializeField] private float velocityThreshold = 1.0f;

    [Header("Bump Behavior")]
    [Tooltip("Cooldown between bumps on this rod")]
    [SerializeField] private float bumpCooldown = 2.0f;

    [Header("Corner Detection")]
    [Tooltip("X distance from center beyond which ball is near a side wall")]
    [SerializeField] private float wallXThreshold = 13f;
    [Tooltip("Y distance from center beyond which ball is near a top/bottom wall")]
    [SerializeField] private float wallYThreshold = 3.8f;
    [Tooltip("Force multiplier when ball is stuck near a wall")]
    [SerializeField] private float wallBumpForceMultiplier = 1.5f;
    [Tooltip("Force multiplier when ball is stuck in a corner (near 2 walls)")]
    [SerializeField] private float cornerBumpForceMultiplier = 2.0f;

    // Physics preset values
    private float bumpStrength = 3f;
    private float maxBumpRange = 3.5f;

    // Runtime state
    private float stuckTimer;
    private float lastBumpTime = -10f;
    private bool ballIsLive;
    private int consecutiveBumps;

    // References
    private AIRodMovementAction rodMovement;
    private GameObject ball;
    private Rigidbody2D ballRb;

    // Static coordination: only closest rod bumps
    private static RodBumpEffect[] allBumpEffects;

    // Stats tracking
    public static int TotalBumpCount { get; private set; }

    public static void IncrementBumpCount()
    {
        TotalBumpCount++;
    }

    public static void ResetMatchStats()
    {
        TotalBumpCount = 0;
    }

    #region Unity Lifecycle

    private void Awake()
    {
        rodMovement = GetComponent<AIRodMovementAction>();
    }

    private void OnEnable()
    {
        MatchController.OnBallSpawned += OnBallSpawned;
        GolController.OnGoalScored += OnGoalScored;
    }

    private void OnDisable()
    {
        MatchController.OnBallSpawned -= OnBallSpawned;
        GolController.OnGoalScored -= OnGoalScored;
    }

    private void Start()
    {
        if (rodMovement == null)
            rodMovement = GetComponent<AIRodMovementAction>();
        CacheAllBumpEffects();
        LoadPresetValues();
        FindBall();
    }

    private void FixedUpdate()
    {
        if (rodMovement == null)
        {
            rodMovement = GetComponent<AIRodMovementAction>();
            if (rodMovement == null) return;
        }

        if (ball == null || ballRb == null)
        {
            FindBall();
            return;
        }

        // Wait for ball to start moving after spawn
        if (!ballIsLive)
        {
            if (ballRb.linearVelocity.magnitude > velocityThreshold)
                ballIsLive = true;
            else
                return;
        }

        if (BallIsStuck())
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckThreshold && CanBump())
                TriggerBump();
        }
        else
        {
            stuckTimer = 0f;
            consecutiveBumps = 0;
        }
    }

    #endregion

    #region Event Handlers

    private void OnBallSpawned()
    {
        ballIsLive = false;
        stuckTimer = 0f;
        consecutiveBumps = 0;
        FindBall();
        CacheAllBumpEffects();
        LoadPresetValues();
    }

    private void OnGoalScored(string goalTag)
    {
        ballIsLive = false;
        stuckTimer = 0f;
        consecutiveBumps = 0;
    }

    #endregion

    #region Stuck Detection

    private bool BallIsStuck()
    {
        return Mathf.Abs(ballRb.linearVelocity.x) < velocityThreshold &&
               Mathf.Abs(ballRb.linearVelocity.y) < velocityThreshold;
    }

    private bool CanBump()
    {
        if (Time.time - lastBumpTime < bumpCooldown)
            return false;

        if (rodMovement.GetMovementMode() == AIRodMovementAction.MovementMode.BumpNudge)
            return false;

        if (!IsClosestRodToBall())
            return false;

        return true;
    }

    private bool HasFigureInRange()
    {
        return GetClosestFigureDistance() <= maxBumpRange;
    }

    private float GetClosestFigureDistance()
    {
        float closest = float.MaxValue;
        Vector2 ballPos = ball.transform.position;

        for (int i = 0; i < transform.childCount; i++)
        {
            float dist = Vector2.Distance(transform.GetChild(i).position, ballPos);
            if (dist < closest) closest = dist;
        }
        return closest;
    }

    private Vector2 GetClosestFigurePosition()
    {
        float closest = float.MaxValue;
        Vector2 closestPos = transform.position;
        Vector2 ballPos = ball.transform.position;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform fig = transform.GetChild(i);
            float dist = Vector2.Distance(fig.position, ballPos);
            if (dist < closest)
            {
                closest = dist;
                closestPos = fig.position;
            }
        }
        return closestPos;
    }

    private bool IsClosestRodToBall()
    {
        float myDist = GetClosestFigureDistance();
        foreach (var other in allBumpEffects)
        {
            if (other == null || other == this || !other.isActiveAndEnabled)
                continue;
            if (other.ball == null)
                continue;
            if (other.GetClosestFigureDistance() < myDist)
                return false;
        }
        return true;
    }

    #endregion

    #region Bump Execution

    private void TriggerBump()
    {
        lastBumpTime = Time.time;
        stuckTimer = 0f;
        consecutiveBumps++;
        TotalBumpCount++;

        // Trigger visual bump movement on the rod
        rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.BumpNudge);

        // Apply direct impulse to dislodge ball
        ApplyBumpForce();

        Vector2 ballPos = ball.transform.position;
        bool inCorner = IsInCorner(ballPos);
        bool nearWall = IsNearWall(ballPos);

        AIDebugLogger.Log(gameObject.name, "BUMP",
            $"dist:{GetClosestFigureDistance():F1} force:{bumpStrength:F1} total:{TotalBumpCount} " +
            $"consecutive:{consecutiveBumps} corner:{inCorner} wall:{nearWall} pos:({ballPos.x:F1},{ballPos.y:F1})");
    }

    private void ApplyBumpForce()
    {
        Vector2 ballPos = ball.transform.position;
        Vector2 figPos = GetClosestFigurePosition();
        bool nearWall = IsNearWall(ballPos);
        bool inCorner = IsInCorner(ballPos);

        Vector2 direction;

        if (inCorner || (nearWall && consecutiveBumps >= 2))
        {
            // Corner or repeated wall stuck: push toward field center
            direction = ((Vector2)Vector3.zero - ballPos).normalized;
            // Bias slightly toward the horizontal center (helps escape side walls)
            direction.x *= 1.3f;
            direction.Normalize();
        }
        else if (nearWall)
        {
            // Near a single wall: push away from the nearest wall
            direction = Vector2.zero;
            if (Mathf.Abs(ballPos.x) > wallXThreshold)
                direction.x = -Mathf.Sign(ballPos.x);
            if (Mathf.Abs(ballPos.y) > wallYThreshold)
                direction.y = -Mathf.Sign(ballPos.y);
            if (direction.sqrMagnitude < 0.01f)
                direction = ((Vector2)Vector3.zero - ballPos).normalized;
            direction.Normalize();
        }
        else
        {
            // Open field: push away from figure (original behavior)
            direction = (ballPos - figPos);
            if (direction.sqrMagnitude < 0.01f)
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            direction.Normalize();
        }

        // Add slight randomness to prevent predictable patterns
        direction += new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
        direction.Normalize();

        // Calculate force with escalation for stuck situations
        float forceMultiplier = 1f;
        if (inCorner)
            forceMultiplier = cornerBumpForceMultiplier;
        else if (nearWall)
            forceMultiplier = wallBumpForceMultiplier;

        // Escalate force after repeated bumps (caps at 3x base)
        if (consecutiveBumps > 3)
            forceMultiplier *= Mathf.Min(1f + (consecutiveBumps - 3) * 0.25f, 3f);

        float force = bumpStrength * forceMultiplier * ballRb.mass;
        ballRb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    #endregion

    #region Corner Detection

    private bool IsNearWall(Vector2 pos)
    {
        return Mathf.Abs(pos.x) > wallXThreshold || Mathf.Abs(pos.y) > wallYThreshold;
    }

    private bool IsInCorner(Vector2 pos)
    {
        return Mathf.Abs(pos.x) > wallXThreshold && Mathf.Abs(pos.y) > wallYThreshold;
    }

    #endregion

    #region Setup

    private void FindBall()
    {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (var b in balls)
        {
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                ball = b;
                ballRb = rb;
                return;
            }
        }
        ball = null;
        ballRb = null;
    }

    private void LoadPresetValues()
    {
        var manager = FindFirstObjectByType<PhysicsPresetManager>();
        if (manager != null && manager.activePreset != null)
        {
            bumpStrength = manager.activePreset.bumpStrength;
            maxBumpRange = manager.activePreset.maxBumpRange;
        }
    }

    private static void CacheAllBumpEffects()
    {
        allBumpEffects = FindObjectsByType<RodBumpEffect>(FindObjectsSortMode.None);
    }

    #endregion
}
