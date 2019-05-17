using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettingsPanel : MonoBehaviour
{

    public void ChangeButtonColorWhenSelected(Button uniformButton)
    {
        string parent = uniformButton.transform.parent.name;
        if (parent == "LeftTeam") uniformButton.GetComponent<Image>().color = Color.blue;
        else uniformButton.GetComponent<Image>().color = Color.red;
        uniformButton.interactable = false;
    }

    public void FocusOnFormationButton(Button firstButtonSelected)
    {
        firstButtonSelected.Select();
    }
}
