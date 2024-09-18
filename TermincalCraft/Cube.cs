using System.Numerics;
using Glyphion.Core;

namespace TermincalCraft;

/// <summary>
/// The Cube class contains static definitions and methods related to the 3D cubic blocks used in the game,
/// including their vertices, textures, and initialization procedures.
/// </summary>
public static class Cube
{
    /// <summary>
    /// A list representing the blocks that make up a tree in the game world.
    /// The tree consists of both trunk and leaves, defined by their positions in a 3D space.
    /// </summary>
    public static List<Vector3> TreeBlocks = new List<Vector3>();

    /// <summary>
    /// Represents a list of byte values that indicate the types of blocks forming a tree.
    /// This list is used to determine the block type (e.g., wood, leaves) for each block in a tree structure.
    /// </summary>
    public static List<byte> TreeAssemble = new List<byte>();

    /// <summary>
    /// Initializes the tree structure by populating the TreeBlocks and TreeAssemble lists.
    /// </summary>
    /// <remarks>
    /// The method generates a tree-like structure with a specified trunk and foliage.
    /// It starts by creating a trunk base at a given position and adds blocks for the foliage
    /// based on predefined coordinates and conditions.
    /// </remarks>
    public static void InitTree()
    {
        Vector3 basePosition = new Vector3(0, 0, 0);

        for (int y = 4; y <= 8; y++)
        {
            for (int z = -2; z <= 2; z++)
            {
                for (int x = -2; x <= 2; x++)
                {
                    if (x * x + z * z + (y - 5) * (y - 5) < 6 || y < 6)
                    {
                        TreeBlocks.Add(new Vector3(x, y, z));
                        TreeAssemble.Add(7);
                    }
                }
            }
        }

        for (int i = 0; i <= 3; i++)
        {
            TreeBlocks.Add(basePosition + new Vector3(0, i, 0));
            TreeAssemble.Add(4);
        }
    }

    /// <summary>
    /// Initializes the textures for the game by adding various textures to the engine.
    /// </summary>
    /// <param name="game">An instance of the CraftGame class used to access the engine for texture addition.</param>
    public static void InitTextures(CraftGame game)
    {
        var engine = game.Engine;
        // Grass
        engine.AddTexturePpm((char)1, "Textures/grass_block_side.ppm");
        engine.AddTexturePpm((char)2, "Textures/grass_block_top.ppm");
        engine.AddTexturePpm((char)3, "Textures/dirt.ppm");

        // Cobblestone
        engine.AddTexturePpm((char)4, "Textures/cobblestone.ppm");

        // Oak plank
        engine.AddTexturePpm((char)5, "Textures/oak_planks.ppm");

        // Oak log
        engine.AddTexturePpm((char)6, "Textures/oak_log.ppm");
        engine.AddTexturePpm((char)7, "Textures/oak_log_top.ppm");

        // Crafting table
        engine.AddTexturePpm((char)8, "Textures/crafting_table_side.ppm");
        engine.AddTexturePpm((char)9, "Textures/crafting_table_front.ppm");
        engine.AddTexturePpm((char)10, "Textures/crafting_table_top.ppm");

        // Furnace
        engine.AddTexturePpm((char)11, "Textures/furnace_front.ppm");
        engine.AddTexturePpm((char)12, "Textures/furnace_side.ppm");
        engine.AddTexturePpm((char)13, "Textures/furnace_top.ppm");

        // Leaves
        engine.AddTexturePpm((char)14, "Textures/oak_leaves.ppm");
    }

    /// <summary>
    /// A static readonly list defining texture indices for various block faces in the game.
    /// The list contains texture data for different types of blocks, where each block type
    /// is represented by 6 entries corresponding to its 6 faces (front, back, top, bottom, left, and right).
    /// Example block types include air, grass, cobblestone, oak plank, log, crafting table, furnace, and oak leaves.
    /// Used by the rendering engine to associate specific textures with the faces of blocks
    /// during the drawing process.
    /// </summary>
    public static readonly List<uint> BlockType = new List<uint>
    {
        // Air
        0, 0, 0, 0, 0, 0,

        // Grass 1
        1, 1, 1, 1, 3, 2,

        // Cobblestone 2
        4, 4, 4, 4, 4, 4,

        // Oak plank 3
        5, 5, 5, 5, 5, 5,

        // Log 4
        6, 6, 6, 6, 7, 7,

        // Crafting table 5
        9, 8, 9, 8, 10, 10,

        // Furnace 6
        11, 12, 12, 12, 13, 13,

        // Oak leaves
        14, 14, 14, 14, 14, 14
    };

    /// <summary>
    /// VerticeData provides the vertex positions for all six faces of a cube.
    /// Each face is represented by four vertices in a counter-clockwise order.
    /// These vertices define the geometry of the cube used in 3D space rendering.
    /// </summary>
    public static readonly Vector3[] VerticeData =
    {
        // Front
        new Vector3(-0.5f, -0.5f, -0.5f), // Bottom Left
        new Vector3(0.5f, -0.5f, -0.5f),  // Bottom Right
        new Vector3(0.5f, 0.5f, -0.5f),   // Top Right
        new Vector3(-0.5f, 0.5f, -0.5f),  // Top Left

        // Right
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),

        // Back
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),

        // Left
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),

        // Bottom
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),

        // Top
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f)
    };
}