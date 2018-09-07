using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToursMenuController : MonoBehaviour {

    public Tournament[] tours;
    public GameObject teamsPanel;
    public Image dummyImage;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DisplayTeamsOnPanel(int tourIndex)
    {
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
        if(teamsPanel.transform.childCount > 0)
        {
            for (int i = 0; i < teamsPanel.transform.childCount; i++)
            {
                GameObject.Destroy(teamsPanel.transform.GetChild(i));
            }
        }
    }
}
