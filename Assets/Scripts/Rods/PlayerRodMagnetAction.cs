using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerRodMovementAction))]
public class PlayerRodMagnetAction : MonoBehaviour
{
    [Header("References")]
    private PlayerRodMovementAction rodMovement;
    private PlayerInput playerInput;
    private InputAction magnetAction;

    [Header("Magnet Configuration")]
    [SerializeField] private float attractionForce = -10f;
    [SerializeField] private float maxMagnetTime = 3f;
    
    private bool isMagnetActive = false;
    public float magnetTimeRemaining;
    private FoosballFigureAnimationController[] figures;
    private FoosballFigureMagnetAction[] magnetActions;

    /// <summary>
    /// Returns true if magnet is currently active
    /// Used by PlayerRodShootAction to lock shooting while magnet is active
    /// </summary>
    public bool IsMagnetActive => isMagnetActive;

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
                // Set up magnet action
                magnetAction = playerInput.actions["Magnet"];
                if (magnetAction == null)
                {
                    Debug.LogError($"[{gameObject.name}] Could not find 'Magnet' action in player input actions!");
                }
            }
        }
    }

    private void Start()
    {
        // Initialize magnet time
        magnetTimeRemaining = maxMagnetTime;
        
        ConfigureFigureMagnets();
    }

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        magnetActions = new FoosballFigureMagnetAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();
            
            // Get or add magnet action component
            magnetActions[i] = child.GetComponentInChildren<FoosballFigureMagnetAction>();
            
            // Configure attraction force if magnet action exists
            if (magnetActions[i] != null)
            {
                magnetActions[i].attractionForce = this.attractionForce;
            }
        }
    }

    private void ConfigureFigureMagnets()
    {
        foreach (var magnetAction in magnetActions)
        {
            if (magnetAction != null)
            {
                magnetAction.attractionForce = this.attractionForce;
            }
        }
    }

    private void Update()
    {
        if (isMagnetActive && rodMovement.isActive)
        {
            // Track magnet usage time
            magnetTimeRemaining -= Time.deltaTime;
            
            // Deactivate if time runs out
            if (magnetTimeRemaining <= 0)
            {
                DeactivateMagnet();
            }
        }
        else if (!isMagnetActive && magnetTimeRemaining < maxMagnetTime)
        {
            // Recharge magnet when not in use
            magnetTimeRemaining = Mathf.Min(magnetTimeRemaining + (Time.deltaTime * 0.5f), maxMagnetTime);
        }
    }

    private void OnMagnetPressed(InputAction.CallbackContext context)
    {
        if (!rodMovement.isActive || magnetTimeRemaining <= 0) return;
        
        // Activate magnet on all figures
        isMagnetActive = true;

        AIDebugLogger.Log(gameObject.name, "PLAYER_MAGNET_ON", "Player activated magnet");

        foreach (var figure in figures)
        {
            if (figure != null)
            {
                figure.SetMagnetState(true);
            }
        }
    }

    private void OnMagnetReleased(InputAction.CallbackContext context)
    {
        if (!rodMovement.isActive) return;
        
        DeactivateMagnet();
    }
    
    private void DeactivateMagnet()
    {
        if (isMagnetActive)
            AIDebugLogger.Log(gameObject.name, "PLAYER_MAGNET_OFF", "Player deactivated magnet");

        isMagnetActive = false;
        
        // Deactivate magnet on all figures
        foreach (var figure in figures)
        {
            if (figure != null)
            {
                figure.SetMagnetState(false);
            }
        }
    }

    private void OnEnable()
    {
        if (magnetAction != null)
        {
            // Register callbacks here
            magnetAction.performed -= OnMagnetPressed; // Unsubscribe first to prevent duplicates
            magnetAction.canceled -= OnMagnetReleased;

            magnetAction.performed += OnMagnetPressed;
            magnetAction.canceled += OnMagnetReleased;

            magnetAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (magnetAction != null)
        {
            magnetAction.performed -= OnMagnetPressed;
            magnetAction.canceled -= OnMagnetReleased;
            magnetAction.Disable();
        }
        
        // Make sure magnet is deactivated
        DeactivateMagnet();
    }
}
