using System.Text;

namespace Glyphion.Core.Textures;

/// <summary>
/// Manages ASCII textures for use in ASCII art rendering.
/// Provides methods for adding and retrieving textures.
/// </summary>
public class TextureManager
    {

        // Dictionary for texture storage
        /// <summary>
        /// Dictionary used for storing ASCII textures mapped by a character key.
        /// </summary>
        private readonly Dictionary<char, AsciiTexture> _textures = new();

        // Private constructor to prevent direct instantiation
        /// <summary>
        /// Manages ASCII textures, allowing addition and retrieval of textures defined by characters.
        /// </summary>
        internal TextureManager() { }

        // Method to add texture with pixel data
        /// <summary>
        /// Adds a new texture to the texture manager.
        /// </summary>
        /// <param name="texCode">The character code used to identify the texture.</param>
        /// <param name="texData">An array of AsciiPixel representing the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        public void AddTexture(char texCode, AsciiPixel[] texData, int width, int height)
        {
            if (_textures.ContainsKey(texCode))
            {
                Console.WriteLine("Texture code already taken");
                return;
            }

            var texture = new AsciiTexture(width, height, texData);
            _textures[texCode] = texture;
        }

        // Method to load texture from PPM file
        /// <summary>
        /// Adds an ASCII texture by loading data from a PPM file.
        /// </summary>
        /// <param name="texCode">A character representing the texture code.</param>
        /// <param name="fileName">The path to the PPM file from which the texture is loaded.</param>
        public void AddTexturePpm(char texCode, string fileName)
        {
            if (_textures.ContainsKey(texCode))
            {
                Console.WriteLine("Texture code already taken");
                return;
            }

            using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
            string type = ReadTokenFromStream(fs);
            if (type != "P6")
            {
                throw new Exception("Unsupported PPM format");
            }

            int width = int.Parse(ReadTokenFromStream(fs));
            int height = int.Parse(ReadTokenFromStream(fs));
            _ = int.Parse(ReadTokenFromStream(fs)); // MaxValue, ignored since we assume it's 255

            int dataSize = width * height * 3;
            byte[] rgbData = new byte[dataSize];
            fs.Read(rgbData, 0, dataSize);

            AsciiTexture texture = new(width, height);
            for (int i = 0, j = 0; i < width * height; i++, j += 3)
            {
                RgbColor color = new(rgbData[j], rgbData[j + 1], rgbData[j + 2]);
                var pixel = new AsciiPixel
                {
                    Color = color,
                    Brightness = color.ToGrayscale() / 255.0f
                };
                texture.Pixels[i] = pixel;
            }

            _textures[texCode] = texture;
        }

        // Method to retrieve texture
        /// <summary>
        /// Retrieves the texture associated with the given texture code.
        /// </summary>
        /// <param name="texCode">The character code representing the texture.</param>
        /// <returns>The AsciiTexture associated with the given texture code, or null if the texture is not found.</returns>
        public AsciiTexture? GetTexture(char texCode)
        {
            _textures.TryGetValue(texCode, out var texture);
            return texture;
        }

        // Helper method to read tokens from the stream (for PPM format)
        /// <summary>
        /// Reads a token from the given file stream, skipping whitespace and comments in PPM format.
        /// </summary>
        /// <param name="fs">The file stream to read from.</param>
        /// <return>Returns the next token as a string from the file stream.</return>
        private string ReadTokenFromStream(FileStream fs)
        {
            var bytes = new List<byte>();
            int b;

            // Skip whitespace and comments
            while (true)
            {
                b = fs.ReadByte();
                if (b == -1) throw new EndOfStreamException();

                char ch = (char)b;
                if (char.IsWhiteSpace(ch)) continue;

                if (ch == '#') // Skip comments
                {
                    do
                    {
                        b = fs.ReadByte();
                        if (b == -1) throw new EndOfStreamException();
                    } while ((char)b != '\n');
                    continue;
                }

                bytes.Add((byte)b);
                break;
            }

            // Read token
            while (true)
            {
                b = fs.ReadByte();
                if (b == -1) break;

                char ch = (char)b;
                if (char.IsWhiteSpace(ch) || ch == '#')
                {
                    if (ch == '#')
                        fs.Position--; // Unread '#'
                    break;
                }
                bytes.Add((byte)b);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
    