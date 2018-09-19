using UnityEngine;

[System.Serializable]
public class Formation
{
    public int defense, mid, attack;
}

[CreateAssetMenu(fileName = "New Team", menuName = "Team")]
public class Team : ScriptableObject {

    public string teamName;
    public Sprite flag;

    public Sprite formationImage;
    public Formation teamFormation;

    public Sprite firstU;
    public Sprite secondU;

}