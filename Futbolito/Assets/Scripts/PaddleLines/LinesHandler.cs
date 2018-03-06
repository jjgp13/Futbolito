using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesHandler : MonoBehaviour {

    public Queue<GameObject> linesSelected = new Queue<GameObject>();
    GameObject ball;
    GameObject[] lines = new GameObject[4];

    private void Start()
    {
        ball = GameObject.Find("Ball");
        for (int i = 0; i < transform.childCount; i++) lines[i] = transform.GetChild(i).gameObject;
    }

    // Update is called once per frame
    void Update () {
        if (ball == null) ball = GameObject.FindGameObjectWithTag("Ball");

        

        if (linesSelected.Count > 2)
        {
            GameObject ins = linesSelected.Dequeue();
            ins.GetComponent<LineSelection>().lineSelected(false, 0);
        }
	}
}
