using UnityEngine;
using UnityEngine.UI;

public class DisplayTeam : MonoBehaviour {

    public Team team;

    public Animator animController;
    public Image flagImage;
    public Image formationImage;
    public Text teamName;

	public void DisplayTeamInfo()
    {
        animController.Play("SelectedTeamAnim");
        flagImage.sprite = team.flag;
        formationImage.sprite = team.formation;
        teamName.text = team.teamName;
    }

}
