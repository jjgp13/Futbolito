using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TeamSelected : MonoBehaviour, ISelectHandler {

    /// <summary>
    /// Prefab of button selection team.
    /// </summary>
    public Team teamInfo;
    public Image flagOutline;
    public bool isSelected = false;
    private Button buttonComponent;
    //Left team or right team
    private string sidePanel;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        sidePanel = transform.parent.name;
    }

    public void OnSelect(BaseEventData eventData)
    {
        ((ISelectHandler)buttonComponent).OnSelect(eventData);
        if (sidePanel == "LeftTeam") SetTeamFlags("LeftTeamFlags");
        if (sidePanel == "RightTeam") SetTeamFlags("RightTeamFlags");
    }

    private void SetTeamFlags(string side)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(side);
        foreach (GameObject flag in flags)
        {
            flag.GetComponent<Image>().sprite = teamInfo.flag;
            flag.transform.GetChild(0).GetComponent<Text>().text = teamInfo.teamName;
        }
    }


    public void SelectTeam()
    {
        GetComponent<Button>().interactable = false;
        //Get scene and depending on that it will act different.
        Scene currentScene = SceneManager.GetActiveScene();

        //When a selection team button appears in tournament selection scene.
        if (currentScene.name == "TournamentSelectionScene")
        {
            TournamentController tc = TournamentController._tourCtlr;
            if (tc != null)
            {
                tc.teamSelected = teamInfo.teamName;
                tc.GetPlayerMatchesInGroupPhase();
            }
            FindObjectOfType<ToursMenuController>().teamSelectedFlag.sprite = teamInfo.flag;
        }

        //Behaviour for outline effect
        if (isSelected) isSelected = false;
        else isSelected = true;

        //Create the outline when is selected.
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
