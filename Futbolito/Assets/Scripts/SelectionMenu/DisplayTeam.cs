using UnityEngine;
using UnityEngine.UI;

public class DisplayTeam : MonoBehaviour {

    public Team team;

    private GameObject playerTeam;
    private GameObject npcTeam;
    private GameObject teams;

    private Animator animController;
    private Image flagImage;
    private Image formationImage;
    private Text teamName;

    private void Start()
    {
        animController = GameObject.Find("TeamSelectedUI").GetComponent<Animator>();
        flagImage = GameObject.Find("TeamFlag").GetComponent<Image>();
        formationImage = GameObject.Find("TeamFormation").GetComponent<Image>();
        teamName = GameObject.Find("TeamName").GetComponent<Text>();
        playerTeam = GameObject.Find("PlayerInfo");
        npcTeam = GameObject.Find("NpcInfo");
        teams = GameObject.Find("Content");
    }

    public void DisplayTeamInfo()
    {
        animController.Play("SelectedTeamAnim");
        flagImage.sprite = team.flag;
        formationImage.sprite = team.formation;
        teamName.text = team.teamName;
        playerTeam.GetComponent<TeamPickedInfo>().teamPicked = team;
        NpcSelectTeam();
    }

    void NpcSelectTeam()
    {
        int teamIndex = Random.Range(0, 32);
        npcTeam.GetComponent<TeamPickedInfo>().teamPicked = teams.GetComponent<FillTeamList>().teamList[teamIndex];
    }

}
