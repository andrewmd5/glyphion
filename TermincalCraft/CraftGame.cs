using System.Numerics;
using Glyphion.Core;
using Glyphion.Core.IO;
using Glyphion.Core.Textures;

namespace TermincalCraft;

/// <summary>
/// Represents the main class for the CraftGame, inheriting from GlyphionGame.
/// It handles game initialization, updates, and rendering.
/// </summary>
public class CraftGame : GlyphionGame
{
    /// <summary>
    /// Manages the chunks within the game world, including their generation, rendering,
    /// block manipulation, and interactions with the player.
    /// </summary>
    private ChunkManager _chunkManager;

    /// <summary>
    /// Represents the time of day within the game world, used to simulate
    /// the passage of time and effect changes such as lighting and
    /// sun position.
    /// </summary>
    private float dayTime = 0;

    /// <summary>
    /// Used to keep track of the current time in the game world.
    /// </summary>
    private float worldTime = 1;

    /// <summary>
    /// Represents the position of the sun in the game world as a 3D vector.
    /// </summary>
    private Vector3 sunPosition = new Vector3(1, 1, 1);

    /// <summary>
    /// Represents the global brightness in the game, which is calculated
    /// based on the position of the sun and affects various visual aspects
    /// such as background color and object rendering.
    /// </summary>
    private float globalBrightness = 0;

    /// <summary>
    /// Thread responsible for generating chunks in the game. This thread runs independently
    /// of the main game loop to ensure that chunk generation does not block other game operations.
    /// </summary>
    private Thread _generationThread;

    /// <summary>
    /// Represents the thread dedicated to running the physics simulation in the game.
    /// </summary>
    private Thread _physicsThread;

    /// <summary>
    /// Specifies the distance in chunks that the game will render around the player.
    /// </summary>
    const int _renderDistance = 5;

    /// <summary>
    /// Represents the player within the game. The player is responsible for interacting
    /// with the game world through various functionalities such as controls, ray casting, and
    /// collision detection.
    /// </summary>
    private Player _player;

    /// <summary>
    /// Tracks the current position in chunk coordinates where new chunks are being generated.
    /// The value is updated to reflect the player's chunk position whenever a chunk needs to be generated or meshed.
    /// </summary>
    private Vector3 _generationPosition;

    /// <summary>
    /// A FIGlet font used for rendering text in the game.
    /// </summary>
    private FigletFont _font;

    // UI-related fields
    /// <summary>
    /// Indicates whether the UI (User Interface) should be shown or hidden. This is toggled
    /// by pressing the 'U' key during the game. When set to true, the UI elements are rendered.
    /// </summary>
    private bool _showUI = false;

    /// <summary>
    /// Index representing the currently selected shape in the list of shape options.
    /// Shape options are defined in the <c>_shapeNames</c> array which includes: Circle, Rectangle, Triangle, Line, and Arc.
    /// This variable is used to cycle through and select different shapes when the UI is visible.
    /// </summary>
    private int _selectedShape = 0;

    /// <summary>
    /// Array of names representing different shapes that can be selected in the game.
    /// </summary>
    private string[] _shapeNames = { "Circle", "Rectangle", "Triangle", "Line", "Arc" };

    /// <summary>
    /// Represents the color used for rendering UI elements in the game.
    /// </summary>
    private RgbColor _uiColor = new RgbColor(200, 200, 200);

    /// <summary>
    /// Represents the color used to highlight selected blocks or UI elements in the game.
    /// </summary>
    private RgbColor _highlightColor = new RgbColor(255, 255, 0);

    /// <summary>
    /// Represents the main game class for TerminalCraft, inheriting from GlyphionGame.
    /// This class initializes and manages the core game components such as player, chunk manager, physics, and rendering functionality.
    /// </summary>
    public CraftGame(GlyphionEngineOptions options) : base(options)
    {
    }

    /// <summary>
    /// Executes the game physics loop, updating the world state and player interactions based on a fixed time step.
    /// </summary>
    /// <remarks>
    /// The physics loop continuously runs while the game is active, updating dayTime and globalBrightness based
    /// on elapsed time. It handles user input for adjusting time and updates the player's position and interactions
    /// with the world, including collision detection.
    /// </remarks>
    private void RunPhysics()
    {
        var physicsDelta = new DeltaTime();
        while (IsRunning)
        {
            if (dayTime > 209.5f) dayTime = 0;
            dayTime += physicsDelta.GetDeltaTime();

            if (Keyboard.IsKeyDown('G'))
            {
                dayTime += 20 * physicsDelta.GetDeltaTime();
              
            }

            physicsDelta.Update();
            globalBrightness = (sunPosition.Y + 50) / 100 + 0.1f;
            sunPosition = Vector3.Transform(new Vector3(10, 0, -50),
                Matrix4x4.CreateRotationX(dayTime * 0.03f) * Matrix4x4.CreateRotationY(0.5f));

            Engine.ChangeBackground(AsciiTexture.GetAsciiGradient(globalBrightness * 0.8f));

            _player.Controls(physicsDelta.GetDeltaTime());
            _player.CastRay(_chunkManager, physicsDelta.GetDeltaTime());
            _player.WorldCollision(_chunkManager);
        }
    }

    /// <summary>
    /// Continuously generates chunks around the player's current position. The player's position
    /// is used to determine the chunk coordinates, and the chunks are generated within a specified
    /// render distance. This method runs in a background thread as long as the game is running.
    /// </summary>
    private void GenerateChunks()
    {
        while (IsRunning)
        {
            var playerChunkPosition = new Vector3(
                _chunkManager.GetChunkPosition((int)_player.Position.X),
                _chunkManager.GetChunkPosition((int)_player.Position.Y),
                _chunkManager.GetChunkPosition((int)_player.Position.Z));

            var genOffset = _renderDistance / 2;

            if (_generationPosition != playerChunkPosition)
            {
                _generationPosition = playerChunkPosition;

                for (var chunkY = (int)(_generationPosition.Y - genOffset);
                     chunkY < _generationPosition.Y + genOffset;
                     chunkY++)
                for (var chunkZ = (int)(_generationPosition.Z - genOffset);
                     chunkZ < _generationPosition.Z + genOffset;
                     chunkZ++)
                for (var chunkX = (int)(_generationPosition.X - genOffset);
                     chunkX < _generationPosition.X + genOffset;
                     chunkX++)
                    _chunkManager.AddBlock(chunkX, chunkY, chunkZ);

                for (var chunkY = (int)(_generationPosition.Y - genOffset);
                     chunkY < _generationPosition.Y + genOffset;
                     chunkY++)
                for (var chunkZ = (int)(_generationPosition.Z - genOffset);
                     chunkZ < _generationPosition.Z + genOffset;
                     chunkZ++)
                for (var chunkX = (int)(_generationPosition.X - genOffset);
                     chunkX < _generationPosition.X + genOffset;
                     chunkX++)
                    _chunkManager.MeshChunk(chunkX, chunkY, chunkZ);

                for (var chunkZ = (int)(_generationPosition.Z - genOffset);
                     chunkZ < _generationPosition.Z + genOffset;
                     chunkZ++)
                for (var chunkX = (int)(_generationPosition.X - genOffset);
                     chunkX < _generationPosition.X + genOffset;
                     chunkX++)
                {
                    var blockX = chunkX * ChunkManager.ChunkLength +
                                 QuickRand(QuickRand(chunkX) + chunkZ * _renderDistance) % ChunkManager.ChunkLength;
                    var blockZ = chunkZ * ChunkManager.ChunkLength +
                                 QuickRand(QuickRand(chunkZ) + chunkX * _renderDistance) % ChunkManager.ChunkLength;
                    if (!(QuickRand(QuickRand(blockX) + QuickRand(blockZ)) % 2 == 0))
                        _chunkManager.PlaceTree(blockX, (int)WorldGenerators.GetHeightMap(blockX, blockZ), blockZ);
                }
            }
        }
    }

    /// <summary>
    /// Initializes the game components and starts necessary background threads for chunk generation and physics.
    /// </summary>
    /// <remarks>
    /// This method is called once when the game is created. It loads the Figlet font, initializes the chunk manager,
    /// starts the chunk generation and physics threads, and positions the player in the world.
    /// </remarks>
    protected override void OnCreate()
    {
        _font = FigletFont.Load("Textures/caligraphy.flf");
        _chunkManager = new ChunkManager(_renderDistance, this);
        _generationThread = new Thread(GenerateChunks)
        {
            IsBackground = true
        };
        _physicsThread = new Thread(RunPhysics)
        {
            IsBackground = true
        };
        _player = new Player();
        _player.Position = new Vector3(32, WorldGenerators.GetHeightMap(32, 32) + 2, 32);
        _chunkManager.Init();
        _generationThread.Start();
        _physicsThread.Start();
    }

    /// <summary>
    /// Handles the game update logic for CraftGame. This method is called periodically by the game engine.
    /// It toggles the UI visibility when the 'U' key is pressed and cycles through selectable shapes
    /// when the 'T' key is pressed while the UI is visible.
    /// </summary>
    protected override void OnUpdate()
    {
        // Toggle UI visibility
        if (Keyboard.IsKeyDown('U'))
        {
            _showUI = !_showUI;
        }

        // Cycle through shapes when UI is visible
        if (_showUI && Keyboard.IsKeyDown('T'))
        {
            _selectedShape = (_selectedShape + 1) % _shapeNames.Length;
        }
    }

    /// <summary>
    /// Handles the rendering logic for the game, including drawing the sun, rendering clouds,
    /// and managing the rendering of chunks and UI elements.
    /// </summary>
    protected override void OnRender()
    {
      
        // Draw a sun with a gradient character and RGB values for the sun-like glow
        Engine.DrawCircle(new Vector3(0, 0, 0), 1, ' ', 255, 200, 50);

        Sun(sunPosition, 14, 1, _player);

        // Render clouds
        for (var z = -_renderDistance; z < _renderDistance; z++)
        {
            for (var x = -_renderDistance; x < _renderDistance; x++)
            {
                var blockX = (int)(_generationPosition.X - x);
                var blockZ = (int)(_generationPosition.Z - z);

                float blkX = blockX * ChunkManager.ChunkLength +
                             QuickRand(QuickRand(blockX) + QuickRand(blockZ)) % ChunkManager.ChunkLength;
                float blkZ = blockZ * ChunkManager.ChunkLength +
                             QuickRand(QuickRand(blockZ) + QuickRand(blockX)) % ChunkManager.ChunkLength;

                if (QuickRand(QuickRand(blockX) + QuickRand(blockZ * 100)) % 6 != 0)
                    Engine.DrawCube(new Vector3(blkX, 60 + QuickRand(QuickRand(blockX) + QuickRand(blockZ + blockZ)) % 30, blkZ),
                        5, Math.Clamp(0.1f + globalBrightness, 0, 0.99f), _player);
            }
        }
        
     

        _chunkManager.Render(_player, Math.Clamp(globalBrightness, 0.4f, 0.60f), sunPosition);

        if (_showUI)
        {
            RenderUI();
        }
    }

    // New fields for UI
    /// <summary>
    /// Array containing names of the different types of blocks available in the game.
    /// </summary>
    private readonly string[] _blockNames = { "Air", "Grass", "Dirt", "Stone", "Wood", "Leaves", "Sand", "Water" };

    /// <summary>
    /// Represents an array of characters used as icons for different blocks in the game.
    /// </summary>
    /// <remarks>
    /// Each character in the array is used to visually represent a different type of block within the game's user interface,
    /// such as the hotbar or the world rendering. The icons include various patterns and symbols to denote distinct block types.
    /// </remarks>
    private readonly char[] _blockIcons = { ' ', '▒', '▓', '█', '░', '♠', ':', '~' };

   // New fields for UI scaling
   /// <summary>
   /// Represents the scaling factor for the game's User Interface (UI).
   /// This factor determines the size of UI elements relative to the game's resolution.
   /// </summary>
   private float _uiScale;

   /// <summary>
   /// Stores the width of the user interface after scaling based on the screen dimensions and the UI scale factor.
   /// This allows for UI elements to be resized proportionally to fit different screen sizes.
   /// </summary>
   private int _scaledWidth;

   /// <summary>
   /// Represents the height of the user interface scaled based on the current screen dimensions and the UI scale factor.
   /// This value is dynamically calculated and used to ensure the UI elements are proportionate to the screen size.
   /// </summary>
    private int _scaledHeight;

    /// <summary>
    /// Updates the UI scale and adjusts the scaled dimensions based on the current screen width and height.
    /// This method ensures that the UI scale maintains a minimum value and scales
    /// the UI dimensions proportionally within the game screen.
    /// </summary>
    private void UpdateUIScale()
    {
        int screenWidth = Engine.ScreenWidth;
        int screenHeight = Engine.ScreenHeight;
        
        // Increase base scale and adjust calculation for larger UI
        _uiScale = Math.Min(screenWidth / 80f, screenHeight / 25f);
        _scaledWidth = (int)(screenWidth / _uiScale);
        _scaledHeight = (int)(screenHeight / _uiScale);

        // Ensure minimum scale
        _uiScale = Math.Max(_uiScale, 1.5f);
    }

    /// <summary>
    /// Renders the user interface elements on the game screen, including the hotbar with block icons,
    /// the crosshair, debug information, and day/night cycle information.
    /// </summary>
    private void RenderUI()
    {
        UpdateUIScale();

        // Hotbar dimensions
        int hotbarWidth = (int)(18 * _uiScale);
        int hotbarHeight = (int)(3 * _uiScale);
        int hotbarX = Engine.ScreenWidth / 2 - hotbarWidth / 2;
        int hotbarY = Engine.ScreenHeight - hotbarHeight - 1;

        // Draw hotbar frame
        Engine.DrawFrame(new Vector2(hotbarX, hotbarY), new Vector2(hotbarX + hotbarWidth, hotbarY + hotbarHeight), _uiColor);

        // Draw hotbar slots
        for (int i = 0; i < 8; i++)
        {
            int slotX = hotbarX + (int)(_uiScale) + i * (int)(2 * _uiScale);
            Engine.WriteText(new Vector2(slotX, hotbarY + (int)_uiScale), _blockIcons[i].ToString(), _uiColor);
            Engine.WriteText(new Vector2(slotX, hotbarY + (int)(16 * _uiScale)), (i + 1).ToString(), _uiColor);
        }

        // Highlight selected block
        int selectedX = hotbarX + (int)_uiScale + (_player.blockSelected - 1) * (int)(2 * _uiScale);
        Engine.DrawRectangle(
            new Vector2(selectedX - 1, hotbarY), 
            new Vector2(selectedX + (int)(_uiScale), hotbarY + hotbarHeight), 
            _highlightColor
        );

        // Draw crosshair
        Engine.WriteText(new Vector2(Engine.ScreenWidth / 2, Engine.ScreenHeight / 2), "+", _highlightColor);

        // Draw debug info (top-left corner)

        Engine.WriteText(new Vector2(2, 2), $"X: {_player.Position.X:F1} Y: {_player.Position.Y:F1} Z: {_player.Position.Z:F1}", _uiColor);
        Engine.WriteText(new Vector2(2, 3), $"Selected: {_blockNames[_player.blockSelected]}", _uiColor);

        // Draw day/night cycle info (top-right corner)
        string timeOfDay = dayTime < 104.75f ? "Day" : "Night";
        Engine.WriteText(new Vector2(Engine.ScreenWidth - 15, 1), $"Time: {timeOfDay}", _uiColor);
        Engine.WriteText(new Vector2(Engine.ScreenWidth - 15, 2), $"Brightness: {globalBrightness:F2}", _uiColor);

        // Draw UI scale info (for debugging)
        Engine.WriteText(new Vector2(2, 4), $"UI Scale: {_uiScale:F2}", _uiColor);
    }


    /// Generates a pseudo-random number based on the given seed.
    /// <param name="seed">The initial value used to generate the pseudo-random number.</param>
    /// <returns>A pseudo-random number generated from the seed.</returns>
    private int QuickRand(int seed)
    {
        const int a = 1664525;
        const int c = 1013904223;
        seed = (a * seed + c) & 0x7fffffff;
        return seed;
    }

    /// <summary>
    /// Draws the sun-like object in the game, adjusting its position relative to the camera and applying
    /// a gradient based on brightness.
    /// </summary>
    /// <param name="position">The 3D position of the sun.</param>
    /// <param name="size">The size of the sun.</param>
    /// <param name="brightness">The brightness of the sun, clamped between 0.0 and 1.0.</param>
    /// <param name="camera">The camera object to adjust the sun's position relative to.</param>
    private void Sun(Vector3 position, float size, float brightness, Camera camera)
    {
        // Adjust the position relative to the camera
        position -= camera.Position;
        position = Vector3.Transform(position, (Matrix4x4)camera.RotXAxis());
        position = Vector3.Transform(position, (Matrix4x4)camera.RotYAxis());

        if (position.Z == 0) return; // Prevent division by zero
        position = new Vector3(position.X / position.Z, position.Y / position.Z, position.Z);

        if (position.Z < 0) return;

        // Adjust the color based on brightness
        brightness = Math.Clamp(brightness, 0.0f, 1.0f);
        var r = (byte)(255 * brightness); // Red color intensity based on brightness
        var g = (byte)(200 * brightness); // Green color (a bit lower for sun-like effect)
        var b = (byte)(50 * brightness); // Blue color (lower for warm tone)

        // Draw the sun-like circle with calculated brightness and color
        Engine.DrawCircle(new Vector3(position.X, position.Y, 1), size, AsciiTexture.GetAsciiGradient(brightness), r, g, b);
    }
}