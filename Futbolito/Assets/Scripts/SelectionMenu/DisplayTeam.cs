using UnityEngine;
using UnityEngine.UI;

public class DisplayTeam : MonoBehaviour {

    public Team team;

    private GameObject teamPicked;
    private Animator animController;
    private Image flagImage;
    private Image formationImage;
    private Text teamName;

    private GameObject playerPad;
    private GameObject npcPad;

    private void Start()
    {
        animController = GameObject.Find("TeamSelectedUI").GetComponent<Animator>();
        flagImage = GameObject.Find("TeamFlag").GetComponent<Image>();
        formationImage = GameObject.Find("TeamFormation").GetComponent<Image>();
        teamName = GameObject.Find("TeamName").GetComponent<Text>();
        teamPicked = GameObject.Find("playerInfo");

        playerPad = GameObject.Find("PlayerPad");
        npcPad = GameObject.Find("NPCPad");
    }

    public void DisplayTeamInfo()
    {
        animController.Play("SelectedTeamAnim");
        flagImage.sprite = team.flag;
        formationImage.sprite = team.formation;
        teamName.text = team.teamName;
        teamPicked.GetComponent<TeamPickedInfo>().teamPicked = team;
    }

    public void ChangePadAspect()
    {

    }

}
