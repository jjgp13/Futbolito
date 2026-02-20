using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerRodMovementAction))]
public class PlayerRodShootAction : MonoBehaviour
{
    [Header("References")]
    private PlayerRodMovementAction rodMovement;
    private PlayerRodMagnetAction magnetAction;
    private PlayerInput playerInput;
    private InputAction shootAction;

    [Header("Shot Configuration")]
    [SerializeField] private float maxChargeTime = 6f; // INCREASED: Allow charging beyond 3 seconds

    // Shot timing thresholds - UPDATED for 2-level system
    // Soft/Quick: ChargeAmount > 0
    // Heavy/Strong: ChargeAmount > 3
    [SerializeField] private float softShotThreshold = 0.1f;   // Just above 0 = soft shot starts
    [SerializeField] private float heavyShotThreshold = 3.0f;  // At 3 seconds = heavy shot starts

    // Shot level indicators for debugging and feedback
    private bool isSoftShotReady = false;
    private bool isHeavyShotReady = false;

    private float currentShotCharge = 0f;
    private bool isCharging = false;
    private bool wasRodActive = false;
    private FoosballFigureAnimationController[] figures;
    private FoosballFigureShootAction[] shootActions;

    private void OnEnable()
    {
        if (shootAction != null)
        {
            // Register callbacks here
            shootAction.performed -= OnShootStarted; // Unsubscribe first to prevent duplicates
            shootAction.canceled -= OnShootReleased;

            shootAction.performed += OnShootStarted;
            shootAction.canceled += OnShootReleased;

            shootAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.performed -= OnShootStarted;
            shootAction.canceled -= OnShootReleased;
            shootAction.Disable();
        }

        // Reset charging state
        isCharging = false;
        currentShotCharge = 0f;
        ResetShotLevelIndicators();
        
        // Stop charging and particles on all figures
        StopAllFiguresCharging();
        ForceStopAllFigureParticles();
    }

    private void ResetShotLevelIndicators()
    {
        isSoftShotReady = false;
        isHeavyShotReady = false;
    }

    private void Awake()
    {
        rodMovement = GetComponent<PlayerRodMovementAction>();
        magnetAction = GetComponent<PlayerRodMagnetAction>();

        // Get all foosball figures in this rod
        CollectFigures();

        // Get player input from TeamRodsController
        var teamController = GetComponentInParent<TeamRodsController>();
        if (teamController != null)
        {
            playerInput = teamController.GetPlayerInputForRodActions(gameObject.name);

            if (playerInput != null)
            {
                // Set up shoot action
                shootAction = playerInput.actions["Shoot"];
                if (shootAction == null)
                {
                    Debug.LogError($"[{gameObject.name}] Could not find 'Shoot' action in player input actions!");
                }
            }
        }
    }

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        shootActions = new FoosballFigureShootAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            // Get figure controller
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();

            // Get or add shoot action component
            shootActions[i] = child.GetComponent<FoosballFigureShootAction>();
            if (shootActions[i] == null && figures[i] != null)
            {
                shootActions[i] = child.gameObject.AddComponent<FoosballFigureShootAction>();
            }
        }
    }

    private void Update()
    {
        if (shootAction == null)
        {
            return;
        }

        // Check if rod just became inactive - stop all particles
        if (wasRodActive && !rodMovement.isActive)
        {
            OnRodBecameInactive();
        }
        wasRodActive = rodMovement.isActive;

        if (isCharging && rodMovement.isActive)
        {
            // Increment charge time
            currentShotCharge = Mathf.Min(currentShotCharge + Time.deltaTime, maxChargeTime);

            // Update all figures with the raw charge time (not normalized)
            // The animator will handle the thresholds internally
            for (int i = 0; i < figures.Length; i++)
            {
                if (figures[i] != null)
                {
                    // We pass the raw charge time value - not normalized
                    figures[i].UpdateChargeAmount(currentShotCharge);
                }

                // Update charge time on shoot action for particle control
                if (shootActions[i] != null)
                {
                    shootActions[i].UpdateChargeTime(currentShotCharge);
                }
            }

            // Track shot level transitions for feedback
            UpdateShotLevelStatus();
        }
    }

    /// <summary>
    /// Called when rod becomes inactive - stops all charging and particles
    /// </summary>
    private void OnRodBecameInactive()
    {
        if (isCharging)
        {
            isCharging = false;
            currentShotCharge = 0f;
            ResetShotLevelIndicators();
        }
        
        StopAllFiguresCharging();
        ForceStopAllFigureParticles();
    }

    private void UpdateShotLevelStatus()
    {
        // Soft shot transition (ChargeAmount > 0)
        if (currentShotCharge >= softShotThreshold && !isSoftShotReady)
        {
            isSoftShotReady = true;
            // Could trigger feedback here (e.g., light haptic, UI indicator)
        }

        // Heavy shot transition (ChargeAmount > 3)
        if (currentShotCharge > heavyShotThreshold && !isHeavyShotReady)
        {
            isHeavyShotReady = true;
            // Could trigger feedback here (e.g., strong haptic, UI indicator change)
        }
    }

    /// <summary>
    /// Checks if shooting is allowed (not blocked by magnet action)
    /// </summary>
    private bool CanShoot()
    {
        // Block shooting if magnet action is active
        if (magnetAction != null && magnetAction.IsMagnetActive)
        {
            return false;
        }
        return true;
    }

    private void OnShootStarted(InputAction.CallbackContext context)
    {
        if (!rodMovement.isActive) return;
        if (!CanShoot()) return;

        // Start charging
        isCharging = true;
        currentShotCharge = 0f;
        ResetShotLevelIndicators();

        AIDebugLogger.Log(gameObject.name, "PLAYER_CHARGE_START", "Player started charging shot");

        // Update all figures - start charging animation (particles will start at threshold)
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                figures[i].StartCharging();
            }

            if (shootActions[i] != null)
            {
                shootActions[i].StartCharging();
            }
        }
    }

    private void OnShootReleased(InputAction.CallbackContext context)
    {
        if (!rodMovement.isActive || !isCharging) return;

        // Get final charge time without normalizing
        float finalChargeTime = currentShotCharge;

        AIDebugLogger.Log(gameObject.name, "PLAYER_SHOOT", $"Player shot! charge:{finalChargeTime:F2}s");

        // Get team side for shot direction
        TeamSide teamSide = GetComponentInParent<TeamRodsController>().teamSide;

        // Update all figures - trigger kick animation and prepare shots
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                // Trigger kick animation - pass the raw charge time, not normalized
                figures[i].TriggerKickAnimation(finalChargeTime);
            }

            // Prepare shot in the figure's shoot action component
            if (shootActions[i] != null)
            {
                shootActions[i].PrepareShot(0f, teamSide, finalChargeTime);
            }
        }

        // Reset charging state
        isCharging = false;
        currentShotCharge = 0f;
        ResetShotLevelIndicators();
    }

    private void StopAllFiguresCharging()
    {
        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].StopCharging();
            }
        }
    }

    /// <summary>
    /// Force stops all particles on all figures
    /// </summary>
    private void ForceStopAllFigureParticles()
    {
        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].ForceStopAllParticles();
            }
        }
    }
}
