using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToursMenuController : MonoBehaviour {

    public Tournament[] tours;
    public GameObject teamsPanel;

    public Button teamButton;

    public Button[] toursBtns;
    public Sprite selectedTourSprite, notSelectedTourSprite;

    public GameObject teamsLayoutObj;
    private GridLayoutGroup teamsLayout;

    public Image tourMapSprite;
    public Sprite[] tourMaps;
    

    // Use this for initialization
    void Start () {
        teamsLayout = teamsLayoutObj.GetComponent<GridLayoutGroup>();
	}

    public void DisplayTeamsOnPanel(int tourIndex)
    {
        DeleteTeamsFromPanel();
        SetTeamsPanel(tours[tourIndex].teams.Length);
        tourMapSprite.sprite = tourMaps[tourIndex];

        Tournament tour = tours[tourIndex];
        for (int i = 0; i < tour.teams.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            Team team = tour.teams[i];
            newTeam.image.sprite = team.flag;
            newTeam.transform.SetParent(teamsPanel.transform);
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
    }

    void SetTeamsPanel(int teamsN)
    {
        if(teamsN == 16)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(128, 104);
            teamsLayout.spacing = new Vector2(55, 15);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else if (teamsN == 12)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(128, 104);
            teamsLayout.spacing = new Vector2(130, 15);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else if (teamsN == 24)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(104, 80);
            teamsLayout.spacing = new Vector2(11, 45);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else
        {
            teamsLayout.padding = new RectOffset(5,5,20,20);
            teamsLayout.cellSize = new Vector2(80, 56);
            teamsLayout.spacing = new Vector2(10, 70);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
    }

    public void MainMenu(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
