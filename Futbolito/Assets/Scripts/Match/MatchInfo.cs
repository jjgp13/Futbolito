using UnityEngine;
using UnityEngine.UI;

public class MatchInfo : MonoBehaviour {

    public static MatchInfo _matchInfo;
    public Team playerTeam;
    public int[] playerLineUp;
    public string playerUniform;

    public Team comTeam;
    public int[] comLineUp;
    public string comUniform;

    public int matchTime;
    public int difficulty;

    private void Awake()
    {
        _matchInfo = this;
    }

    public void SetFlags(string tag, Sprite flag, string teamName)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        foreach (var item in flags)
        {
            item.GetComponent<Image>().sprite = flag;
            item.transform.GetChild(1).GetComponent<Text>().text = teamName;
        }
    }
}