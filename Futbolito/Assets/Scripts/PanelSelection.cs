using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSelection : MonoBehaviour {

    public Sprite pressedSprite, notPressedSprite;
    public List<GameObject> panelChildren;
    public Image formationImage;
    public MatchInfo matchInfo;

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

    void GetLineUp(string btnText)
    {
        int[] lineup = new int[3];
        string[] lines = btnText.Split(new char[] { '-' });
        for (int i = 0; i < lines.Length; i++)
        {
            lineup[i] = int.Parse(lines[i]);
        }
        if (gameObject.transform.parent.name == "PlayerUI")
        {
            matchInfo.playerLineUp.defense = lineup[0];
            matchInfo.playerLineUp.mid = lineup[1];
            matchInfo.playerLineUp.attack = lineup[2];
        }
        if (gameObject.transform.parent.name == "ComUI")
        {
            matchInfo.comLineUp.defense = lineup[0];
            matchInfo.comLineUp.mid = lineup[1];
            matchInfo.comLineUp.attack = lineup[2];
        }   
    }

    public void ChangeFormationImage(Sprite lineUp)
    {
        formationImage.sprite = lineUp;
    }
}
