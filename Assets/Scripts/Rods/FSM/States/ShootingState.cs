using UnityEngine;

/// <summary>
/// Shooting State - Executes the shot
/// 
/// STATE PURPOSE:
/// - Releases the charged shot
/// - Triggers animations and effects
/// - Applies force to ball
/// 
/// BEHAVIOR:
/// 1. Get charge time from previous state
/// 2. Trigger kick animations on figures
/// 3. Prepare shot in FoosballFigureShootAction
/// 4. Transition to cooldown
/// 
/// TRANSITIONS:
/// - To CooldownState: Immediately after shot execution
/// </summary>
public class ShootingState : AIRodState
{
    public ShootingState(AIRodStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ExecuteShot();
    }

    private void ExecuteShot()
    {
        // Get charge time from charging state
        float chargeTime = GetChargeTimeFromPreviousState();

        // Get team side
        TeamSide teamSide = stateMachine.TeamSide;

        // Trigger animations and prepare shots on all figures
        foreach (var figure in stateMachine.Figures)
        {
            if (figure != null)
            {
                figure.TriggerKickAnimation(chargeTime);
            }
        }

        foreach (var shootAction in stateMachine.ShootActions)
        {
            if (shootAction != null)
            {
                shootAction.PrepareShot(0f, teamSide, chargeTime);
            }
        }

        // Transition to cooldown after shot
        stateMachine.ChangeState<CooldownState>();
    }

    /// <summary>
    /// Gets the charge time from AIRodShootAction component
    /// 
    /// UPDATED: Now gets charge time from action component instead of state
    /// </summary>
    private float GetChargeTimeFromPreviousState()
    {
        // Try to get shoot action component
        AIRodShootAction shootAction = stateMachine.gameObject.GetComponent<AIRodShootAction>();
        
        if (shootAction != null)
        {
            return shootAction.GetCurrentChargeTime();
        }

        // Fallback to medium charge
        return stateMachine.MediumShotThreshold * 0.5f;
    }
}
