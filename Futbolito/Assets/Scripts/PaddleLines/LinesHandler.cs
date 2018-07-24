using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesHandler : MonoBehaviour {

    public GameObject[] lines;
    public GameObject ball;

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
    }

    // Update is called once per frame
    void Update ()
    {
        if (ball != null)
        {
            GetClosetsLines();
        }
        else ball = GameObject.FindGameObjectWithTag("Ball");
    }

    private void GetClosetsLines()
    {
        int i, j;
        float dis;
        for (i = 1; i < lines.Length; i++)
        {
            dis = Vector2.Distance(lines[i].transform.position, ball.transform.position);
            GameObject temp = lines[i];
            j = i - 1;
            
            while (j >= 0 && Vector2.Distance(lines[j].transform.position, ball.transform.position) > dis)
            {
                lines[j + 1] = lines[j];
                j = j - 1;
            }
            lines[j + 1] = temp;
        }
        lines[0].GetComponent<LineAutomatic>().lineSelected(true);
        lines[1].GetComponent<LineAutomatic>().lineSelected(true);
        lines[2].GetComponent<LineAutomatic>().lineSelected(false);
        lines[3].GetComponent<LineAutomatic>().lineSelected(false);
    }
}
