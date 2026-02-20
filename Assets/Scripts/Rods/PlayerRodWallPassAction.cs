using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerRodMovementAction))]
public class PlayerRodWallPassAction : MonoBehaviour
{
    [Header("References")]
    private PlayerRodMovementAction rodMovement;
    private PlayerInput playerInput;
    private InputAction wallPassAction;

    [Header("WallPass Configuration")]
    [SerializeField] private float wallPassForce = 10f;

    [Header("Slow Motion Effect")]
    [SerializeField] private bool enableSlowMotion = true;
    [SerializeField] private float slowMotionScale = 0.3f;
    [SerializeField] private float slowMotionDuration = 1.0f;

    private FoosballFigureAnimationController[] figures;
    private FoosballFigureWallPassAction[] wallPassActions;
    private bool isSlowMotionActive = false;

    private void Awake()
    {
        rodMovement = GetComponent<PlayerRodMovementAction>();

        // Get all foosball figures in this rod
        CollectFigures();

        // Get player input from TeamRodsController
        var teamController = GetComponentInParent<TeamRodsController>();
        if (teamController != null)
        {
            playerInput = teamController.GetPlayerInputForRodActions(gameObject.name);

            if (playerInput != null)
            {
                // Set up wall pass action
                wallPassAction = playerInput.actions["WallPass"];
            }
        }
    }

    private void Start()
    {
        if (wallPassAction != null)
        {
            wallPassAction.performed += OnWallPassPressed;
        }
        
        ConfigureFigureWallPass();
    }

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        wallPassActions = new FoosballFigureWallPassAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();
            wallPassActions[i] = child.GetComponent<FoosballFigureWallPassAction>();

            // Add FoosballFigureWallPassAction component if it doesn't exist
            if (wallPassActions[i] == null && figures[i] != null)
            {
                wallPassActions[i] = child.gameObject.AddComponent<FoosballFigureWallPassAction>();
            }
            
            // Configure wall pass force if wall pass action exists
            if (wallPassActions[i] != null)
            {
                wallPassActions[i].wallPassForce = this.wallPassForce;
            }
        }
    }

    private void ConfigureFigureWallPass()
    {
        foreach (var wallPassAction in wallPassActions)
        {
            if (wallPassAction != null)
            {
                wallPassAction.wallPassForce = this.wallPassForce;
            }
        }
    }

    private void OnWallPassPressed(InputAction.CallbackContext context)
    {
        if (!rodMovement.isActive)
        {
            return;
        }

        // Check if any figure can perform a wall pass
        bool wallPassPerformed = false;
        foreach (var wallPassAction in wallPassActions)
        {
            if (wallPassAction != null && wallPassAction.CanPerformWallPass())
            {
                // Trigger slow motion effect before wall pass
                if (enableSlowMotion && !isSlowMotionActive)
                {
                    StartCoroutine(SlowMotionEffect());
                }

                wallPassAction.PerformWallPass();
                AIDebugLogger.Log(gameObject.name, "PLAYER_WALLPASS", "Player executed wall pass");
                wallPassPerformed = true;
                return; // Only perform one wall pass at a time
            }
        }
    }

    private IEnumerator SlowMotionEffect()
    {
        // Prevent multiple slow motion effects from stacking
        if (isSlowMotionActive) yield break;

        isSlowMotionActive = true;

        // Store original time scale
        float originalTimeScale = Time.timeScale;

        // Apply slow motion
        Time.timeScale = slowMotionScale;

        // Wait for the duration (using unscaled time since we changed timeScale)
        yield return new WaitForSecondsRealtime(slowMotionDuration);

        // Restore normal time scale
        Time.timeScale = originalTimeScale;

        isSlowMotionActive = false;
    }

    private void OnEnable()
    {
        if (wallPassAction != null)
        {
            // Register callbacks here
            wallPassAction.performed -= OnWallPassPressed; // Unsubscribe first to prevent duplicates
            wallPassAction.performed += OnWallPassPressed;

            wallPassAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (wallPassAction != null)
        {
            wallPassAction.performed -= OnWallPassPressed;
            wallPassAction.Disable();
        }

        // Reset time scale if this component is disabled during slow motion
        if (isSlowMotionActive)
        {
            Time.timeScale = 1f;
            isSlowMotionActive = false;
        }
    }
}