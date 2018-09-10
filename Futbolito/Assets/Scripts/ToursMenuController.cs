using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToursMenuController : MonoBehaviour {

    public Tournament[] tours;
    public GameObject teamsPanel;
    public Image dummyImage;

    public Button[] toursBtns;
    public Sprite selectedTourSprite, notSelectedTourSprite;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DisplayTeamsOnPanel(int tourIndex)
    {
        DeleteTeamsFromPanel();
        Tournament tour = tours[tourIndex];
        for (int i = 0; i < tour.teams.Length; i++)
        {
            Image newFlag = Instantiate(dummyImage);
            Team team = tour.teams[i];
            newFlag.sprite = team.flag;
            newFlag.transform.SetParent(teamsPanel.transform);
        }
    }

    void DeleteTeamsFromPanel()
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ChangeButtonSprite(int index) {
        for (int i = 0; i < toursBtns.Length; i++)
        {
            if (index == i) toursBtns[index].image.sprite = selectedTourSprite;
            else toursBtns[i].image.sprite = notSelectedTourSprite;
        }
        //toursBtns[index].image.sprite = selectedTourSprite;
    }
}
