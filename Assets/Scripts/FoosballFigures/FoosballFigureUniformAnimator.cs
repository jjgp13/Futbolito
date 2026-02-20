using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages team uniform sprites and handles sprite sheet swapping for player animations.
/// </summary>
public class FoosballFigureUniformAnimator : MonoBehaviour
{
    [Header("Uniform Configuration")]
    [Tooltip("The team name (folder name in Resources/Teams)")]
    public string teamPicked;

    [Tooltip("The uniform variant to use (subfolder name)")]
    public string uniform;

    [Header("Debug Info")]
    [SerializeField, Tooltip("Currently loaded uniform variant")]
    private string loadedUniformVariant;

    // The dictionary containing all sprites in the current sprite sheet
    private Dictionary<string, Sprite> spriteSheet;
    private SpriteRenderer spriteRenderer;
    private bool initialized = false;

    private void Awake()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Cache the sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"No SpriteRenderer found on {gameObject.name}");
            return;
        }

        LoadSpriteSheet();
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized) return;

        // Reload sprite sheet if uniform variant has changed
        if (loadedUniformVariant != uniform)
        {
            LoadSpriteSheet();
        }

        // Apply the sprite with matching name from our sprite sheet
        if (spriteRenderer.sprite != null && spriteSheet.ContainsKey(spriteRenderer.sprite.name))
        {
            spriteRenderer.sprite = spriteSheet[spriteRenderer.sprite.name];
        }
        else
        {
            Debug.LogWarning($"Missing sprite: {spriteRenderer.sprite?.name} in uniform {uniform} for team {teamPicked}");
        }
    }

    /// <summary>
    /// Loads the appropriate sprite sheet based on team and uniform selection
    /// </summary>
    public void LoadSpriteSheet()
    {
        if (string.IsNullOrEmpty(teamPicked) || string.IsNullOrEmpty(uniform))
        {
            Debug.LogWarning("Team or uniform name is empty!");
            return;
        }

        string resourcePath = $"Teams/{teamPicked}/{uniform}";

        // Load all sprites from the specified resource path
        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError($"Failed to load sprites from {resourcePath}");
            return;
        }

        // Create dictionary of sprites by name for quick lookup
        spriteSheet = sprites.ToDictionary(sprite => sprite.name, sprite => sprite);
        loadedUniformVariant = uniform;
    }

    /// <summary>
    /// Changes the team uniform at runtime
    /// </summary>
    /// <param name="teamName">The team name</param>
    /// <param name="uniformVariant">The uniform variant</param>
    public void ChangeUniform(string teamName, string uniformVariant)
    {
        teamPicked = teamName;
        uniform = uniformVariant;
        LoadSpriteSheet();
    }
}