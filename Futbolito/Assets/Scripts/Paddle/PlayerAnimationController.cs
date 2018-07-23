using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour {

    public Animator animatorController;

    public float xForce, yForce;

    public ShootButton shootBtn;
    public HoldButton holdBtn;

    private void Awake()
    {
        shootBtn = GameObject.Find("ShootButton").GetComponent<ShootButton>();
        holdBtn = GameObject.Find("HoldBtn").GetComponent<HoldButton>();
    }

    // Update is called once per frame
    void Update () {
        if (gameObject.GetComponentInParent<LineMovement>().isActive)
        {
            if (shootBtn.isShooting)
            {
                animatorController.SetBool("touching", true);
                animatorController.SetFloat("timeTouching", shootBtn.holdingTime);
            }
            else
            {
                animatorController.SetBool("touching", false);
                animatorController.SetFloat("timeTouching", shootBtn.holdingTime);
            }
        }
    }

}
