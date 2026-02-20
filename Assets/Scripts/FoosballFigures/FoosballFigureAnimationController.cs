using UnityEngine;

public class FoosballFigureAnimationController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private FoosballFigureMagnetAction magnet;

    [Header("Animation Parameters")]
    [Tooltip("Names of animation parameters to control")]
    public string lineActiveAnimationBool = "IsLineActive";
    public string wallPassAnimationTrigger = "WallPass";
    private const string magnetAnimationBool = "Magnet";
    public string chargeAnimationBool = "IsCharging";
    public string chargeAmountFloat = "ChargeAmount";
    public string shootAnimationBool = "Shoot";

    // State tracking
    private bool isMagnetActive = false;
    private bool isCharging = false;
    private float chargeAmount = 0f;

    private void Awake()
    {
        // Get the animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        magnet = GetComponentInChildren<FoosballFigureMagnetAction>();
    }

    private void Update()
    {
        UpdateLineActiveStatus();
    }

    private void UpdateLineActiveStatus()
    {
        // Get line active state from parent
        var rodMovement = transform.parent.GetComponent<PlayerRodMovementAction>();
        var aiMovement = transform.parent.GetComponent<AIRodMovementAction>();

        bool isActive = false;

        // Check both player and AI movement components
        if (rodMovement != null && rodMovement.enabled)
        {
            isActive = rodMovement.isActive;
        }
        else if (aiMovement != null && aiMovement.enabled)
        {
            isActive = aiMovement.isActive;
        }

        if (animator != null)
        {
            animator.SetBool(lineActiveAnimationBool, isActive);
        }
    }

    private void UpdateAnimationParameters()
    {
        if (animator != null)
        {
            animator.SetBool(magnetAnimationBool, isMagnetActive);
        }
    }

    // Method to start charging shot
    public void StartCharging()
    {
        isCharging = true;
        if (animator != null)
        {
            animator.SetBool(chargeAnimationBool, true);
            animator.SetFloat(chargeAmountFloat, 0f);
        }
    }

    // Method to update charge amount
    public void UpdateChargeAmount(float amount)
    {
        chargeAmount = amount;
        if (animator != null)
        {
            animator.SetFloat(chargeAmountFloat, amount);
        }
    }

    // Method to trigger shoot animation
    public void TriggerShootAnimation(bool active)
    {
        if (animator != null)
        {
            animator.SetBool(shootAnimationBool, active);
        }
    }

    // Methods called by the LineActionHandler or AI components
    public void TriggerKickAnimation(float chargeAmount = 0f)
    {
        // Reset charging state
        isCharging = false;
        if (animator != null)
        {
            animator.SetBool(chargeAnimationBool, false);
            animator.SetFloat(chargeAmountFloat, chargeAmount);
        }
    }

    public void TriggerWallPassAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(wallPassAnimationTrigger);
        }
    }

    public void SetMagnetState(bool active)
    {
        isMagnetActive = active;
        
        // Set animator parameter for magnet animation
        if (animator != null)
        {
            animator.SetBool(magnetAnimationBool, active);
        }

        // Update magnet component
        if (magnet != null)
        {
            if (active)
            {
                magnet.MagnetOn(); 
            }
            else
            {
                magnet.MagnetOff(); 
            }
        }
    }

    // Callback from physics system to detect when ball is in magnetic field
    public void OnBallEnterMagneticField()
    {
        UpdateAnimationParameters();
    }

    // Callback from physics system to detect when ball leaves magnetic field
    public void OnBallExitMagneticField()
    {
        UpdateAnimationParameters();
    }
}
