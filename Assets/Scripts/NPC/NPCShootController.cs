using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCShootController : MonoBehaviour {

    public float distanceToShoot;
    public float distanceToAttract;
    private float attractionForce;
    private NPCStats level;

    private void Start()
    {
        distanceToShoot = 1;
        distanceToAttract = 2;
        attractionForce = 0;
        level = transform.parent.GetComponent<NPCStats>();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (GetComponent<NPCLineMovement>().isActive)
        {
            float distanceToBall = GetComponent<NPCLineMovement>().nearDistance;
            if (distanceToBall < distanceToAttract)
            {
                AttractBall(true);
                PlayAttractParticles(true);
            }
            else
            {
                AttractBall(false);
                PlayAttractParticles(false);
            }
            
            if (distanceToBall < distanceToShoot)
                Shoot(true);
            else
                Shoot(false);
        }else
            PlayAttractParticles(false);
    }

    private void Shoot(bool isShooting)
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<Animator>().SetBool("Shoot", isShooting);
    }

    private void AttractBall(bool isAttracting)
    {
        if (isAttracting)
            attractionForce = level.attractionForce;
        else
            attractionForce = 0;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<PointEffector2D>().forceMagnitude = attractionForce;
    }

    private void PlayAttractParticles(bool play)
    {      
        for (int i = 0; i < transform.childCount; i++)
        {
            ParticleSystem attraction = transform.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystem>();
            if (play)
                attraction.Play();
            else
                attraction.Stop();
        }
            
    }
}
