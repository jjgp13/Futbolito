using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1)]
public class PlayerRodMovementAction : MonoBehaviour
{

    //Line Speed. Depends on the number of players.
    public float speed;
    public float velocity;
    public bool isActive;
    private int paddlesInLine;

    // Input System variables
    private PlayerInput playerInput;
    private InputAction moveAction;

    void Awake()
    {
        GetPlayerInputDependingOnLineName();

        if (playerInput != null)
        {
            // Get the move action from the action map
            moveAction = playerInput.actions["MoveLine"];

            if (moveAction == null)
            {
                Debug.LogError("MoveLine action not found in the Input Action asset");
            }
        }
        else
        {
            //disable this script
            this.enabled = false;
        }
    }

    void Start()
    {
        // Set speed of line based on number of paddles
        paddlesInLine = GetComponent<RodConfiguration>().rodFoosballFigureCount;
        RodConfigurationSpeed(paddlesInLine);
        velocity = 0;
    }

    void OnEnable()
    {
        // Enable the move action when this component is enabled
        moveAction?.Enable();
    }

    void OnDisable()
    {
        // Disable the move action when this component is disabled
        moveAction?.Disable();
    }

    void LateUpdate()
    {
        if (isActive && moveAction != null)
        {
            MoveLine();
        }
    }

    private void GetPlayerInputDependingOnLineName()
    {
        if(GetComponentInParent<TeamRodsController>() != null && GetComponentInParent<TeamRodsController>().playersAssignedToThisTeamSide > 0)
        {
            if (gameObject.name == "GoalKepperRod" || gameObject.name == "DefenseRod")
                playerInput = GetComponentInParent<TeamRodsController>().defensePlayerInput;
            else if (gameObject.name == "MidfieldRod" || gameObject.name == "AttackerRod")
                playerInput = GetComponentInParent<TeamRodsController>().attackerPlayerInput;
        }
    }

    private void MoveLine()
    {
        // Read the movement value from the input action
        Vector2 movement = moveAction.ReadValue<Vector2>();
        float yMov = movement.y;

        // Apply movement along Y axis
        velocity = yMov * speed;
        transform.Translate(Vector3.up * velocity * Time.deltaTime);

        // Get reference to RodConfiguration component once to improve performance
        RodConfiguration RodConfiguration = GetComponent<RodConfiguration>();

        // Clamp position within the allowed range
        if (transform.position.y < -RodConfiguration.rodMovementLimit + RodConfiguration.halfPlayer)
            transform.position = new Vector2(transform.position.x, -RodConfiguration.rodMovementLimit + RodConfiguration.halfPlayer);
        if (transform.position.y > RodConfiguration.rodMovementLimit - RodConfiguration.halfPlayer)
            transform.position = new Vector2(transform.position.x, RodConfiguration.rodMovementLimit - RodConfiguration.halfPlayer);
    }

    private void RodConfigurationSpeed(int numPlayerInLine)
    {
        // Use FormationPreset speed if active, otherwise fall back to hardcoded defaults
        var preset = GetComponent<RodConfiguration>()?.activeFormationPreset;
        if (preset != null)
        {
            speed = preset.GetPlayerSpeed(numPlayerInLine, GetComponent<RodConfiguration>().rodRole);
            return;
        }

        switch (numPlayerInLine)
        {
            case 1:
                speed = 10f;
                break;
            case 2:
                speed = 8f;
                break;
            case 3:
                speed = 6f;
                break;
            case 4:
                speed = 4f;
                break;
            case 5:
                speed = 2f;
                break;
            default:
                speed = 6f;
                break;
        }
    }
}
