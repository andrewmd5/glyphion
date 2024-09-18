namespace Glyphion.Core.Textures;

/// <summary>
/// A FIGlet font.
/// </summary>
public class FigletFont
{
    /// <summary>
    /// Gets the number of comment lines in the FIGlet font file.
    /// </summary>
    public int CommentLines { get; private set; }

    /// <summary>
    /// Gets the character used to represent hard blanks in the FIGlet font.
    /// </summary>
    public string HardBlank { get; private set; }

    /// <summary>
    /// Gets the height of the FIGlet font.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the value of kerning for the FIGlet font. Kerning is the space between characters in a rendered text string.
    /// </summary>
    public int Kerning { get; private set; }

    /// <summary>
    /// Gets the lines of the FIGlet font file.
    /// </summary>
    public string[] Lines { get; private set; }

    /// <summary>
    /// Gets the maximum length of any FIGlet character in the font.
    /// </summary>
    public int MaxLength { get; private set; }

    /// <summary>
    /// Gets the signature of the FIGlet font file.
    /// </summary>
    public string Signature { get; private set; }

    /// <summary>
    /// Loads a FIGlet font from a file.
    /// </summary>
    /// <param name="filePath">The path to the FIGlet font file.</param>
    /// <returns>A FigletFont object loaded from the specified file.</returns>
        public static FigletFont Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            var fontLines = File.ReadAllLines(filePath);

            var font = new FigletFont {
                Lines = fontLines
            };
            
            var firstLine = font.Lines[0];
            var configs = firstLine.Split(' ');
            
            // Signature processing
            font.Signature = configs[0][..^1];
            if (font.Signature == "flf2a") {
                font.HardBlank = configs[0][^1..];
                font.Height = ParseInt(configs, 1);
                font.MaxLength = ParseInt(configs, 3);
                font.CommentLines = ParseInt(configs, 5);
                font.Kerning = ParseInt(configs, 6);
            }

            return font;
        }

    /// <summary>
    /// Parses an integer from a string array at the specified index.
    /// </summary>
    /// <param name="values">The array of strings containing integer values.</param>
    /// <param name="index">The index in the array from which to parse the integer.</param>
    /// <returns>The parsed integer value or 0 if parsing fails or the index is out of bounds.</returns>
    private static int ParseInt(string[] values, int index)
    {
        return (values.Length > index && int.TryParse(values[index], out var result)) ? result : 0;
    }

    /// <summary>
    /// Calculates the width of a string when rendered with the FIGlet font.
    /// </summary>
    /// <param name="font">The FIGlet font used to render the string.</param>
        /// <param name="value">The string to calculate the width for.</param>
        /// <returns>The width of the rendered string.</returns>
        internal static int GetStringWidth(FigletFont font, string value)
        {
            var totalWidth = 0;

            foreach (var character in value)
            {
                var charWidth = 0;

                for (var line = 1; line <= font.Height; line++)
                {
                    var figletCharacter = GetCharacter(font, character, line);

                    // Determine the maximum width of the current character across all lines
                    charWidth = Math.Max(charWidth, figletCharacter.Length);
                }

                totalWidth += charWidth;
            }

            return totalWidth;
        }

    /// <summary>
    /// Retrieves the FIGlet character representation for a specific line.
    /// </summary>
    /// <param name="font">The FIGlet font object.</param>
    /// <param name="character">The character for which the FIGlet representation is required.</param>
    /// <param name="line">The specific line of the FIGlet character representation.</param>
    /// <returns>The FIGlet character representation for the specified line.</returns>
    internal static string GetCharacter(FigletFont font, char character, int line)
    {
        var charIndex = character - 32; // FIGlet assumes characters start from ASCII 32
        if (charIndex < 0) return string.Empty; // Handle out of range characters

        var startIndex = font.CommentLines + (charIndex * font.Height);
        var result = font.Lines[startIndex + line];

        // Remove end markers (typically "$" in FIGlet fonts)
            var lineEnding = result[^1];
            var trimIndex = result.LastIndexOf(lineEnding);
            if (trimIndex > 0) {
                result = result[..trimIndex];  // Trim the end markers
            }

            // Add kerning space if required
            if (font.Kerning > 0) {
                result += new string(' ', font.Kerning);
            }

            // Replace hard blanks with spaces
            return result.Replace(font.HardBlank, " ");
        }
    }