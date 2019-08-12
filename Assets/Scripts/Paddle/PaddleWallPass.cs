using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleWallPass : MonoBehaviour
{

    public string wallPassButton;
    public float wallPassForce;


    private void Start()
    {
        wallPassButton = GetComponentInParent<PaddleController>().wallPassButton;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GetComponentInParent<LineMovement>().isActive)
        {
            GameObject obj = collision.gameObject;
            //If ball is inside of trigger and press wall pass
            if (obj.tag == "Ball" && Input.GetButtonDown(wallPassButton))
            {
                if (!BulletTimeController.instance.inSlowMotion)
                    BulletTimeController.instance.DoSlowMotion(0.25f, 1f);

                
                obj.GetComponent<Rigidbody2D>().velocity *= 0.1f;
                collision.gameObject.GetComponent<BallBehavior>().stopBallParticles.Play();
                
            }
        }
    }
}
