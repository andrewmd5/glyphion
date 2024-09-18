namespace Glyphion.Core.Textures;

/// <summary>
/// Represents a pixel in an ASCII texture with associated brightness and color information.
/// </summary>
public struct AsciiPixel
{
    /// <summary>
    /// Represents the brightness level of an ASCII pixel.
    /// </summary>
    /// <remarks>
    /// Brightness is a floating-point value typically normalized between 0 and 1,
    /// where 0 represents the darkest and 1 represents the brightest.
    /// It is used to determine the corresponding character in an ASCII texture based on its brightness.
    /// </remarks>
    public float Brightness;

    /// <summary>
    /// Represents the RGB color of a single ASCII pixel.
    /// </summary>
    public RgbColor Color;
}