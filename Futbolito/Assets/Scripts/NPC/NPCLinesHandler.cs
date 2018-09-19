using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLinesHandler : MonoBehaviour {

    GameObject ball;
    GameObject[] lines = new GameObject[4];

    // Use this for initialization
    void Start () {
        ball = GameObject.Find("Ball");
        for (int i = 0; i < transform.childCount; i++) lines[i] = transform.GetChild(i).gameObject;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (ball != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                GetClosetsLines();
            }
        } else ball = GameObject.FindGameObjectWithTag("Ball");
    }

    private void GetClosetsLines()
    {
        float ballPos = ball.transform.position.y;
        if (ballPos > 4.75f)
            ActivateLines(new bool[] { true, false, false, false });
        else if (ballPos > 2.1f)
            ActivateLines(new bool[] { true, true, false, false });
        else if (ballPos < -1.85f)
            ActivateLines(new bool[] { false, false, true, true });
        else
            ActivateLines(new bool[] { false, true, true, false });
    }

    void ActivateLines(bool[] conf)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].GetComponent<NPCLineMovement>().isActive = conf[i];
            for (int j = 0; j < lines[i].transform.childCount; j++)
                lines[i].transform.GetChild(j).GetComponent<Animator>().SetBool("Active", conf[i]);
        }
    }
}
