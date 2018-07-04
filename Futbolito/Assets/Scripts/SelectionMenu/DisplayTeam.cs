using UnityEngine;
using UnityEngine.UI;

public class DisplayTeam : MonoBehaviour {

    public Team team;

    public Animator animController;
    public Image flagImage;
    public Image formationImage;
    public Text teamName;

    private void Start()
    {
        animController = GameObject.Find("TeamSelectedUI").GetComponent<Animator>();
        flagImage = GameObject.Find("TeamFlag").GetComponent<Image>();
        formationImage = GameObject.Find("TeamFormation").GetComponent<Image>();
        teamName = GameObject.Find("TeamName").GetComponent<Text>();
    }

    public void DisplayTeamInfo()
    {
        animController.Play("SelectedTeamAnim");
        flagImage.sprite = team.flag;
        formationImage.sprite = team.formation;
        teamName.text = team.teamName;
    }

}
