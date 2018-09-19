using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSelection : MonoBehaviour {

    public Sprite pressedSprite, notPressedSprite;
    public List<GameObject> panelChildren;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int index = i;
            panelChildren.Add(transform.GetChild(i).gameObject);
            panelChildren[i].GetComponent<Button>().onClick.AddListener(delegate { ChangePanelSelection(index); });
        }
    }

    public void ChangePanelSelection(int btnIndex)
    {
        for (int i = 0; i < panelChildren.Count; i++)
        {
            if(i == btnIndex) panelChildren[i].GetComponent<Image>().sprite = pressedSprite;
            else panelChildren[i].GetComponent<Image>().sprite = notPressedSprite;
        }
    }
}
