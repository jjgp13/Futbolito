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
        //approach
        /*
        for (int i = 0; i < 2; i++)
        {
            // Find the minimum element in unsorted array
            int min_idx = i;
            for (int j = i + 1; j < lines.Length; j++)
                if (GetDistanceToBall(lines[j]) < GetDistanceToBall(lines[min_idx]))
                    min_idx = j;

            // Swap the found minimum element with the first
            // element
            GameObject temp = lines[min_idx];
            lines[min_idx] = lines[i];
            lines[i] = temp;
        }*/
        float ballPos = ball.transform.position.y;
        if (ballPos < -2.1f)
        {
            lines[0].GetComponent<LineAutomatic>().lineSelected(true);
            lines[1].GetComponent<LineAutomatic>().lineSelected(true);
            lines[2].GetComponent<LineAutomatic>().lineSelected(false);
            lines[3].GetComponent<LineAutomatic>().lineSelected(false);
        } else if (ballPos > 1.85f)
        {
            lines[0].GetComponent<LineAutomatic>().lineSelected(false);
            lines[1].GetComponent<LineAutomatic>().lineSelected(false);
            lines[2].GetComponent<LineAutomatic>().lineSelected(true);
            lines[3].GetComponent<LineAutomatic>().lineSelected(true);
        }
        else
        {
            lines[0].GetComponent<LineAutomatic>().lineSelected(false);
            lines[1].GetComponent<LineAutomatic>().lineSelected(true);
            lines[2].GetComponent<LineAutomatic>().lineSelected(true);
            lines[3].GetComponent<LineAutomatic>().lineSelected(false);
        }
    }

    float GetDistanceToBall(GameObject line)
    {
        float dist = Mathf.Abs(ball.transform.position.y - line.transform.position.y);
        return dist;
    }
}
