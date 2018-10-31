using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuickMatchMenuController : MonoBehaviour {

    //This game ob
    public MatchInfo matchInfo;

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

    public GameObject clearTeamSelection;
    public Sprite flagOutline;
    
    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(matchInfo);
    }


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
        if (MatchInfo._matchInfo.playerTeam == null)
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.playerTeam = btnInfo.team;
            
            MatchInfo._matchInfo.playerLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.playerLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.playerLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.playerUniform = "Local";

            //Set UI given team selected
            SetFlags("PlayerFlags", btnInfo.team.flag, btnInfo.team.teamName);
            SetUI(playerUI, btnInfo.team);
            //Activate clear selection button
            clearTeamSelection.SetActive(true);
        }
        else
        {
            //Set info needed for Match scene
            MatchInfo._matchInfo.comTeam = btnInfo.team;
            MatchInfo._matchInfo.comLineUp.defense = btnInfo.team.teamFormation.defense;
            MatchInfo._matchInfo.comLineUp.mid = btnInfo.team.teamFormation.mid;
            MatchInfo._matchInfo.comLineUp.attack = btnInfo.team.teamFormation.attack;
            MatchInfo._matchInfo.comUniform = "Local";

            //Set UI given team selected
            SetFlags("ComFlags", btnInfo.team.flag, btnInfo.team.teamName);
            SetUI(comUI, btnInfo.team);
        }
    }

    public void ClearTeamSelection()
    {
        MatchInfo._matchInfo.playerTeam = null;
        MatchInfo._matchInfo.comTeam = null;
        clearTeamSelection.SetActive(false);
        SetFlags("PlayerFlags", flagOutline, "");
        SetFlags("ComFlags", flagOutline, "");
    }

    //On click MatchSettings button it will show the match settings panel
    public void MatchSettingMenuAnimation(bool state)
    {
        if(MatchInfo._matchInfo.playerTeam == null || MatchInfo._matchInfo.comTeam == null) notTeamSelectedPanel.SetActive(true);
        else
        {
            clearTeamSelection.SetActive(false);
            Animator anim = matchSettingMenu.GetComponent<Animator>();
            anim.SetBool("Show", state);
            MatchInfo._matchInfo.matchTime = 4;
            MatchInfo._matchInfo.difficulty = 1;
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
        SceneManager.LoadScene(sceneName);
    }

    public void HidePanel(GameObject panel)
    {
        panel.SetActive(false);
    }
}
