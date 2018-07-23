using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LineSelection : MonoBehaviour {

    
    public GameObject line;
    public Sprite lineActiveSprite;
    public Sprite lineInactiveSprite;

    void Start () {
        GetComponent<Image>().sprite = lineInactiveSprite;
        lineSelected(false, lineInactiveSprite);
    }

    public void ChangeLineState()
    {
        if (!line.GetComponent<LineMovement>().isActive)
        {
            lineSelected(true, lineActiveSprite);
            //GetComponentInParent<LinesHandler>().linesSelected.Enqueue(gameObject);
        }
        else
        {
            lineSelected(false, lineInactiveSprite);
            //GetComponentInParent<LinesHandler>().linesSelected.Dequeue();
        }
    }

    public void lineSelected(bool isSelected, Sprite state)
    {
        GetComponent<Image>().sprite = state;
        line.GetComponent<LineMovement>().isActive = isSelected;
        for (int i = 0; i < line.transform.childCount; i++) line.transform.GetChild(i).GetComponent<Animator>().SetBool("holding", isSelected);
    }
}
