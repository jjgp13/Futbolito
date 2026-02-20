using UnityEngine;
using System.Collections;

/// <summary>
/// Cooldown State - Prevents immediate action after completing one
/// 
/// STATE PURPOSE:
/// - Prevents AI from spamming actions repeatedly
/// - Adds natural "recovery" time after actions
/// - Makes AI behavior more realistic and less exploitable
/// 
/// PROGRAMMING CONCEPTS:
/// - Temporal Coupling: Time-based state transitions
/// - Rate Limiting: Prevents action spam
/// 
/// BEHAVIOR:
/// 1. Enter cooldown for fixed duration
/// 2. Wait for cooldown to complete
/// 3. Return to positioning state
/// 
/// TRANSITIONS:
/// - To PositioningState: After cooldown duration elapses
/// - To IdleState: If rod becomes inactive during cooldown
/// </summary>
public class CooldownState : AIRodState
{
    private Coroutine cooldownCoroutine;

    public CooldownState(AIRodStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        cooldownCoroutine = stateMachine.StartCoroutine(CooldownSequence());
    }

    public override void Update()
    {
        // Check if rod becomes inactive
        if (!IsRodActive())
        {
            if (cooldownCoroutine != null)
            {
                stateMachine.StopCoroutine(cooldownCoroutine);
            }
            stateMachine.ChangeState<IdleState>();
        }
    }

    public override void Exit()
    {
        if (cooldownCoroutine != null)
        {
            stateMachine.StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
    }

    /// <summary>
    /// Simple cooldown timer
    /// </summary>
    private IEnumerator CooldownSequence()
    {
        // Wait for cooldown duration
        yield return new WaitForSeconds(stateMachine.CooldownDuration);

        // Return to positioning state
        stateMachine.ChangeState<PositioningState>();
    }
}
