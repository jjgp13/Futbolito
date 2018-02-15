using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {

    public Animator animatorController;
    public float timeTouching;
    public int hitForce;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary)
        if (Input.GetKey(KeyCode.S)){
            timeTouching += Time.deltaTime;
            animatorController.SetFloat("timeTouching", timeTouching);
            animatorController.SetBool("touching", true);
        } else {
            animatorController.SetBool("touching", false);
            timeTouching = 0;
            animatorController.SetFloat("timeTouching", timeTouching);
        }
    }
}
