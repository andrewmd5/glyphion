using System.Runtime.InteropServices;

namespace Glyphion.Core.Textures;

/// <summary>
/// Represents a color in the RGB color space.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RgbColor
{
    /// <summary>
    /// Represents the red component of an RGB color value.
    /// </summary>
    public byte R;

    /// <summary>
    /// Represents the green component of the RGB color model.
    /// </summary>
    public byte G;

    /// <summary>
    /// Represents the blue component of an RGB color value.
    /// A byte value ranging from 0 to 255, where 0 represents no blue intensity and 255 represents full blue intensity.
    /// </summary>
    public byte B;

    /// <summary>
    /// Converts the RGB color to a grayscale value.
    /// The grayscale value is computed as the average of the red, green, and blue components.
    /// </summary>
    /// <returns>
    /// A byte representing the grayscale value of the color.
    /// </returns>
    public byte ToGrayscale()
    {
        return (byte)((R + G + B) / 3);
    }

    /// <summary>
    /// Represents an RGB color using three byte values for red, green, and blue components.
    /// </summary>
    public RgbColor(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }
}