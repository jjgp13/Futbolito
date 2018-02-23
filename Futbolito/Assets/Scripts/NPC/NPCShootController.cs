using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCShootController : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        if(GetComponent<NPCLineMovement>().nearDistance < 0.5f)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Animator>().SetBool("Shoot", true);
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Animator>().SetBool("Shoot", false);
            }
        }
	}
}
