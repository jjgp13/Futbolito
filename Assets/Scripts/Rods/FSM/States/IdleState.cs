using UnityEngine;

/// <summary>
/// Idle State - Rod is inactive or waiting
/// 
/// STATE PURPOSE:
/// - Initial state when rod is not active
/// - Waiting state when rod has no ball nearby
/// - Minimal CPU usage when nothing is happening
/// 
/// TRANSITIONS:
/// - To PositioningState: When rod becomes active AND ball exists
/// </summary>
public class IdleState : AIRodState
{
    public IdleState(AIRodStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Deactivate any active actions
        DeactivateAllActions();
    }

    public override void Update()
    {
        // Check if rod becomes active
        if (IsRodActive() && GetBall() != null)
        {
            // Transition to positioning state (unified active state)
            stateMachine.ChangeState<PositioningState>();
        }
    }

    private void DeactivateAllActions()
    {
        // Stop any charging
        foreach (var figure in stateMachine.Figures)
        {
            if (figure != null)
            {
                figure.SetMagnetState(false);
            }
        }

        // Stop any shoot actions
        foreach (var shootAction in stateMachine.ShootActions)
        {
            if (shootAction != null && shootAction.IsCharging())
            {
                shootAction.StopCharging();
            }
        }
    }
}
