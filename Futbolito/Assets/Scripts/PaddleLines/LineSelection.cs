using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSelection : MonoBehaviour {

    
    public GameObject line;
    public Sprite[] selectionState;

    // Use this for initialization
    void Start () {
        GetComponent<SpriteRenderer>().sprite = selectionState[0];
        lineSelected(false, 0);
    }

    // Update is called once per frame
    private void OnMouseDown()
    {
        if (!line.GetComponent<LineMovement>().isActive)
        {
            lineSelected(true, 1);
            GetComponentInParent<LinesHandler>().linesSelected.Enqueue(gameObject);
        }
        else
        {
            lineSelected(false, 0);
            GetComponentInParent<LinesHandler>().linesSelected.Dequeue();
        }
    }

    public void lineSelected(bool isSelected, int spriteState)
    {
        GetComponent<SpriteRenderer>().sprite = selectionState[spriteState];
        line.GetComponent<LineMovement>().isActive = isSelected;
        for (int i = 0; i < line.transform.childCount; i++) line.transform.GetChild(i).GetComponent<Animator>().SetBool("holding", isSelected);
    }
}
