using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SetAnimations : MonoBehaviour {

    // The name of the sprite sheet to use
    public string teamPicked;
    public string SpriteSheetName;

    // The name of the currently loaded sprite sheet
    private string LoadedSpriteSheetName;

    // The dictionary containing all the sliced up sprites in the sprite sheet
    private Dictionary<string, Sprite> spriteSheet;

    // The Unity sprite renderer so that we don't have to get it multiple times
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    private void Start()
    {
        // Get and cache the sprite renderer for this game object
        spriteRenderer = GetComponent<SpriteRenderer>();

        LoadSpriteSheet();
    }

    // Runs after the animation has done its work
    private void LateUpdate()
    {
        // Check if the sprite sheet name has changed (possibly manually in the inspector)
        if (LoadedSpriteSheetName != SpriteSheetName)
        {
            // Load the new sprite sheet
            LoadSpriteSheet();
        }

        // Swap out the sprite to be rendered by its name
        // Important: The name of the sprite must be the same!
        spriteRenderer.sprite = spriteSheet[spriteRenderer.sprite.name];
    }

    // Loads the sprites from a sprite sheet
    private void LoadSpriteSheet()
    {
        // Load the sprites from a sprite sheet file (png). 
        // Note: The file specified must exist in a folder named Resources
        var sprites = Resources.LoadAll<Sprite>("Teams/"+teamPicked+"/"+SpriteSheetName);
        spriteSheet = sprites.ToDictionary(x => x.name, x => x);

        // Remember the name of the sprite sheet in case it is changed later
        LoadedSpriteSheetName = SpriteSheetName;
    }
}
