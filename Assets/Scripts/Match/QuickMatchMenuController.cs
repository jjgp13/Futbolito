using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuickMatchMenuController : MonoBehaviour {

    

    //Reference to panel that handles teams
    public GameObject teamsPanel;
    public Button teamButton;

    //Reference to continent buttons and data for teams.
    public Button[] confBtns;
    public Team[] americanTeams;
    public Team[] europeTeams;
    public Team[] africanTeams;
    public Team[] asianTeams;

    public Image mapRegion;
    public Text teamsRegion;

    public Button buttonLeft, buttonRight;
    int begin, end;

    public Button setMatchBtn;
    public GameObject matchSettingMenu;
    public GameObject playerUI, comUI;
    public GameObject notTeamSelectedPanel;

    public GameObject clearTeamSelectionButton;
    public Sprite flagOutline;

    public void SelectedConf(string region)
    {
        teamsRegion.text = region;
        switch (region)
        {
            case "America":
                FillTeamsPanel(americanTeams);
                break;
            case "Europe":
                FillTeamsPanel(europeTeams);
                break;
            case "Africa":
                FillTeamsPanel(africanTeams);
                break;
            case "Asia":
                FillTeamsPanel(asianTeams);
                break;
        }
    }

    void FillTeamsPanel(Team[] teams)
    {
        begin = 0;
        end = 6;
        DeleteTeamsFromPanel();
        for (int i = 0; i < teams.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            newTeam.onClick.AddListener(delegate { ReturnTeamSelected(newTeam.GetComponent<TeamSelected>()); });
            newTeam.GetComponent<TeamSelected>().team = teams[i];
            newTeam.image.sprite = teams[i].flag;
            newTeam.transform.GetChild(0).GetComponent<Text>().text = teams[i].teamName;
            newTeam.transform.SetParent(teamsPanel.transform);
            if(i >= 0 && i < 6) newTeam.gameObject.SetActive(true);
            else newTeam.gameObject.SetActive(false);
        }
    }

    void DeleteTeamsFromPanel()
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void ChangeTeamsInPanel(int bIndex, int eIndex)
    {
        for (int i = 0; i < teamsPanel.transform.childCount; i++)
        {
            if (i >= bIndex && i < eIndex)
            {
                teamsPanel.transform.GetChild(i).gameObject.SetActive(true);
            } else
            {
                teamsPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void ChangeTeamsIndex(string side)
    {
        DeselectPreviousTeams();
        if(teamsPanel.transform.childCount > 0)
        {
            if (side == "left")
            {
                if (begin - 6 < 0)
                {
                    begin = 0;
                    end = begin + 6;
                }
                else
                {
                    end = begin;
                    begin -= 6;
                }
                ChangeTeamsInPanel(begin, end);
            }

            if (side == "right")
            {
                if (end + 6 > teamsPanel.transform.childCount)
                {
                    end = teamsPanel.transform.childCount;
                    begin = end - 6;
                }
                else
                {
                    begin = end;
                    end += 6;
                }
                ChangeTeamsInPanel(begin, end);
            }
        }
    }

    void DeselectPreviousTeams()
    {
        for (int i = 0; i < teamsPanel.transform.childCount; i++)
        {
            TeamSelected teamSelected = teamsPanel.transform.GetChild(i).GetComponent<TeamSelected>();
            if (teamSelected.isSelected)
            {
                teamSelected.isSelected = false;
                teamSelected.DeletePreviousSelected();
            }
        }
    }

    public void ChangeMapSprite(Sprite mapSprite)
    {
        mapRegion.sprite = mapSprite;
    }

    /// <summary>
    /// Get info of the team button that has been pressed and set its info to the match game object info
    /// Set default values.
    /// </summary>
    /// <param name="btnInfo"></param>
    void ReturnTeamSelected(TeamSelected btnInfo)
    {
        if (MatchInfo._matchInfo.leftTeam == null)
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.leftTeam = btnInfo.team;
            
            MatchInfo._matchInfo.leftTeamLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.leftTeamLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.leftTeamLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.leftTeamUniform = "Local";

            //Set UI given team selected
            SetFlags("LeftTeamFlags", btnInfo.team.flag, btnInfo.team.teamName);
            SetUI(playerUI, btnInfo.team);
            //Activate clear selection button
            clearTeamSelectionButton.SetActive(true);
        }
        else
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.rightTeam = btnInfo.team;
            MatchInfo._matchInfo.rightTeamLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.rightTeamLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.rightTeamLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.rightTeamUniform = "Local";

            //Set UI given team selected
            SetFlags("RightTeamFlags", btnInfo.team.flag, btnInfo.team.teamName);
            SetUI(comUI, btnInfo.team);
        }
    }

    //Clear teams selected (Button)
    public void ClearTeamSelection()
    {
        clearTeamSelectionButton.SetActive(false);
        MatchInfo._matchInfo.leftTeam = null;
        MatchInfo._matchInfo.rightTeam = null;
        SetFlags("LeftTeamFlags", flagOutline, "");
        SetFlags("RightTeamFlags", flagOutline, "");
    }

    //On click MatchSettings button it will show the match settings panel
    public void MatchSettingMenuAnimation(bool state)
    {
        if(MatchInfo._matchInfo.leftTeam == null || MatchInfo._matchInfo.rightTeam == null) notTeamSelectedPanel.SetActive(true);
        else
        {
            Animator anim = GetComponent<Animator>();
            anim.SetBool("Show", state);
            MatchInfo._matchInfo.matchTime = 2;
            MatchInfo._matchInfo.matchLevel = 2;
        }
    }

    //On click Team button this will set the UI flags in main panel and match settings panel
    void SetFlags(string tag, Sprite flag, string teamName)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        foreach (var item in flags)
        {
            item.GetComponent<Image>().sprite = flag;
            item.transform.GetChild(1).GetComponent<Text>().text = teamName;
        }
    }

    void SetUI(GameObject parent, Team team)
    {
        //Set Uniforms
        Image local = parent.transform.Find("Uniforms/LocalU/Uniforme").GetComponent<Image>();
        Image visit = parent.transform.Find("Uniforms/VisitU/Uniforme").GetComponent<Image>();
        local.sprite = team.firstU;
        visit.sprite = team.secondU;
        //Set lineup image
        parent.transform.Find("FormationImage").GetComponent<Image>().sprite = team.formationImage;
    }

    public void ChangeScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            Destroy(GameObject.Find("MatchInfo"));
            Destroy(GameObject.FindGameObjectWithTag("PlayerDataObject"));
        }
        SceneManager.LoadScene(sceneName);
    }

    public void HidePanel(GameObject panel)
    {
        panel.SetActive(false);
    }
}
