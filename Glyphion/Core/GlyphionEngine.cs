using System.Numerics;
using System.Text;
using Glyphion.Core.CoolMath;
using Glyphion.Core.Renderer;
using Glyphion.Core.Textures;

namespace Glyphion.Core;

/// <summary>
/// The GlyphionEngine class is responsible for the primary rendering logic within the Glyphion framework.
/// </summary>
public class GlyphionEngine
{
    /// <summary>
    /// An instance of the TerminalRenderer class used for rendering ASCII graphics and text onto the terminal.
    /// This renderer handles all the low-level drawing operations such as setting pixels, managing screen dimensions, and applying colors.
    /// </summary>
    private readonly TerminalRenderer _renderer;

    /// <summary>
    /// Manages the textures used within the Glyphion engine.
    /// Provides methods for adding new textures and retrieving existing textures based on character codes.
    /// </summary>
    private readonly TextureManager _textureManager;

    /// <summary>
    /// Represents the engine responsible for rendering and managing ASCII textures in a console application.
    /// </summary>
    internal GlyphionEngine(GlyphionEngineOptions options)
    {
        _renderer = new TerminalRenderer(options.UseColors);
  
        _textureManager = new TextureManager();
    }

    /// <summary>
    /// Adds a new texture to the texture manager.
    /// </summary>
    /// <param name="texCode">The character code used to identify the texture.</param>
    /// <param name="texData">An array of AsciiPixel representing the texture data.</param>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    public void AddTexture(char texCode, AsciiPixel[] texData, int width, int height)
    {
        _textureManager.AddTexture(texCode, texData, width, height);
    }

    /// <summary>
    /// Adds a texture in PPM format to the texture manager using the specified texture code and file name.
    /// </summary>
    /// <param name="texCode">The character code for the texture.</param>
    /// <param name="fileName">The file name of the PPM texture to add.</param>
    public void AddTexturePpm(char texCode, string fileName)
    {
        _textureManager.AddTexturePpm(texCode, fileName);
    }

    /// <summary>
    /// Retrieves an AsciiTexture corresponding to the given texCode.
    /// </summary>
    /// <param name="texCode">The character code representing the texture to retrieve.</param>
    /// <returns>The AsciiTexture associated with the specified texCode, or null if not found.</returns>
    public AsciiTexture? GetTexture(char texCode)
    {
        return _textureManager.GetTexture(texCode);
    }
    
    
    
    // 2d stuff

    /// <summary>Writes plain text to the buffer, calls new method with Background as the bgColor.</summary>
    /// <param name="pos">The position to write to.</param>
    /// <param name="text">String to write.</param>
    /// <param name="foregroundColor">Specified color to write with.</param>
    public void WriteText(Vector2 pos, string text, RgbColor foregroundColor)
    {
        for (int i = 0; i < text.Length; i++)
        {
            // Invert the Y-coordinate here
            Vector3 position = new Vector3(
                pos.X + i,
                _renderer.GetScreenHeight() - pos.Y - 1, 
                -1f
            );
            _renderer.SetPixel(position, text[i], foregroundColor.R, foregroundColor.G, foregroundColor.B);
 
        }
    }

    /// <summary>
    /// Writes text to the buffer in a FIGlet font, calls new method with Background as the bgColor.
    /// </summary>
    /// <param name="pos">The top left corner of the text.</param>
    /// <param name="text">String to write.</param>
    /// <param name="font">FIGLET font to write with.</param>
    /// <param name="foregroundColor">Specified color index to write with.</param>
    public void WriteFiglet(Vector2 pos, string text, FigletFont font, RgbColor foregroundColor)
    {
        WriteFiglet(pos, text, font, foregroundColor, new RgbColor(255, 255, 255));
    }

    /// <summary> Writes text to the buffer in a FIGlet font. </summary>
    /// <param name="pos">The top left corner of the text.</param>
    /// <param name="text">String to write.</param>
    /// <param name="font">FIGlet font to write with.</param>
    /// <param name="fgColor">Specified foreground color index to write with.</param>
    /// <param name="bgColor">Specified background color index to write with.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    /// <exception cref="ArgumentException">Thrown when text contains non-ASCII characters.</exception>
    public void WriteFiglet(Vector2 pos, string text, FigletFont font, RgbColor fgColor, RgbColor bgColor)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        if (Encoding.UTF8.GetByteCount(text) != text.Length)
            throw new ArgumentException("String contains non-ascii characters");

        int sWidth = FigletFont.GetStringWidth(font, text);

        for (int line = 1; line <= font.Height; line++)
        {
            int runningWidthTotal = 0;

            for(int c = 0; c < text.Length; c++) {
                char character = text[c];
                string fragment = FigletFont.GetCharacter(font, character, line);
                for(int f = 0; f < fragment.Length; f++) {
                    if(fragment[f] != ' ') {
                        // Invert the Y-coordinate here
                        Vector3 position = new Vector3(
                            pos.X + runningWidthTotal + f, 
                            _renderer.GetScreenHeight() - (pos.Y + line - 1) - 1, 
                            -1f
                        );
                        _renderer.SetPixel(position, fragment[f], fgColor.R, fgColor.G, fgColor.B, bgColor.R, bgColor.G, bgColor.B);
                    }
                }
                runningWidthTotal += fragment.Length;
            }
        }
    }

    /// <summary> Draws an arc. </summary>
    /// <param name="center">The center point of the arc.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="fgColor">The foreground color to use.</param>
    /// <param name="bgColor">The background color to use (optional).</param>
    /// <param name="arc">The angle of the arc in degrees. Defaults to 360 degrees.</param>
    /// <param name="c">The character to use for drawing the arc. Defaults to '█'.</param>
    public void DrawArc(Vector2 center, float radius, RgbColor fgColor, RgbColor? bgColor = null, float arc = 360,
        char c = '█')
    {
        for (int a = 0; a < arc; a++)
        {
            float x = radius * MathF.Cos(a * MathF.PI / 180);
            float y = radius * MathF.Sin(a * MathF.PI / 180);

            Vector3 position = new Vector3(center.X + x, center.Y + y, -1f);
            if (bgColor.HasValue)
            {
                _renderer.SetPixel(position, c, fgColor.R, fgColor.G, fgColor.B, bgColor.Value.R, bgColor.Value.G, bgColor.Value.B);
            }
            else
            {
                _renderer.SetPixel(position, c, fgColor.R, fgColor.G, fgColor.B);
            }
        }
    }

    /// <summary> Draws a semi-circle filled with a specified character. </summary>
    /// <param name="center">Center of the semi-circle.</param>
    /// <param name="radius">Radius of the semi-circle.</param>
    /// <param name="start">Start angle in degrees for the semi-circle.</param>
    /// <param name="arc">Arc angle in degrees to complete the semi-circle.</param>
    /// <param name="fgColor">Specified foreground color.</param>
    /// <param name="bgColor">Specified background color (optional).</param>
    /// <param name="c">Character to fill the semi-circle with (default is '█').</param>
    public void DrawSemiCircle(Vector2 center, float radius, float start, float arc, RgbColor fgColor,
        RgbColor? bgColor = null, char c = '█')
    {
        for (float a = start; a > -arc + start; a--)
        {
            for (float r = 0; r < radius + 1; r++)
            {
                float x = r * MathF.Cos(a * MathF.PI / 180);
                float y = r * MathF.Sin(a * MathF.PI / 180);

                Vector3 position = new Vector3(center.X + x, center.Y + y, -1f);
                if (bgColor.HasValue)
                {
                    _renderer.SetPixel(position, c, fgColor.R, fgColor.G, fgColor.B, bgColor.Value.R, bgColor.Value.G, bgColor.Value.B);
                }
                else
                {
                    _renderer.SetPixel(position, c, fgColor.R, fgColor.G, fgColor.B);
                }
            }
        }
    }

    /// <summary> Draws a line from start to end using Bresenham's Line Algorithm. </summary>
    /// <param name="start">Point to draw the line from.</param>
    /// <param name="end">Point to draw the line to.</param>
    /// <param name="fgColor">Foreground color to draw with.</param>
    /// <param name="bgColor">Background color to draw with, if any.</param>
    /// <param name="c">Character to use for drawing the line.</param>
    public void DrawLine(Vector2 start, Vector2 end, RgbColor fgColor, RgbColor? bgColor = null, char c = '█')
    {
        Vector2 delta = end - start;
        Vector2 da = Vector2.Zero, db = Vector2.Zero;
        if (delta.X < 0) da.X = -1;
        else if (delta.X > 0) da.X = 1;
        if (delta.Y < 0) da.Y = -1;
        else if (delta.Y > 0) da.Y = 1;
        if (delta.X < 0) db.X = -1; else if (delta.X > 0) db.X = 1;
        float longest = MathF.Abs(delta.X);
        float shortest = MathF.Abs(delta.Y);

        if (!(longest > shortest))
        {
            longest = MathF.Abs(delta.Y);
            shortest = MathF.Abs(delta.X);
            if (delta.Y < 0) db.Y = -1; else if (delta.Y > 0) db.Y = 1;
            db.X = 0;
        }

        float numerator = longest / 2;
        Vector2 p = start;
        for (int i = 0; i <= longest; i++)
        {
            Vector3 position = new Vector3(p.X, p.Y, -1f);
            if (bgColor.HasValue)
            {
                _renderer.SetPixel(position, c, fgColor.R, fgColor.G, fgColor.B, bgColor.Value.R, bgColor.Value.G, bgColor.Value.B);
            }
            else
            {
                _renderer.SetPixel(position, c, fgColor.R, fgColor.G, fgColor.B);
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                p += da;
            }
            else
            {
                p += db;
            }
        }
    }

    /// <summary> Draws a rectangle. </summary>
    /// <param name="topLeft">Top left corner of the rectangle.</param>
    /// <param name="bottomRight">Bottom right corner of the rectangle.</param>
    /// <param name="fgColor">Color to draw with.</param>
    /// <param name="bgColor">Color to draw to the background with.</param>
    /// <param name="c">Character to use.</param>
    public void DrawRectangle(Vector2 topLeft, Vector2 bottomRight, RgbColor fgColor, RgbColor? bgColor = null,
        char c = '█')
    {
        for (float i = 0; i < bottomRight.X - topLeft.X; i++)
        {
            SetPixelWithColor(new Vector2(topLeft.X + i, topLeft.Y), fgColor, bgColor, c);
            SetPixelWithColor(new Vector2(topLeft.X + i, bottomRight.Y), fgColor, bgColor, c);
        }

        for (float i = 0; i < bottomRight.Y - topLeft.Y + 1; i++)
        {
            SetPixelWithColor(new Vector2(topLeft.X, topLeft.Y + i), fgColor, bgColor, c);
            SetPixelWithColor(new Vector2(bottomRight.X, topLeft.Y + i), fgColor, bgColor, c);
        }
    }

    /// <summary> Draws a Rectangle and fills it. </summary>
    /// <param name="topLeft">Top Left corner of rectangle.</param>
    /// <param name="bottomRight">Bottom Right corner of rectangle.</param>
    /// <param name="fgColor">Color to draw with.</param>
    /// <param name="bgColor">Color to draw the background with.</param>
    /// <param name="c">Character to use.</param
    public void DrawFill(Vector2 topLeft, Vector2 bottomRight, RgbColor fgColor, RgbColor? bgColor = null, char c = '█')
    {
        for (float y = topLeft.Y; y < bottomRight.Y; y++)
        {
            for (float x = topLeft.X; x < bottomRight.X; x++)
            {
                SetPixelWithColor(new Vector2(x, y), fgColor, bgColor, c);
            }
        }
    }

    /// <summary> Draws a grid. </summary>
    /// <param name="topLeft">Top Left corner of grid.</param>
    /// <param name="bottomRight">Bottom Right corner of the grid.</param>
    /// <param name="spacing">The spacing between grid lines.</param>
    /// <param name="fgColor">Color to draw the grid lines.</param>
    /// <param name="bgColor">Color to draw the background, if any.</param>
    /// <param name="c">Character to use for drawing the grid.</param>
    public void DrawGrid(Vector2 topLeft, Vector2 bottomRight, float spacing, RgbColor fgColor,
        RgbColor? bgColor = null, char c = '█')
    {
        for (float y = topLeft.Y; y < bottomRight.Y / spacing; y++)
        {
            DrawLine(new Vector2(topLeft.X, y * spacing), new Vector2(bottomRight.X, y * spacing), fgColor, bgColor, c);
        }

        for (float x = topLeft.X; x < bottomRight.X / spacing; x++)
        {
            DrawLine(new Vector2(x * spacing, topLeft.Y), new Vector2(x * spacing, bottomRight.Y), fgColor, bgColor, c);
        }
    }


    /// <summary> Draws a frame using box drawing symbols. </summary>
    /// <param name="topLeft">Top Left corner of box.</param>
    /// <param name="bottomRight">Bottom Right corner of box.</param>
    /// <param name="fgColor">The specified color.</param>
    /// <param name="bgColor">The specified background color.</param>
    public void DrawFrame(Vector2 topLeft, Vector2 bottomRight, RgbColor fgColor, RgbColor? bgColor = null)
    {
        for (float i = 1; i < bottomRight.X - topLeft.X; i++)
        {
            SetPixelWithColor(new Vector2(topLeft.X + i, topLeft.Y), fgColor, bgColor, '─');
            SetPixelWithColor(new Vector2(topLeft.X + i, bottomRight.Y), fgColor, bgColor, '─');
        }
        for (float i = 1; i < bottomRight.Y - topLeft.Y; i++)
        {
            SetPixelWithColor(new Vector2(topLeft.X, topLeft.Y + i), fgColor, bgColor, '│');
            SetPixelWithColor(new Vector2(bottomRight.X, topLeft.Y + i), fgColor, bgColor, '│');
        }
        SetPixelWithColor(new Vector2(topLeft.X, topLeft.Y), fgColor, bgColor, '┌');
        SetPixelWithColor(new Vector2(bottomRight.X, topLeft.Y), fgColor, bgColor, '┐');
        SetPixelWithColor(new Vector2(topLeft.X, bottomRight.Y), fgColor, bgColor, '└');
        SetPixelWithColor(new Vector2(bottomRight.X, bottomRight.Y), fgColor, bgColor, '┘');
    }

    // Helper methods

    /// <summary>
    /// Calculates the orientation of three points in 2D space.
    /// </summary>
    /// <param name="a">The first point as a Vector2.</param>
    /// <param name="b">The second point as a Vector2.</param>
    /// <param name="c">The third point as a Vector2.</param>
    /// <returns>Returns a float indicating the orientation. A positive value means counter-clockwise,
    /// a negative value means clockwise, and zero means collinear.</returns>
    private float Orient(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));
    }

    /// <summary>
    /// Sets a pixel at the specified position with the given foreground and background colors,
    /// and the specified character.
    /// </summary>
    /// <param name="position">The 2D coordinates where the pixel will be set.</param>
    /// <param name="fgColor">The color used for the foreground of the pixel.</param>
    /// <param name="bgColor">The color used for the background of the pixel if specified; otherwise, the background is not set.</param>
    /// <param name="c">The character to be displayed at the specified position.</param>
    private void SetPixelWithColor(Vector2 position, RgbColor fgColor, RgbColor? bgColor, char c)
    {
        Vector3 pos3D = new Vector3(position.X, position.Y, -1f);
        if (bgColor.HasValue)
        {
            _renderer.SetPixel(pos3D, c, fgColor.R, fgColor.G, fgColor.B, bgColor.Value.R, bgColor.Value.G, bgColor.Value.B);
        }
        else
        {
            _renderer.SetPixel(pos3D, c, fgColor.R, fgColor.G, fgColor.B);
        }
    }


    /// <summary>
    /// Draws a triangle on the screen with the specified vertices and brightness factor.
    /// Coordinates are normalized between -1 to 1 for both x and y axes.
    /// </summary>
    /// <param name="p1">The first vertex of the triangle in 3D space.</param>
    /// <param name="p2">The second vertex of the triangle in 3D space.</param>
    /// <param name="p3">The third vertex of the triangle in 3D space.</param>
    /// <param name="brightness">The brightness level of the triangle, clamped between 0.0 and 1.0.</param>
    public void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, float brightness)
{
    // Normalizing coordinates from -1 to 1 in x and y axis
    var halfScreenW = _renderer.GetScreenWidth() / 2f;
    var halfScreenH = _renderer.GetScreenHeight() / 2f;
    var normalizedScreenWidth = halfScreenW / _renderer.GetAspectRatio();

    p1.X = p1.X * normalizedScreenWidth + halfScreenW;
    p1.Y = p1.Y * halfScreenH + halfScreenH;
    p2.X = p2.X * normalizedScreenWidth + halfScreenW;
    p2.Y = p2.Y * halfScreenH + halfScreenH;
    p3.X = p3.X * normalizedScreenWidth + halfScreenW;
    p3.Y = p3.Y * halfScreenH + halfScreenH;

    var minX = (int)Math.Min(Math.Min(p1.X, p2.X), p3.X);
    var minY = (int)Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);
    var maxX = (int)Math.Max(Math.Max(p1.X, p2.X), p3.X);
    var maxY = (int)Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);

    // Prevent rendering off screen
    minX = Math.Max(minX, 0);
    minY = Math.Max(minY, 0);
    maxX = Math.Min(maxX, _renderer.GetScreenWidth() - 1);
    maxY = Math.Min(maxY, _renderer.GetScreenHeight() - 1);

    var denom = ((p2.Y - p1.Y) * (p3.X - p1.X)) - ((p2.X - p1.X) * (p3.Y - p1.Y));
    if (denom == 0) return;

    // Adjust the color based on brightness
    brightness = Math.Clamp(brightness, 0.0f, 1.0f);
    var r = (byte)(255 * brightness);  // Red intensity
    var g = (byte)(200 * brightness);  // Green intensity
    var b = (byte)(50 * brightness);   // Blue intensity

    for (var posY = minY; posY <= maxY; posY++)
    {
        for (var posX = minX; posX <= maxX; posX++)
        {
            var w1 = ((p2.Y - p3.Y) * (posX - p3.X) + (p3.X - p2.X) * (posY - p3.Y)) / denom;
            var w2 = ((p3.Y - p1.Y) * (posX - p3.X) + (p1.X - p3.X) * (posY - p3.Y)) / denom;
            var w3 = 1f - w1 - w2;

            if (w1 < 0 || w2 < 0 || w3 < 0) continue;

            var interpolatedZ = (p1.Z * w1 + p2.Z * w2 + p3.Z * w3);
            var zValue = MathUtils.ZFormula(interpolatedZ);
            var pixelPosition = new Vector3(posX, posY, zValue);

            // Render pixel with the character and the calculated RGB color
            _renderer.SetPixel(pixelPosition, AsciiTexture.GetAsciiGradient(brightness), r, g, b);
        }
    }
    
}

/// <summary>
/// Changes the background character of the terminal.
/// </summary>
/// <param name="character">The character used to set the background.</param>
public void ChangeBackground(char character)
{
    _renderer.ChangeBackground(character);
}

/// <summary>
/// Renders the current buffer to the display and then clears the buffer for the next frame.
/// </summary>
internal void Render()
{
    _renderer.Render();
    _renderer.ClearBuffer();
}

/// <summary>
/// Gets the width of the screen in characters.
/// </summary>
public int ScreenWidth => _renderer.GetScreenWidth();

/// <summary>
/// Gets the height of the screen in characters.
/// </summary>
public int ScreenHeight => _renderer.GetScreenHeight();

/// <summary>
/// Draws a textured triangle on the screen using UV coordinates for texture mapping.
/// </summary>
/// <param name="p1">The first vertex of the triangle, specified as a Vector3.</param>
/// <param name="p2">The second vertex of the triangle, specified as a Vector3.</param>
/// <param name="p3">The third vertex of the triangle, specified as a Vector3.</param>
/// <param name="uv">An array containing the UV coordinates corresponding to each vertex of the triangle.</param>
/// <param name="texCode">The code representing the texture to be applied.</param>
/// <param name="brightness">The brightness level to be applied to the texture.</param>
public void DrawTriangleUV(Vector3 p1, Vector3 p2, Vector3 p3, Vector2[] uv, char texCode, float brightness)
{
    var tex = GetTexture(texCode);
    if (tex is null)
        return;

    // Backface culling
    var A = new Vector2(p2.X - p1.X, p2.Y - p1.Y);
    var A_normal = new Vector2(-A.Y, A.X);
    var B = new Vector2(p3.X - p1.X, p3.Y - p1.Y);
    if (Vector2.Dot(A_normal, B) < 0)
        return;

    // Normalizing coordinates from -1 to 1 in x and y axis
    var halfScreenW = _renderer.GetScreenWidth() / 2f;
    var halfScreenH = _renderer.GetScreenHeight() / 2f;
    var normalizedScreenWidth = halfScreenW / _renderer.GetAspectRatio();

    p1.X = p1.X * normalizedScreenWidth + halfScreenW;
    p1.Y = p1.Y * halfScreenH + halfScreenH;
    p2.X = p2.X * normalizedScreenWidth + halfScreenW;
    p2.Y = p2.Y * halfScreenH + halfScreenH;
    p3.X = p3.X * normalizedScreenWidth + halfScreenW;
    p3.Y = p3.Y * halfScreenH + halfScreenH;

    var invZ1 = 1f / p1.Z;
    var invZ2 = 1f / p2.Z;
    var invZ3 = 1f / p3.Z;

    var minX = (int)Math.Min(Math.Min(p1.X, p2.X), p3.X);
    var minY = (int)Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);
    var maxX = (int)Math.Max(Math.Max(p1.X, p2.X), p3.X);
    var maxY = (int)Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);

    // Prevent rendering off screen
    minX = Math.Max(minX, 0);
    minY = Math.Max(minY, 0);
    maxX = Math.Min(maxX, _renderer.GetScreenWidth() - 1);
    maxY = Math.Min(maxY, _renderer.GetScreenHeight() - 1);

    var denomW2 = ((p2.Y - p1.Y) * (p3.X - p1.X)) - ((p2.X - p1.X) * (p3.Y - p1.Y));
    var denomW3 = ((p3.Y - p2.Y) * (p1.X - p2.X)) - ((p3.X - p2.X) * (p1.Y - p2.Y));

    if (denomW2 == 0 || denomW3 == 0)
        return;

    for (var posY = minY; posY <= maxY; posY++)
    {
        for (var posX = minX; posX <= maxX; posX++)
        {
            var w2 = ((posY - p1.Y) * (p3.X - p1.X) - (posX - p1.X) * (p3.Y - p1.Y)) / denomW2;
            var w3 = ((posY - p2.Y) * (p1.X - p2.X) - (posX - p2.X) * (p1.Y - p2.Y)) / denomW3;
            var w1 = 1f - w2 - w3;

            if (w1 < 0 || w1 > 1 || w2 < 0 || w2 > 1 || w3 < 0 || w3 > 1)
                continue;

            var interpolatedZ = (p1.Z * w1 + p2.Z * w2 + p3.Z * w3);
            var zValue = MathUtils.ZFormula(interpolatedZ);
            var pixelPosition = new Vector3(posX, posY, zValue);

            var invZ = invZ1 * w1 + invZ2 * w2 + invZ3 * w3;

            var uvCoord = (uv[0] * w1 + uv[1] * w2 + uv[2] * w3) / invZ;
            var uvX = (int)(uvCoord.X * tex.Width);
            var uvY = (int)(uvCoord.Y * tex.Height);

            // Use GetCoord with out parameters for the RGB values

            var pixelChar = tex.GetCoord(uvX, uvY, brightness, out var color);

            // Render the pixel with the RGB color
            _renderer.SetPixel(pixelPosition, pixelChar, color.R, color.G, color.B);
        }
    }
}


/// <summary>
/// Draws a quadrilateral on the screen by splitting it into two triangles.
/// </summary>
/// <param name="p1">The first vertex of the quadrilateral.</param>
/// <param name="p2">The second vertex of the quadrilateral.</param>
/// <param name="p3">The third vertex of the quadrilateral.</param>
/// <param name="p4">The fourth vertex of the quadrilateral.</param>
/// <param name="brightness">The brightness value for the drawn quadrilateral.</param>
public void DrawQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float brightness)
{
    DrawTriangle(p1, p2, p3, brightness);
    DrawTriangle(p1, p3, p4, brightness);
}

/// <summary>
/// Draws a quadrilateral by dividing it into two triangles and rendering each triangle.
/// </summary>
/// <param name="p">An array of four vertices that define the quadrilateral.</param>
/// <param name="brightness">The brightness value of the quadrilateral.</param>
public void DrawQuad(Vector3[] p, float brightness)
{
    DrawTriangle(p[0], p[1], p[2], brightness);
    DrawTriangle(p[0], p[2], p[3], brightness);
}

/// <summary>
/// Draws a quadrilateral with UV coordinates mapped to a texture.
/// </summary>
/// <param name="p1">The first vertex of the quadrilateral.</param>
/// <param name="p2">The second vertex of the quadrilateral.</param>
/// <param name="p3">The third vertex of the quadrilateral.</param>
/// <param name="p4">The fourth vertex of the quadrilateral.</param>
/// <param name="texCode">The character code representing the texture to map.</param>
/// <param name="brightness">The brightness level of the texture mapping.</param>
public void DrawQuadUV(Vector3[] p, char texCode, float brightness)
{
    Vector2[] uv1 = { new Vector2(0, 0), new Vector2(1, 0) / p[1].Z, new Vector2(1, 1) / p[2].Z };
    Vector2[] uv2 = { new Vector2(0, 0), new Vector2(1, 1) / p[2].Z, new Vector2(0, 1) / p[3].Z };
    DrawTriangleUV(p[0], p[1], p[2], uv1, texCode, brightness);
    DrawTriangleUV(p[0], p[2], p[3], uv2, texCode, brightness);
}

/// <summary> Draws a quadrilateral with UV texture coordinates. </summary>
/// <param name="p1">The first vertex position of the quadrilateral.</param>
/// <param name="p2">The second vertex position of the quadrilateral.</param>
/// <param name="p3">The third vertex position of the quadrilateral.</param>
/// <param name="p4">The fourth vertex position of the quadrilateral.</param>
/// <param name="texCode">The texture code used for UV mapping.</param>
/// <param name="brightness">The brightness value for rendering the quadrilateral.</param>
public void DrawQuadUV(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, char texCode, float brightness)
{
    Vector2[] uv1 = { new Vector2(0, 0), new Vector2(1, 0) / p2.Z, new Vector2(1, 1) / p3.Z };
    Vector2[] uv2 = { new Vector2(0, 0), new Vector2(1, 1) / p3.Z, new Vector2(0, 1) / p4.Z };
    DrawTriangleUV(p1, p2, p3, uv1, texCode, brightness);
    DrawTriangleUV(p1, p3, p4, uv2, texCode, brightness);
}

/// <summary>
/// Draws a circle at the specified position with the given radius, character, and RGB color.
/// </summary>
/// <param name="position">The 3D vector position where the circle's center is located.</param>
/// <param name="r">The radius of the circle.</param>
/// <param name="character">The character to use for rendering the circle.</param>
/// <param name="red">The red component of the RGB color to use for the circle.</param>
/// <param name="green">The green component of the RGB color to use for the circle.</param>
/// <param name="blue">The blue component of the RGB color to use for the circle.</param>
public void DrawCircle(Vector3 position, float r, char character, byte red, byte green, byte blue)
{
    var halfScreenW = _renderer.GetScreenWidth() / 2f;
    var halfScreenH = _renderer.GetScreenHeight() / 2f;
    var normalizedScreenWidth = halfScreenW / _renderer.GetAspectRatio();

    position.X = position.X * normalizedScreenWidth + halfScreenW;
    position.Y = position.Y * halfScreenH + halfScreenH;

    var minX = (int)(position.X - r);
    var minY = (int)(position.Y - r);
    var maxX = (int)(position.X + r);
    var maxY = (int)(position.Y + r);

    minX = Math.Max(minX, 0);
    minY = Math.Max(minY, 0);
    maxX = Math.Min(maxX, _renderer.GetScreenWidth() - 1);
    maxY = Math.Min(maxY, _renderer.GetScreenHeight() - 1);

    for (var y = minY; y <= maxY; y++)
    {
        for (var x = minX; x <= maxX; x++)
        {
            var dx = x - position.X;
            var dy = y - position.Y;
            if ((dx * dx) * _renderer.GetAspectRatio() + dy * dy < r * r)
            {
                _renderer.SetPixel(new Vector3(x, y, position.Z), character, red, green, blue);
            }
        }
    }
}

// 3d stuff

    /// <summary>
    /// Draws a textured quad using 3D vertices, a texture code, and brightness level while applying camera transformations.
    /// </summary>
    /// <param name="position">The position offset for the quad.</param>
    /// <param name="vertices4">An array of four 3D vertices representing the quad.</param>
    /// <param name="texCode">The texture code representing the texture to be applied on the quad.</param>
    /// <param name="brightness">The brightness level of the texture.</param>
    /// <param name="camera">The camera through which the transformation is applied.</param>
    public void DrawPlainUV(Vector3 position, Vector3[] vertices4, char texCode, float brightness, Camera camera)
    {
        var quadVertices = new Vector3[4];
        for (var i = 0; i < 4; i++)
        {
            // Converting 3D vertices to 2D vertices
            var vertice = vertices4[i] + position;
            vertice -= camera.Position;
            vertice = Vector3.Transform(vertice, camera.RotXAxis());
            vertice = Vector3.Transform(vertice, camera.RotYAxis());

            if (vertice.Z == 0) continue; // Prevent division by zero
            vertice = new Vector3(vertice.X / vertice.Z, vertice.Y / vertice.Z, vertice.Z);

            // Temporary fix for rendering -z values
            if (vertice.Z < 0) return;
            quadVertices[i] = vertice;
        }
        // Render 2D vertices
      DrawQuadUV(quadVertices, texCode, brightness);
    }

    /// <summary>
    /// Draws a plain object using specified vertices, brightness, and camera settings.
    /// </summary>
    /// <param name="position">The position to start drawing from.</param>
    /// <param name="vertices4">An array of four vertices defining the object.</param>
    /// <param name="brightness">The brightness level for drawing.</param>
    /// <param name="camera">The camera through which the scene is viewed.</param>
    public void DrawPlain(Vector3 position, Vector3[] vertices4, float brightness, Camera camera)
    {
        var quadVertices = new Vector3[4];
        for (var i = 0; i < 4; i++)
        {
            var vertice = vertices4[i] + position;
            vertice -= camera.Position;
            vertice = Vector3.Transform(vertice, camera.RotXAxis());
            vertice = Vector3.Transform(vertice, camera.RotYAxis());

            if (vertice.Z == 0) continue; // Prevent division by zero
            vertice = new Vector3(vertice.X / vertice.Z, vertice.Y / vertice.Z, vertice.Z);

            if (vertice.Z < 0) return;
            quadVertices[i] = vertice;
        }
       DrawQuad(quadVertices, brightness);
    }

    /// <summary>
    /// Draws a 3D cube at a specified position with a given size and brightness, using the specified camera for orientation.
    /// </summary>
    /// <param name="position">The position where the center of the cube will be drawn.</param>
    /// <param name="size">The size of the cube.</param>
    /// <param name="brightness">The brightness of the cube.</param>
    /// <param name="camera">The camera used to transform and project the cube's vertices.</param>
    public void DrawCube(Vector3 position, float size, float brightness, Camera camera)
    {
        var v = new Vector3[8]
        {
            new Vector3(-1, -0.5f, -2),
            new Vector3(1, -0.5f, -2),
            new Vector3(1, 0.5f, -2),
            new Vector3(-1, 0.5f, -2),
            new Vector3(-1, -0.5f, 2),
            new Vector3(1, -0.5f, 2),
            new Vector3(1, 0.5f, 2),
            new Vector3(-1, 0.5f, 2),
        };

        for (var i = 0; i < 8; i++)
        {
            v[i] *= size;
            v[i] += position;
            v[i] -= camera.Position;
            v[i] = Vector3.Transform(v[i], camera.RotXAxis());
            v[i] = Vector3.Transform(v[i], camera.RotYAxis());

            if (v[i].Z == 0) continue; // Prevent division by zero
            v[i] = new Vector3(v[i].X / v[i].Z, v[i].Y / v[i].Z, v[i].Z);

            if (v[i].Z < 0) return;
        }

       DrawQuad(new Vector3[] { v[0], v[1], v[2], v[3] }, brightness * 0.8f);
       DrawQuad(new Vector3[] { v[1], v[2], v[6], v[5] }, brightness * 0.95f);
       DrawQuad(new Vector3[] { v[0], v[3], v[7], v[4] }, brightness * 0.8f);
       DrawQuad(new Vector3[] { v[4], v[5], v[6], v[7] }, brightness * 0.95f);
       DrawQuad(new Vector3[] { v[3], v[2], v[6], v[7] }, brightness * 1f);
       DrawQuad(new Vector3[] { v[0], v[1], v[5], v[4] }, brightness * 1f);
    }

		

}