using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSelection : MonoBehaviour {

    public GameObject line;
    public Sprite[] selectionState;

    // Use this for initialization
    void Start () {
        GetComponent<SpriteRenderer>().sprite = selectionState[0];
	}

    // Update is called once per frame
    private void OnMouseDown()
    {
        if (!line.GetComponent<LineMovement>().isActive)
        {
            GetComponent<SpriteRenderer>().sprite = selectionState[1];
            line.GetComponent<LineMovement>().isActive = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = selectionState[0];
            line.GetComponent<LineMovement>().isActive = false;
        }
    }

}
