using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettingsPanel : MonoBehaviour
{
    [Header("Left team UI elements")]
    public Image leftTeamFlag;
    public Image leftTeamLocalUniform;
    public Image leftTeamVisitorUniform;
    public Text leftTeamName;
    public Image leftTeamFormation;

    [Header("Right team UI elements")]
    public Image rightTeamFlag;
    public Image rightTeamLocalUniform;
    public Image rightTeamVisitorUniform;
    public Text rightTeamName;
    public Image rightTeamFormation;


    [Header("References to UI settings elements")]
    public Image matchBall;
    public Image matchGrass;
    public Image matchTable;
    public Text matchTime;
    public Text matchLevel;

    [Header("Referenes to Settings Panels")]
    public GameObject mainPanel;
    public GameObject ballsPanel;
    public GameObject tablesPanel;
    public GameObject timesPanel;
    public GameObject levelsPanel;

    private void Awake()
    {
        SetTeamInfoInUI();
    }

    private void SetTeamInfoInUI()
    {
        //Set left team info
        leftTeamFlag.sprite = MatchInfo.instance.leftTeam.flag;
        leftTeamLocalUniform.sprite = MatchInfo.instance.leftTeam.firstU;
        leftTeamVisitorUniform.sprite = MatchInfo.instance.leftTeam.secondU;
        leftTeamName.text = MatchInfo.instance.leftTeam.teamName;
        leftTeamFormation.sprite = MatchInfo.instance.leftTeam.formationImage;

        //Set right team info
        rightTeamFlag.sprite = MatchInfo.instance.rightTeam.flag;
        rightTeamLocalUniform.sprite = MatchInfo.instance.rightTeam.firstU;
        rightTeamVisitorUniform.sprite = MatchInfo.instance.rightTeam.secondU;
        rightTeamName.text = MatchInfo.instance.rightTeam.teamName;
        rightTeamFormation.sprite = MatchInfo.instance.rightTeam.formationImage;
    }

    public void ShowHideParentSettingPanel(bool activate)
    {
        mainPanel.SetActive(activate);
    }

    public void ShowInsidePanel(string panel)
    {
        switch (panel)
        {
            case "Balls":
                ballsPanel.SetActive(true);
                break;
            case "Tables":
                tablesPanel.SetActive(true);
                break;
            case "Time":
                timesPanel.SetActive(true);
                break;
            case "Level":
                levelsPanel.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void SelectingBall(Button ballButton)
    {
        //Change info in Match Info Object
        MatchInfo.instance.ballSelected = ballButton.GetComponent<Image>().sprite;
        //Hide parent panel
        ballButton.transform.parent.gameObject.SetActive(false);
        //Hide main settings panel
        ShowHideParentSettingPanel(false);
        //Change image of the ball selected
        matchBall.sprite = ballButton.GetComponent<Image>().sprite;
    }

    public void SelectingTable(Button tableButton)
    {
        Sprite grassPattern = tableButton.GetComponent<Image>().sprite;
        Sprite tableColor = tableButton.transform.GetChild(1).GetComponent<Image>().sprite;
        //Change grass pattern and table color in Match Info Object;
        MatchInfo.instance.grassSelected = grassPattern;
        MatchInfo.instance.tableSelected = tableColor;
        //Hide Parent panel
        tableButton.transform.parent.gameObject.SetActive(false);
        //Hide main settings panel
        ShowHideParentSettingPanel(false);
        //Change table image
        matchGrass.sprite = grassPattern;
        matchTable.sprite = tableColor;
    }

    public void SelectingTime(Button timeButton)
    {
        //Change time in match info object
        string timeString = timeButton.transform.GetChild(0).GetComponent<Text>().text;
        int time = int.Parse(timeString.Substring(0,1));
        MatchInfo.instance.matchTime = time;
        //Hide Parent panel
        timeButton.transform.parent.gameObject.SetActive(false);
        //Hide main settings panel
        ShowHideParentSettingPanel(false);
        //change button time text
        matchTime.text = timeString;
    }

    public void SelectingLevel(Button levelButton)
    {
        //Change time in match info object
        string levelString = levelButton.transform.GetChild(0).GetComponent<Text>().text;
        int level = 2;
        switch (levelString)
        {
            case "Easy":
                level = 1;
                break;
            case "Normal":
                level = 2;
                break;
            case "Hard":
                level = 3;
                break;
        }
        MatchInfo.instance.matchLevel = level;
        //Hide Parent panel
        levelButton.transform.parent.gameObject.SetActive(false);
        //Hide main settings panel
        ShowHideParentSettingPanel(false);
        //change button time text
        matchLevel.text = levelString;
    }
}
