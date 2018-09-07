using UnityEngine;

[CreateAssetMenu(fileName = "New Tournament", menuName = "Tournament")]
public class Tournament : ScriptableObject {

    public string tourName;
    public Sprite cupImage;
    public Team[] teams;

}
