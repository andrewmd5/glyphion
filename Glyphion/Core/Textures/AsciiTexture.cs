namespace Glyphion.Core.Textures;

/// <summary>
/// Represents a texture composed of ASCII characters, with each pixel
/// represented as an AsciiPixel object. The texture is defined by a
/// width and a height and contains methods to retrieve specific ASCII
/// characters based on coordinates and brightness.
/// </summary>
public class AsciiTexture
{
    /// <summary>
    /// Gets the array of <see cref="AsciiPixel"/> representing the texture's pixel data.
    /// </summary>
    public AsciiPixel[] Pixels { get;  }

    /// <summary>
    /// Gets the width of the AsciiTexture.
    /// </summary>
    /// <value>
    /// The width represents the number of pixels along the horizontal axis of the texture.
    /// </value>
    public int Width { get;}

    /// <summary>
    /// Gets the height of the AsciiTexture in pixels.
    /// </summary>
    public int Height { get;}

    /// Represents an ASCII texture composed of ASCII pixels.
    public AsciiTexture(int width, int height, AsciiPixel[]? pixels = null)
    {
        Width = width;
        Height = height;
        Pixels = pixels ?? new AsciiPixel[width * height];
    }

    /// <summary>
    /// Retrieves a character representing a specific coordinate in the ASCII texture based on the given brightness level.
    /// </summary>
    /// <param name="x">The x-coordinate within the ASCII texture.</param>
    /// <param name="y">The y-coordinate within the ASCII texture.</param>
    /// <param name="brightness">The brightness factor used to adjust the pixel brightness, between 0 and 0.99.</param>
    /// <param name="color">The output parameter that returns the RGB color of the pixel at the specified coordinates.</param>
    /// <returns>A character corresponding to the brightness level of the pixel at the specified coordinates.</returns>
    public char GetCoord(int x, int y, float brightness, out RgbColor color)
    {
        brightness = Clamp(brightness, 0, 0.99f);

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            color = new RgbColor(0, 0, 0);
            return '?';
        }

        int offset = x + (Height - y - 1) * Width;
        var pixel = Pixels[offset];
        color = pixel.Color;
        return GetAsciiGradient(pixel.Brightness * brightness);
    }

    /// <summary>
    /// Returns the ASCII character corresponding to the given brightness value,
    /// using a predefined gradient of ASCII characters.
    /// </summary>
    /// <param name="brightness">A float value representing the brightness, where 0.0 represents the darkest and 0.99 represents the brightest.</param>
    /// <return>A char representing the ASCII character that corresponds to the given brightness value.</return>
    public static char GetAsciiGradient(float brightness)
    {
        const string ASCI_GRADIENT = " .:'-~=<\\*({[%08O#@Q&";
        brightness = Clamp(brightness, 0, 0.99f);
        int index = (int)(ASCI_GRADIENT.Length * brightness);
        return ASCI_GRADIENT[index];
    }

    /// <summary>
    /// Clamps a float value between a minimum and maximum value.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The minimum value to which to clamp.</param>
    /// <param name="max">The maximum value to which to clamp.</param>
    /// <returns>The clamped float value.</returns>
    private static float Clamp(float value, float min, float max)
    {
        return Math.Clamp(value, min, max);
    }
}