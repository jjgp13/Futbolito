using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Team", menuName = "Team")]
public class Team : ScriptableObject {

    public string teamName;

    public Sprite flag;
    public Sprite formation;

    public int defense;
    public int midfield;
    public int attack;

    public string spriteSheetName;

}
