using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAutomatic : MonoBehaviour {

    private GameObject ball;

    private void Start()
    {
        ball = GameObject.Find("Ball");
    }

    /// <summary>
    /// For automatic controls.
    /// </summary>
    /// 

    private void Update()
    {
        if (ball != null)
        {
            if(transform.position.y < ball.transform.position.y && GetComponentInParent<LinesHandler>().linesSelected.Count < 3)
            {
                lineSelected(true);
                GetComponentInParent<LinesHandler>().linesSelected.Enqueue(gameObject);
            }
            else
            {
                lineSelected(false);
                GetComponentInParent<LinesHandler>().linesSelected.Dequeue();
            }
        }
        else ball = GameObject.FindGameObjectWithTag("Ball");
    }

    public void lineSelected(bool isSelected)
    {
        gameObject.GetComponent<LineMovement>().isActive = isSelected;
        for (int i = 0; i < gameObject.transform.childCount; i++) gameObject.transform.GetChild(i).GetComponent<Animator>().SetBool("holding", isSelected);
    }

}
