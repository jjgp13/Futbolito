using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesHandler : MonoBehaviour {

    public Queue<GameObject> linesSelected = new Queue<GameObject>();
    

    // Update is called once per frame
    void Update () {
        if (linesSelected.Count > 2)
        {
            GameObject ins = linesSelected.Dequeue();
            ins.GetComponent<LineSelection>().lineSelected(false, GetComponentInChildren<LineSelection>().lineInactiveSprite);
        }
	}
}
