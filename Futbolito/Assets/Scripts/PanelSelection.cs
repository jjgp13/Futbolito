using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        if (grandParent == "PlayerUI") MatchInfo._matchInfo.playerLineUp = line;
        else if (grandParent == "ComUI") MatchInfo._matchInfo.comLineUp = line;
    }

    public void SetTime(int time)
    {
        MatchInfo._matchInfo.matchTime = time;
    }

    public void SetDifficulty(int difficulty)
    {
        //1 is easy
        //2 is normal
        //3 is hard
        MatchInfo._matchInfo.difficulty = difficulty;
    }

    public void SetUniform(string uniform)
    {
        string grandParent = transform.parent.gameObject.name;
        if (grandParent == "PlayerUI") MatchInfo._matchInfo.playerUniform = uniform;
        else if (grandParent == "ComUI") MatchInfo._matchInfo.comUniform = uniform;
    }
}
