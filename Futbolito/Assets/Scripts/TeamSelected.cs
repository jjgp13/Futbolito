﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TeamSelected : MonoBehaviour {

    public Team team;
    public Image flagOutline;
    public bool isSelected = false;

    public void SelectTeam()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if(currentScene.name == "TournamentSelectionScene")
        {
            //For tournaments
            GameObject tc = GameObject.Find("TourController");
            if (tc != null) tc.GetComponent<TournamentController>().teamSelected = team.teamName;
            FindObjectOfType<ToursMenuController>().teamSelectedFlag.sprite = team.flag;
        }

        //Behaviour for outline effect
        if (isSelected) isSelected = false;
        else isSelected = true;

        if(isSelected)
        {
            DeletePreviousSelected();
            Image outline = Instantiate(flagOutline);
            outline.transform.SetParent(gameObject.transform);
            outline.transform.position = gameObject.transform.position;
            Vector2 size = gameObject.GetComponent<RectTransform>().sizeDelta;
            int inc = 16;
            outline.rectTransform.sizeDelta = new Vector2(size.x + inc, size.y + inc);
        }
    }

    public void DeletePreviousSelected()
    {
        GameObject[] outlines = GameObject.FindGameObjectsWithTag("FlagOutline");
        foreach (var item in outlines)
        {
            item.GetComponentInParent<TeamSelected>().isSelected = false;
            Destroy(item.gameObject);
        }
    }

}
