using UnityEngine;

/// <summary>
/// Scriptable Object with tournament information.
/// </summary>
[CreateAssetMenu(fileName = "New Tournament", menuName = "Tournament")]
public class Tournament : ScriptableObject {

    public string tourName;
    public Sprite cupImage;
    public Team[] teams;

}
