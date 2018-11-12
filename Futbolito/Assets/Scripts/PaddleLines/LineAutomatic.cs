using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAutomatic : MonoBehaviour {


    public void LineSelected(bool isSelected)
    {
        gameObject.GetComponent<LineMovement>().isActive = isSelected;
        for (int i = 0; i < gameObject.transform.childCount; i++) gameObject.transform.GetChild(i).GetComponent<Animator>().SetBool("holding", isSelected);
    }

}
