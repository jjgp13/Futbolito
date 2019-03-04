using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootButton : MonoBehaviour {

    public static ShootButton _shootButton;

    public bool isShooting;
    public float holdingTime;
    public float shootForce;
    public Slider shootSlider;

    private void Awake()
    {
        _shootButton = this;
    }

    // Use this for initialization
    void Start () {
        isShooting = false;
        holdingTime = 0;
	}

    private void Update()
    {
        shootSlider.value = Mathf.Clamp01(holdingTime / 3f);
        if (isShooting)
            if (holdingTime < 3)
            {
                holdingTime += Time.deltaTime;
                shootForce = holdingTime;
            }
        
        if (!isShooting)
            holdingTime = 0;
    }

    public void Holding()
    {
        isShooting = true;
    }

    public void Release()
    {
        isShooting = false;
    }
    
}
