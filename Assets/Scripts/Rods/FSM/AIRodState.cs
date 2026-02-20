using UnityEngine;

/// <summary>
/// Base abstract class for all AI rod states in the Finite State Machine
/// 
/// PROGRAMMING CONCEPTS USED:
/// - Abstract Class: Provides common functionality while forcing derived classes to implement specific behavior
/// - Polymorphism: All states share the same interface but behave differently
/// - Encapsulation: Each state manages its own logic independently
/// </summary>
public abstract class AIRodState
{
    protected AIRodStateMachine stateMachine;
    protected AIRodMovementAction rodMovement;
    protected GameObject ball;

    /// <summary>
    /// Constructor - initializes the state with a reference to the state machine
    /// </summary>
    public AIRodState(AIRodStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.rodMovement = stateMachine.RodMovement;
    }

    /// <summary>
    /// Called once when entering this state
    /// Override this to initialize state-specific data
    /// </summary>
    public virtual void Enter()
    {
        // Base implementation does nothing
        // Derived classes can override to add entry logic
    }

    /// <summary>
    /// Called every frame while in this state
    /// Override this to implement state-specific behavior
    /// </summary>
    public virtual void Update()
    {
        // Base implementation does nothing
        // Derived classes must override this
    }

    /// <summary>
    /// Called every fixed frame while in this state
    /// Use for physics-related updates
    /// </summary>
    public virtual void FixedUpdate()
    {
        // Base implementation does nothing
        // Derived classes can override if needed
    }

    /// <summary>
    /// Called once when exiting this state
    /// Override this to clean up state-specific data
    /// </summary>
    public virtual void Exit()
    {
        // Base implementation does nothing
        // Derived classes can override to add exit logic
    }

    /// <summary>
    /// Returns the name of this state for debugging
    /// </summary>
    public virtual string GetStateName()
    {
        return this.GetType().Name;
    }

    /// <summary>
    /// Helper method to get or find the ball reference
    /// </summary>
    protected GameObject GetBall()
    {
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
        }
        return ball;
    }

    /// <summary>
    /// Helper method to check if the rod is currently active
    /// </summary>
    protected bool IsRodActive()
    {
        return rodMovement != null && rodMovement.isActive;
    }

    /// <summary>
    /// Helper method to calculate distance to ball from a specific figure
    /// </summary>
    protected float GetDistanceToBall(Transform figureTransform)
    {
        GameObject ballObj = GetBall();
        if (ballObj == null) return float.MaxValue;

        return Vector2.Distance(ballObj.transform.position, figureTransform.position);
    }

    /// <summary>
    /// Helper method to calculate angle to ball from a specific figure
    /// </summary>
    protected float GetAngleToBall(Transform figureTransform)
    {
        GameObject ballObj = GetBall();
        if (ballObj == null) return 180f;
        
        Vector2 direction = ballObj.transform.position - figureTransform.position;
        return Vector2.Angle(direction, figureTransform.up);
    }

    /// <summary>
    /// Helper method to find the closest figure to the ball
    /// Returns the index of the closest figure
    /// </summary>
    protected int FindClosestFigureIndex()
    {
        FoosballFigureAnimationController[] figures = stateMachine.Figures;
        float closestDistance = float.MaxValue;
        int closestIndex = -1;
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                float distance = GetDistanceToBall(figures[i].transform);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }
        }

        return closestIndex;
    }
}
