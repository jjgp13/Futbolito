using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniformButton : MonoBehaviour
{
    [Header("Next Element in the UI to select")]
    public Button nextButtonToSelected;
    //Button component in this GameObject
    private Button thisButton;

    private void Start()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(delegate { ChangeButtonColorWhenSelected(); });
    }

    /// <summary>
    /// Change color of the button selected given the team side
    /// Left: Blue ... Right: Red
    /// </summary>
    /// <param name="uniformButton">Button selected</param>
    public void ChangeButtonColorWhenSelected()
    {
        string parent = thisButton.transform.parent.name;
        if (parent == "LeftTeam")
        {
            thisButton.GetComponent<Image>().color = Color.blue;
            MatchInfo._matchInfo.leftTeamUniform = gameObject.name;
        }
        else
        {
            thisButton.GetComponent<Image>().color = Color.red;
            MatchInfo._matchInfo.rightTeamUniform = gameObject.name;
        }
            
        thisButton.interactable = false;
        FocusOnFormationButton();
    }

    /// <summary>
    /// Next element to Select in Menu
    /// </summary>
    private void FocusOnFormationButton()
    {
        nextButtonToSelected.Select();
    }
}
