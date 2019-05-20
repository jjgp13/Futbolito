using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettingsPanel : MonoBehaviour
{
    [Header("References to UI settings elements")]
    public Image matchBall;
    public Image matchGrass;
    public Image matchTable;
    public Text matchTime;
    public Text matchLevel;

    [Header("Referenes to Settings Panels")]
    public GameObject mainPanel;
    public GameObject ballsPanel;
    public GameObject grassPanel;
    public GameObject tablesPanel;
    public GameObject timesPanel;
    public GameObject levelsPanel;

}
