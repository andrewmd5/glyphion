namespace Glyphion.Core;

/// <summary>
/// Represents options for configuring the Glyphion engine.
/// </summary>
public class GlyphionEngineOptions
{
    /// <summary>
    /// Controls whether the engine should use colors when rendering text and graphics.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, the engine will render outputs in color.
    /// When set to <c>false</c>, the engine will render outputs in monochrome.
    /// </remarks>
    public bool UseColors { get; set; } = false;

    /// <summary>
    /// Gets or sets the target frames per second (FPS) for the game.
    /// If the value is <c>null</c>, the game will not limit FPS.
    /// </summary>
    public float? TargetFps { get; set; } = null;

    /// <summary>
    /// Gets or sets the title of the game.
    /// The default value is "Glyphion".
    /// </summary>
    public string GameTitle { get; set; } = "Glyphion";
}