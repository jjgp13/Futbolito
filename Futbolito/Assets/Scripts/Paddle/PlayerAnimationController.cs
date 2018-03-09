using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {

    public Animator animatorController;
    public float timeTouching;

    public float xForce, yForce;

    // Update is called once per frame
    void Update () {
        if (gameObject.GetComponentInParent<LineMovement>().isActive)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary)
            //if (Input.GetKey(KeyCode.Space))
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
