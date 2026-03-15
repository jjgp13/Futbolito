using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player-triggered rod bump action. Press Bump input (R / Y button) to nudge
/// a stuck ball away from the closest figure on the active rod.
/// Mirrors the AI RodBumpEffect impulse logic but is input-driven.
/// </summary>
public class PlayerRodBumpAction : MonoBehaviour
{
    [Header("Bump Configuration")]
    [SerializeField] private float bumpCooldown = 1.0f;

    // Physics preset values
    private float bumpStrength = 3f;
    private float maxBumpRange = 3.5f;

    // References
    private PlayerRodMovementAction rodMovement;
    private PlayerInput playerInput;
    private InputAction bumpAction;
    private GameObject ball;
    private Rigidbody2D ballRb;

    // State
    private float lastBumpTime = -10f;

    #region Unity Lifecycle

    private void Awake()
    {
        rodMovement = GetComponent<PlayerRodMovementAction>();

        var teamController = GetComponentInParent<TeamRodsController>();
        if (teamController != null)
        {
            playerInput = teamController.GetPlayerInputForRodActions(gameObject.name);
            if (playerInput != null)
            {
                bumpAction = playerInput.actions["Bump"];
            }
        }
    }

    private void Start()
    {
        if (bumpAction != null)
        {
            bumpAction.performed += OnBumpPressed;
        }
        LoadPresetValues();
        FindBall();
    }

    private void OnEnable()
    {
        MatchController.OnBallSpawned += OnBallSpawned;

        if (bumpAction != null)
        {
            bumpAction.performed -= OnBumpPressed;
            bumpAction.performed += OnBumpPressed;
        }
    }

    private void OnDisable()
    {
        MatchController.OnBallSpawned -= OnBallSpawned;

        if (bumpAction != null)
        {
            bumpAction.performed -= OnBumpPressed;
        }
    }

    #endregion

    #region Input Handling

    private void OnBumpPressed(InputAction.CallbackContext context)
    {
        if (rodMovement == null || !rodMovement.isActive)
            return;

        if (Time.time - lastBumpTime < bumpCooldown)
            return;

        if (ball == null || ballRb == null)
        {
            FindBall();
            if (ball == null) return;
        }

        if (!HasFigureInRange())
            return;

        ExecuteBump();
    }

    #endregion

    #region Bump Execution

    private void ExecuteBump()
    {
        lastBumpTime = Time.time;
        RodBumpEffect.IncrementBumpCount();

        Vector2 figPos = GetClosestFigurePosition();
        Vector2 ballPos = ball.transform.position;
        Vector2 direction = (ballPos - figPos);

        if (direction.sqrMagnitude < 0.01f)
            direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        direction.Normalize();

        direction += new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
        direction.Normalize();

        float force = bumpStrength * ballRb.mass;
        ballRb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    #endregion

    #region Helpers

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

    private void OnBallSpawned()
    {
        FindBall();
        LoadPresetValues();
    }

    #endregion
}
