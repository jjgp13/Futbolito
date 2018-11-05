using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesHandler : MonoBehaviour {

    GameObject[] lines = new GameObject[4];
    public GameObject ball;

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        for (int i = 0; i < transform.childCount; i++) lines[i] = transform.GetChild(i).gameObject;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (ball != null)
        {
            GetClosetsLines();
        }
        else ball = GameObject.FindGameObjectWithTag("Ball");
    }

    private void GetClosetsLines()
    {
        float ballPos = ball.transform.position.y;
        if (ballPos < -4.6f)
            ActivateLines(new bool[] {true, false, false, false});
        else if (ballPos < -2.1f)
            ActivateLines(new bool[] { true, true, false, false });
        else if (ballPos > 1.85f)
            ActivateLines(new bool[] { false, false, true, true});
        else
            ActivateLines(new bool[] { false, true, true, false });
    }

    void ActivateLines(bool[] conf)
    {
        for (int i = 0; i < lines.Length; i++) lines[i].GetComponent<LineAutomatic>().lineSelected(conf[i]);
    }
}
