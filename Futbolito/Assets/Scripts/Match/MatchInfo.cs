using UnityEngine;
using UnityEngine.UI;

public struct lineUp
{
    int defense;
    int med;
    int attack;
}

public class MatchInfo : MonoBehaviour{

    public Team playerTeam;
    public lineUp playerLineUp;

    public Team comTeam;
    public lineUp comLineUp;

    public int matchTime;
    public int difficulty;

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(gameObject);
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
