using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillTeamList : MonoBehaviour {

    public Team[] teamList;
    public Button teamButton;

	// Use this for initialization
	void Start () {
        PopulateList();
	}
	
	void PopulateList()
    {
        for (int i = 0; i < teamList.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            newTeam.image.sprite = teamList[i].flag;
            newTeam.GetComponent<DisplayTeam>().team = teamList[i];
            newTeam.transform.SetParent(gameObject.transform);
        }
    }
}
