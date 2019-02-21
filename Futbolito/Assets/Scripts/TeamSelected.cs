using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TeamSelected : MonoBehaviour {

    /// <summary>
    /// Prefab of button selection team.
    /// </summary>
    public Team team;
    public Image flagOutline;
    public bool isSelected = false;

    public void SelectTeam()
    {
        //Get scene and depending on that it will act different.
        Scene currentScene = SceneManager.GetActiveScene();

        //When a selection team button appears in tournament selection scene.
        if (currentScene.name == "TournamentSelectionScene")
        {
            TournamentController tc = TournamentController._tourCtlr;
            if (tc != null)
            {
                tc.teamSelected = team.teamName;
                tc.GetPlayerMatches();
            }
            FindObjectOfType<ToursMenuController>().teamSelectedFlag.sprite = team.flag;
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
