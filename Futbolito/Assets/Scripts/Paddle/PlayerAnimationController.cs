using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour {

    public Animator animatorController;
    public float timeTouching;

    public float xForce, yForce;

    public GameObject shootButton;

    private void Awake()
    {
        shootButton = GameObject.Find("ShootButton");
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (gameObject.GetComponentInParent<LineMovement>().isActive)
        {
            if (shootButton.GetComponent<ShootButton>().isShooting)
            {
                timeTouching += Time.deltaTime;
                animatorController.SetFloat("timeTouching", timeTouching);
                animatorController.SetBool("touching", true);
            }
            else
            {
                animatorController.SetBool("touching", false);
                timeTouching = 0;
                animatorController.SetFloat("timeTouching", timeTouching);
            }
        }

        
    }

}
