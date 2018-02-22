using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLineMovement : MonoBehaviour {

    public GameObject ball;
    public float speed;

	// Use this for initialization
	void Start () {
        ball = GameObject.Find("Ball");
	}
	
	// Update is called once per frame
	void Update () {
        //Get nearest Child
        GameObject nearChild = transform.GetChild(0).gameObject;
        float nearDistance = Vector2.Distance(ball.transform.position, transform.GetChild(0).transform.position);
        for (int i = 1; i < transform.childCount; i++)
        {
            float nextDistance = Vector2.Distance(ball.transform.position, transform.GetChild(i).transform.position);
            if (nearDistance > nextDistance)
            {
                nearDistance = nextDistance;
                nearChild = transform.GetChild(i).gameObject;
            }
        }

        if(ball.transform.position.x > nearChild.transform.position.x) transform.Translate(Vector2.right * speed * Time.deltaTime);
        else transform.Translate(Vector2.right * -speed * Time.deltaTime);

        if (transform.position.x < -GetComponent<SetLine>().xLimit + GetComponent<SetLine>().halfPlayer)
            transform.position = new Vector2(-GetComponent<SetLine>().xLimit + GetComponent<SetLine>().halfPlayer, transform.position.y);
        if (transform.position.x > GetComponent<SetLine>().xLimit - GetComponent<SetLine>().halfPlayer)
            transform.position = new Vector2(GetComponent<SetLine>().xLimit - GetComponent<SetLine>().halfPlayer, transform.position.y);

    }

}
