using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LineUpButton : MonoBehaviour, ISelectHandler
{
    [Header("Reference to team formation image")]
    public Image formationImage;
    [Header("Sprite of the line-Up")]
    public Sprite formationSprite;
    [Header("Next UI element to select")]
    public Button nextButton;

    private string lineUp;
    private string grandParent;

    private void Start()
    {
        grandParent = transform.parent.parent.name;
        lineUp = gameObject.name;
        GetComponent<Button>().onClick.AddListener(delegate
        {
            NextUIElement();
        });
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetLineUp();
        ChangeFormationImage();
    }

    //Change formation sprite given the button pressed
    public void ChangeFormationImage()
    {
        formationImage.sprite = formationSprite;
    }

    public void NextUIElement()
    {
        GetComponent<Button>().interactable = false;
        nextButton.Select();
    }

    //Set teams line up given the str of the button pressed
    public void SetLineUp()
    {
        Formation line = new Formation();
        string[] lineByline = lineUp.Split(new char[] { '-' });
        line.defense = int.Parse(lineByline[0]);
        line.mid = int.Parse(lineByline[1]);
        line.attack = int.Parse(lineByline[2]);

        if (grandParent == "LeftTeam") MatchInfo._matchInfo.leftTeamLineUp = line;
        else if (grandParent == "RightTeam") MatchInfo._matchInfo.rightTeamLineUp = line;
    }

}
