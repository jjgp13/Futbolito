using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelSelection : MonoBehaviour {

    public Sprite pressedSprite, notPressedSprite;
    public List<GameObject> panelChildren;
    public Image formationImage;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int index = i;
            panelChildren.Add(transform.GetChild(i).gameObject);
            panelChildren[i].GetComponent<Button>().onClick.AddListener(delegate { ChangePanelSelection(index); });
        }
    }

    //Change color of the button pressed
    public void ChangePanelSelection(int btnIndex)
    {
        for (int i = 0; i < panelChildren.Count; i++)
        {
            if(i == btnIndex) panelChildren[i].GetComponent<Image>().sprite = pressedSprite;
            else panelChildren[i].GetComponent<Image>().sprite = notPressedSprite;
        }
    }

    //Change formation sprite given the button pressed
    public void ChangeFormationImage(Sprite lineUp)
    {
        formationImage.sprite = lineUp;
    }

    //Set teams line up given the str of the button pressed
    public void SetLineUp(Button lineup)
    {
        string formation = lineup.transform.GetChild(0).GetComponent<Text>().text;
        string grandParent = transform.parent.gameObject.name;
        Formation line = new Formation();
        string[] lineByline = formation.Split(new char[] { '-' });
        line.defense = int.Parse(lineByline[0]);
        line.mid = int.Parse(lineByline[1]);
        line.attack = int.Parse(lineByline[2]);

        if (grandParent == "LeftTeam") MatchInfo._matchInfo.leftTeamLineUp = line;
        else if (grandParent == "RightTeam") MatchInfo._matchInfo.rightTeamLineUp = line;
    }

    public void SetTime(int time)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "TournamentSelectionScene")
            TournamentController._tourCtlr.matchTime = time;
        else if (currentScene.name == "QuickMatchMenu")
            MatchInfo._matchInfo.matchTime = time;
    }

    public void SetDifficulty(int difficulty)
    {
        //1 is easy
        //2 is normal
        //3 is hard
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "TournamentSelectionScene")
            TournamentController._tourCtlr.tourLevel = difficulty;
        else if (currentScene.name == "QuickMatchMenu")
            MatchInfo._matchInfo.matchLevel = difficulty;
    }

    public void SetUniform(string uniform)
    {
        string grandParent = transform.parent.gameObject.name;
        if (grandParent == "LeftTeam") MatchInfo._matchInfo.leftTeamUniform = uniform;
        else if (grandParent == "RightTeam") MatchInfo._matchInfo.rightTeamUniform = uniform;
    }
}
