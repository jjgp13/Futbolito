using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This scprit handles the match setup, given the information in the match info object passed in the matchMenuSettings
/// Set table
/// Set grass pattern
/// Set left team and right team
/// </summary>
public class SetMatchController : MonoBehaviour
{
    [Header("Reference to table selected")]
    public SpriteRenderer grassPattern;
    public SpriteRenderer tableColor;

    [Header("Reference to left/right team gameobjects")]
    public GameObject leftTeam;
    public GameObject rightTeam;

    // Start is called before the first frame update
    void Awake()
    {
        //Set table and grass selected
        grassPattern.sprite = MatchInfo._matchInfo.grassSelected;
        tableColor.sprite = MatchInfo._matchInfo.tableSelected;

        //Set team side scripts
        SetTeam(leftTeam, MatchInfo._matchInfo.leftControlsAssigned.Count);
        SetTeam(rightTeam, MatchInfo._matchInfo.rightControlsAssigned.Count);
    }


    /// <summary>
    /// Given the amount of player on each team.
    /// Active or deactive component that will handle the movement of the team lines
    /// </summary>
    /// <param name="team">Left or right team game object</param>
    /// <param name="teamPlayers">Number of players assigned to that team</param>
    private void SetTeam(GameObject team, int teamPlayersCount)
    {
        //If there are at least one player.
        //Lines will be handle by controller input
        if(teamPlayersCount > 0)
        {
            team.GetComponent<LinesHandler>().enabled = true;
            team.GetComponent<NPCLinesHandler>().enabled = false;
        }
        //If not, lines will be handle by AI
        else
        {
            team.GetComponent<LinesHandler>().enabled = false;
            team.GetComponent<NPCLinesHandler>().enabled = true;
        }
    }

}
