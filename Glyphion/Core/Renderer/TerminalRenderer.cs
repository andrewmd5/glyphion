using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Glyphion.Core.Renderer
{
    /// <summary>
    /// Responsible for terminal-based rendering operations.
    /// </summary>
    public partial class TerminalRenderer
    {
        /// <summary>
        /// Holds the z-depth values for the back buffer, used for depth comparison
        /// in the rendering pipeline to determine the visibility of pixels.
        /// </summary>
        private float[] _zBackBuffer;

        /// <summary>
        /// Holds the z-depth values for the front buffer, used to determine the visibility of pixels
        /// in the rendering process.
        /// </summary>
        private float[] _zFrontBuffer;

        /// <summary>
        /// Buffer that holds the color information for each character on the screen.
        /// </summary>
        private string?[] _colorBuffer;

        /// <summary>
        /// The buffer that stores background color information for each pixel
        /// for rendering purposes in the terminal.
        /// </summary>
        private string?[] _bgColorBuffer;

        /// <summary>
        /// Represents the back buffer for the terminal renderer,
        /// which is used to hold the characters that will be drawn
        /// to the screen in the next rendering pass.
        /// </summary>
        private char[] _backBuffer;

        /// <summary>
        /// The buffer used for storing the current frame's character data to be rendered on the screen.
        /// </summary>
        private char[] _frontBuffer;

        // Screen properties
        /// <summary>
        /// Represents the width of the screen in character cells.
        /// This value is updated during screen resizing operations
        /// and is used for rendering calculations and layout management.
        /// </summary>
        private int _screenWidth;

        /// <summary>
        /// Represents the height of the terminal screen in characters.
        /// <remarks>
        /// This value is updated dynamically based on the current console window size,
        /// and is used to manage and update the screen buffer during rendering operations.
        /// </remarks>
        /// </summary>
        private int _screenHeight;

        /// <summary>
        /// Represents the aspect ratio of the terminal screen, defined as the ratio of the screen's width to its height.
        /// </summary>
        /// <remarks>
        /// This value is used to maintain consistent rendering proportions when the screen size changes.
        /// </remarks>
        private float _aspectRatio;

        /// <summary>
        /// Represents the total count of characters on the screen.
        /// Used to manage the screen's character buffer and determine
        /// the rendering limits within the terminal.
        /// </summary>
        private int _screenCharCount;

        /// <summary>
        /// Represents the width of a single line of characters on the screen in the terminal renderer.
        /// </summary>
        /// <remarks>
        /// This is calculated as twice the screen width plus one to account for newline characters.
        /// </remarks>
        private int _screenCharWidth;

        /// <summary>
        /// Represents the total number of pixels available for rendering within the terminal window.
        /// </summary>
        private int _pixelCount;

        // Background management
        /// <summary>
        /// Stores the character used to fill the background of the terminal screen.
        /// </summary>
        private char _backgroundCharacter = '\0';

        // Terminal and color handling
        /// <summary>
        /// Indicates whether virtual terminal processing is enabled.
        /// </summary>
        /// <remarks>
        /// Virtual terminal processing allows for advanced terminal capabilities, such as extended color support.
        /// It is automatically enabled on non-Windows platforms, and conditionally enabled on Windows if the
        /// necessary processing can be activated successfully.
        /// </remarks>
        private readonly bool _useVirtualTerminal;

        /// <summary>
        /// Indicates if color rendering is enabled in the terminal renderer.
        /// </summary>
        /// <remarks>
        /// This boolean variable determines whether the terminal renderer should use
        /// colors when rendering output. It is initialized in the constructor based
        /// on the provided parameter. When set to true (default), the renderer will
        /// attempt to use colors if the platform and terminal support it.
        /// </remarks>
        private readonly bool _useColors;

        // Window size tracking
        /// <summary>
        /// Tracks the width of the console window during the previous update cycle to detect changes in screen size.
        /// </summary>
        private int _lastWindowWidth;

        /// <summary>
        /// Stores the height of the console window during the last rendering cycle.
        /// This value is used to detect changes in window size and adjust the rendering
        /// accordingly.
        /// </summary>
        private int _lastWindowHeight;
        


        /// <summary>
        /// Responsible for terminal-based rendering operations.
        /// </summary>
        public TerminalRenderer(bool enableColors = true)
        {
            Console.CursorVisible = false;
            _useColors = enableColors;
            
            _useVirtualTerminal = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || TryEnableVirtualTerminalProcessing();

            _lastWindowWidth = Console.WindowWidth;
            _lastWindowHeight = Console.WindowHeight;
            Initialize('Q');
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Changes the background character used for rendering the terminal.
        /// </summary>
        /// <param name="character">The new background character to be used.</param>
        public void ChangeBackground(char character)
        {
            if (_backgroundCharacter == character) return;

            _backgroundCharacter = character;
            Array.Fill(_backBuffer, character);

            for (int i = 0; i < _screenHeight; i++)
            {
                int index = (_screenWidth * 2) + i * _screenCharWidth;
                if (index < _backBuffer.Length)
                    _backBuffer[index] = '\n';
            }
        }

        /// <summary>
        /// Initializes the terminal renderer with the specified background character.
        /// </summary>
        /// <param name="backgroundCharacter">The character to use as the background fill.</param>
        private void Initialize(char backgroundCharacter)
        {
            UpdateScreenSize();
            InitializeBuffers(_screenWidth, _screenHeight, backgroundCharacter);
        }

        /// <summary>
        /// Updates the screen size and aspect ratio based on the current console window dimensions.
        /// Adjusts the console buffer size and initializes buffers with the new screen size if necessary.
        /// </summary>
        private void UpdateScreenSize()
        {
            int newWidth, newHeight;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                newWidth = Math.Min(Console.WindowWidth, Console.BufferWidth);
                newHeight = Console.WindowHeight - 1;

                newWidth -= newWidth % 2;

                try
                {
                    Console.SetBufferSize(newWidth, newHeight + 1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    newWidth = Math.Min(newWidth, Console.BufferWidth);
                    newHeight = Math.Min(newHeight, Console.BufferHeight - 1);
                }
            }
            else
            {
                newWidth = Math.Min(Console.WindowWidth, Console.LargestWindowWidth);
                newHeight = Math.Min(Console.WindowHeight - 1, Console.LargestWindowHeight - 1);

                newWidth -= newWidth % 2;
            }

            if (newWidth != _screenWidth * 2 || newHeight != _screenHeight)
            {
                _screenWidth = newWidth / 2;
                _screenHeight = newHeight;
                _aspectRatio = (float)_screenWidth / _screenHeight;

                InitializeBuffers(_screenWidth, _screenHeight, _backgroundCharacter);
                Console.Clear();
            }

            _lastWindowWidth = Console.WindowWidth;
            _lastWindowHeight = Console.WindowHeight;
        }

        /// <summary>
        /// Clears the rendering buffers in the terminal renderer. Copies the content of
        /// the back buffers to the front buffers for both the Z-buffer and character buffer,
        /// then clears the color and background color buffers.
        /// </summary>
        public void ClearBuffer()
        {
            Array.Copy(_zBackBuffer, _zFrontBuffer, _pixelCount);
            Array.Copy(_backBuffer, _frontBuffer, _screenCharCount);
            Array.Clear(_colorBuffer, 0, _colorBuffer.Length);
            Array.Clear(_bgColorBuffer, 0, _bgColorBuffer.Length);
        }

        /// <summary>
        /// Renders the current frame to the terminal screen using the front buffer.
        /// </summary>
        /// <remarks>
        /// This method processes the front buffer and outputs its content to the terminal.
        /// It handles screen resizing and clears any excess characters.
        /// </remarks>
        public void Render()
        {
            if (HasScreenSizeChanged())
            {
                UpdateScreenSize();
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\x1B[H"); // Move cursor to the top

            for (int i = 0; i < _frontBuffer.Length; i++)
            {
                if (_useColors && _useVirtualTerminal)
                {
                    if (_colorBuffer[i] != null)
                    {
                        stringBuilder.Append(_colorBuffer[i]);
                    }
                    else if (_bgColorBuffer[i] != null)
                    {
                        stringBuilder.Append(_bgColorBuffer[i]);
                    }
                    else
                    {
                        stringBuilder.Append(_frontBuffer[i]);
                    }
                }
                else
                {
                    stringBuilder.Append(_frontBuffer[i]);
                }
            }

            // Clear any excess characters
            int remainingChars = Console.BufferWidth * Console.WindowHeight - stringBuilder.Length;
            if (remainingChars > 0)
            {
                stringBuilder.Append(new string(' ', remainingChars));
            }

            Console.Write(stringBuilder.ToString());
        }

        /// Sets a pixel at the specified position with the given character and colors.
        /// <param name="position">The 3D position of the pixel. The Y-coordinate is inverted for screen rendering.</param>
        /// <param name="character">The character to be drawn at the specified position.</param>
        /// <param name="fgRed">The red component of the foreground color (0-255).</param>
        /// <param name="fgGreen">The green component of the foreground color (0-255).</param>
        /// <param name="fgBlue">The blue component of the foreground color (0-255).</param>
        /// <param name="bgRed">The red component of the optional background color (0-255). Defaults to null.</param>
        /// <param name="bgGreen">The green component of the optional background color (0-255). Defaults to null.</param>
        /// <param name="bgBlue">The blue component of the optional background color (0-255). Defaults to null.</param>
        public void SetPixel(Vector3 position, char character, byte fgRed, byte fgGreen, byte fgBlue, byte? bgRed = null, byte? bgGreen = null, byte? bgBlue = null)
        {
            int x = (int)position.X;
            int y = _screenHeight - (int)position.Y - 1; // Invert the y value

            if (x < 0 || x >= _screenWidth || y < 0 || y >= _screenHeight)
                return;

            int zBufferOffset = x + y * _screenWidth;

            if (position.Z > _zFrontBuffer[zBufferOffset] || position.Z < -1)
                return;

            _zFrontBuffer[zBufferOffset] = position.Z;

            int frontBufferOffset = x * 2 + y * _screenCharWidth;

            if (frontBufferOffset < _frontBuffer.Length - 1)
            {
                if (_useColors && _useVirtualTerminal)
                {
                    string colorCode = $"\x1B[38;2;{fgRed};{fgGreen};{fgBlue}m";
                    if (bgRed.HasValue && bgGreen.HasValue && bgBlue.HasValue)
                    {
                        colorCode += $"\x1B[48;2;{bgRed};{bgGreen};{bgBlue}m";
                    }
                    colorCode += $"{character}\x1B[0m";
                    
                    _frontBuffer[frontBufferOffset] = character;
                    _frontBuffer[frontBufferOffset + 1] = ' ';
                    _colorBuffer[frontBufferOffset] = colorCode;
                    
                    // Set background color for the space character
                    if (bgRed.HasValue && bgGreen.HasValue && bgBlue.HasValue)
                    {
                        _bgColorBuffer[frontBufferOffset + 1] = $"\x1B[48;2;{bgRed};{bgGreen};{bgBlue}m \x1B[0m";
                    }
                }
                else
                {
                    _frontBuffer[frontBufferOffset] = character;
                    _frontBuffer[frontBufferOffset + 1] = ' ';
                }
            }
        }

        /// <summary>
        /// Returns the width of the screen.
        /// </summary>
        /// <returns>The screen width as an integer.</returns>
        public int GetScreenWidth() => _screenWidth;

        /// <summary>
        /// Returns the height of the terminal screen.
        /// </summary>
        /// <returns>An integer representing the height of the screen in characters.</returns>
        public int GetScreenHeight() => _screenHeight;

        /// <summary>
        /// Retrieves the aspect ratio of the terminal renderer.
        /// </summary>
        /// <returns>The aspect ratio as a float value.</returns>
        public float GetAspectRatio() => _aspectRatio;

        /// <summary>
        /// Cleans up resources used by the <see cref="TerminalRenderer"/> class.
        /// Sets the back buffer, front buffer, color buffers, and Z buffers to null.
        /// </summary>
        public void Terminate()
        {
            _zBackBuffer = null;
            _zFrontBuffer = null;
            _backBuffer = null;
            _frontBuffer = null;
            _colorBuffer = null;
            _bgColorBuffer = null;
        }

        /// <summary>
        /// Initializes the various buffers needed for rendering, including back buffer, front buffer,
        /// color buffer, background color buffer, and z-buffers.
        /// </summary>
        /// <param name="screenWidth">The width of the screen in characters.</param>
        /// <param name="screenHeight">The height of the screen in characters.</param>
        /// <param name="backgroundCharacter">The character used to fill the background buffer.</param>
        private void InitializeBuffers(int screenWidth, int screenHeight, char backgroundCharacter)
        {
            _screenCharWidth = screenWidth * 2 + 1;
            _screenCharCount = _screenCharWidth * screenHeight;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            _backBuffer = new char[_screenCharCount];
            _frontBuffer = new char[_screenCharCount];
            _colorBuffer = new string[_screenCharCount];
            _bgColorBuffer = new string[_screenCharCount];

            Array.Fill(_backBuffer, backgroundCharacter);

            for (int i = 0; i < screenHeight; i++)
            {
                int index = (screenWidth * 2) + i * _screenCharWidth;
                if (index < _backBuffer.Length)
                    _backBuffer[index] = '\n';
            }

            Array.Copy(_backBuffer, _frontBuffer, _screenCharCount);

            _pixelCount = screenWidth * screenHeight;
            _zBackBuffer = new float[_pixelCount];
            _zFrontBuffer = new float[_pixelCount];

            Array.Fill(_zBackBuffer, 1.0f);
            Array.Copy(_zBackBuffer, _zFrontBuffer, _pixelCount);

            _backgroundCharacter = backgroundCharacter;
        }

        /// <summary>
        /// Checks if the screen size has changed since the last render.
        /// </summary>
        /// <returns>
        /// True if the screen size has changed, otherwise false.
        /// </returns>
        private bool HasScreenSizeChanged()
        {
            return Console.WindowWidth != _lastWindowWidth || Console.WindowHeight != _lastWindowHeight;
        }

   
    }
}