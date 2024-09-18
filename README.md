# Glyphion Game Engine

Glyphion is an experimental game engine that renders 2D and 3D games to the terminal. Built with .NET, Glyphion offers a unique approach to game development, bringing the charm of ASCII graphics to modern game design. This is a hobby project.

<p align="center">
  <img src="./media/demo.gif" alt="Glyphion Engine Demo" width="600">
</p>

## Features

- **Terminal Rendering**: Create games that run directly in the console
- **2D and 3D Support**: Develop both 2D and 3D games using ASCII characters
- **Texture Support**: Load and render textures using PPM files
- **Font Integration**: Use Figlet fonts for text rendering
- **Multi-threaded Rendering**: Optimized performance with multi-threaded rendering
- **Level of Detail**: Implement LOD techniques for improved performance
- **Culling**: Efficient rendering with culling algorithms
- **Drawing Primitives**: Built-in support for rendering various shapes
- **Cross-platform**: Windows and macOS supported. 

## Getting Started

### Prerequisites

- .NET SDK 9

### Building the Project

Building Glyphion is straightforward:

```
dotnet build
```

### Creating a Game

To create a game using Glyphion, inherit from the `GlyphionGame` class:

```csharp
public class MyGame : GlyphionGame
{
    public MyGame(GlyphionEngineOptions options) : base(options) { }

    protected override void OnCreate()
    {
        // Initialize your game
    }

    protected override void OnUpdate()
    {
        // Update game logic
    }

    protected override void OnRender()
    {
        // Render your game
    }
}
```

## Example Project: TerminalCraft

Included in the repository is TerminalCraft, a basic clone of Minecraft that showcases Glyphion's capabilities. It demonstrates various features of the engine, including:

- Voxel-based world generation
- Day/night cycle
- Basic player movement and interaction
- UI rendering

## Contributing

While Glyphion is a personal hobby project, contributions are welcome! If you're interested in contributing, please:

1. Fork the repository
2. Create a new branch for your feature
3. Commit your changes
4. Push to your branch
5. Create a new Pull Request

Please note that as a hobby project, response times may vary.

## Planned Features

As a hobby project, Glyphion is constantly evolving. Here are some features planned for future implementation:

- **Terminal Graphics Protocol support**: Enhanced graphics capabilities for compatible terminals
- **Gamepad support**: Expand input options for a more versatile gaming experience
- **Entity System**: Streamline game object management and interactions
- **Built-in mesh loading from OBJ**: Easy integration of 3D models
- **Built-in animation system**: Simplify the process of adding animations to your games
- Other features as inspiration strikes!


## License

MIT
