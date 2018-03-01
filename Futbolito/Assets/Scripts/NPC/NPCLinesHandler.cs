using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLinesHandler : MonoBehaviour {

    public Queue<GameObject> linesSelected = new Queue<GameObject>();
    public GameObject ball;
    public GameObject[] lines = new GameObject[4];

    // Use this for initialization
    void Start () {
        ball = GameObject.Find("Ball");
        for (int i = 0; i < transform.childCount; i++)
        {
            lines[i] = transform.GetChild(i).gameObject;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (ball != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (lines[i].transform.position.y < ball.transform.position.y)
                {
                    SetLineGivenBallPosition(i, false);
                }
                else
                {
                    SetLineGivenBallPosition(i, true);
                }
            }
        }

        if (ball == null) ball = GameObject.FindGameObjectWithTag("Ball");
    }

    void SetLineGivenBallPosition(int child, bool state)
    {
        lines[child].GetComponent<NPCLineMovement>().isActive = state;
        for (int j = 0; j < lines[child].transform.childCount; j++)
        {
            lines[child].transform.GetChild(j).GetComponent<Animator>().SetBool("Active", state);
        }
    }
}
