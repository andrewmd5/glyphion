using System.Diagnostics;
using System.Numerics;
using Glyphion.Core;

namespace TermincalCraft;

public class Block
    {
        /// <summary>
        /// An array indicating the visibility of each face of a block.
        /// The array has a length of 6, corresponding to the six faces of a cube (Front, Right, Back, Left, Bottom, Top).
        /// If an element is true, the corresponding face should be rendered.
        /// </summary>
        public bool[] Face = new bool[6];  // Specifies which face to render

        /// <summary>
        /// Represents the type of a block within the game world.
        /// </summary>
        /// <remarks>
        /// BlockType is used to identify the specific type of a block, allowing for a variety of block types
        /// such as air, dirt, stone, etc. This information is essential for rendering and game logic purposes.
        /// </remarks>
        public byte BlockType = 0;

        public Block() { }

        public Block(byte blockType)
        {
            BlockType = blockType;
        }
    }

    public class Chunk
    {
        public Block[]? Data = null;
        public int ChunkX = int.MaxValue;
        public int ChunkY = int.MaxValue;

        /// <summary>
        /// Represents the Z coordinate of the chunk's position within the world.
        /// The value is initialized to <see cref="int.MaxValue"/> indicating an uninitialized chunk.
        /// </summary>
        public int ChunkZ = int.MaxValue;

        /// <summary>
        /// Retrieves a reference to the Block at the specified coordinates within the Chunk.
        /// </summary>
        /// <param name="x">The x-coordinate of the block within the chunk.</param>
        /// <param name="y">The y-coordinate of the block within the chunk.</param>
        /// <param name="z">The z-coordinate of the block within the chunk.</param>
        /// <returns>A reference to the Block at the specified coordinates.</returns>
        /// <exception cref="Exception">Thrown if the coordinates are out of range or the chunk's data is null.</exception>
        public Block GetBlockRef(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkManager.ChunkLength || y < 0 || y >= ChunkManager.ChunkLength || z < 0 || z >= ChunkManager.ChunkLength || Data == null)
            {
                throw new Exception("Incorrect Chunk Indexing in GetBlockRef");
            }
            return Data[x + z * ChunkManager.ChunkLength + y * ChunkManager.ChunkLength * ChunkManager.ChunkLength];
        }

        /// <summary>
        /// Retrieves the block type at the specified coordinates within the chunk.
        /// </summary>
        /// <param name="x">The x-coordinate within the chunk.</param>
        /// <param name="y">The y-coordinate within the chunk.</param>
        /// <param name="z">The z-coordinate within the chunk.</param>
        /// <returns>The block type at the specified coordinates.</returns>
        /// <exception cref="Exception">Thrown when the specified coordinates are out of bounds or the chunk data is null.</exception>
        public byte GetBlock(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkManager.ChunkLength || y < 0 || y >= ChunkManager.ChunkLength || z < 0 || z >= ChunkManager.ChunkLength || Data == null)
            {
                throw new Exception("Incorrect Chunk Indexing in GetBlock");
            }
            return Data[x + z * ChunkManager.ChunkLength + y * ChunkManager.ChunkLength * ChunkManager.ChunkLength].BlockType;
        }
    }

    public class ChunkManager
    {
        public const int ChunkLength = 16;

        /// <summary>
        /// Represents the total number of blocks contained in a single chunk.
        /// Each chunk is a 3D cube of blocks, with dimensions defined by <c>ChunkLength</c>.
        /// </summary>
        public const int ChunkSize = ChunkLength * ChunkLength * ChunkLength;

        private int _mapSize;

        /// <summary>
        /// Represents the length of the map in terms of chunks.
        /// Used to calculate other properties related to the map size and chunk positioning.
        /// </summary>
        private int _mapLength;

        /// <summary>
        /// An array storing the chunks in the game world.
        /// </summary>
        /// <remarks>
        /// The size of the array is determined by the map size and is initialized within the Init method.
        /// Each element in the array represents a chunk, which holds blocks and their associated data.
        /// </remarks>
        private Chunk[] _chunks;
        private readonly CraftGame _game;

        public ChunkManager(int distance, CraftGame game)
        {
            _game = game;
            _mapLength = distance;
            _mapSize = distance * distance * distance;

           
        }

        public void Init()
        {
            Cube.InitTextures(_game);
            Cube.InitTree();

            _chunks = new Chunk[_mapSize];
            for (int i = 0; i < _mapSize; i++)
            {
                _chunks[i] = new Chunk();
            }
        }

        // Hash function for unique chunk identifier
        /// <summary>
        /// Generates a unique hash value for a chunk based on its coordinates.
        /// </summary>
        /// <param name="chunkX">The X coordinate of the chunk.</param>
        /// <param name="chunkY">The Y coordinate of the chunk.</param>
        /// <param name="chunkZ">The Z coordinate of the chunk.</param>
        /// <returns>A unique hash value for the chunk.</returns>
        private int HashFunction(int chunkX, int chunkY, int chunkZ)
        {
            return ((chunkX + chunkZ * _mapLength + chunkY * _mapLength * _mapLength) + 536870911) % _mapSize;
        }

        // Returns block type at world position
        /// <summary>
        /// Retrieves the block type at a specified world position.
        /// </summary>
        /// <param name="blockX">The x-coordinate of the block in the world.</param>
        /// <param name="blockY">The y-coordinate of the block in the world.</param>
        /// <param name="blockZ">The z-coordinate of the block in the world.</param>
        /// <returns>A byte representing the block type at the specified coordinates. Returns 0 if the chunk is missing or the coordinates are invalid.</returns>
        public byte GetBlock(int blockX, int blockY, int blockZ)
        {
            int chunkX = GetChunkPosition(blockX);
            int chunkY = GetChunkPosition(blockY);
            int chunkZ = GetChunkPosition(blockZ);

            Chunk chunk = _chunks[HashFunction(chunkX, chunkY, chunkZ)];

            if (!(chunk.ChunkX == chunkX && chunk.ChunkY == chunkY && chunk.ChunkZ == chunkZ))
            {
                return 0; // Return air block for missing chunks
            }

            // World block position to chunk space block position
            int x = Mod(blockX, ChunkLength);
            int y = Mod(blockY, ChunkLength);
            int z = Mod(blockZ, ChunkLength);

            return chunk.GetBlock(x, y, z);
        }

        // Returns a reference to a block
        /// Returns a reference to a block at the specified world coordinates.
        /// This method calculates the appropriate chunk that contains the block
        /// and then retrieves the block reference from that chunk.
        /// Exceptions:
        /// - Throws an exception if there is incorrect indexing.
        /// Parameters:
        /// - blockX: The X-coordinate of the block in the world.
        /// - blockY: The Y-coordinate of the block in the world.
        /// - blockZ: The Z-coordinate of the block in the world.
        /// Returns:
        /// - A reference to the Block object at the specified coordinates.
        public Block GetBlockRef(int blockX, int blockY, int blockZ)
        {
            int chunkX = GetChunkPosition(blockX);
            int chunkY = GetChunkPosition(blockY);
            int chunkZ = GetChunkPosition(blockZ);

            Chunk chunk = _chunks[HashFunction(chunkX, chunkY, chunkZ)];

            if (!(chunk.ChunkX == chunkX && chunk.ChunkY == chunkY && chunk.ChunkZ == chunkZ))
            {
                throw new Exception("Incorrect Indexing in GetBlockRef");
            }

            int x = Mod(blockX, ChunkLength);
            int y = Mod(blockY, ChunkLength);
            int z = Mod(blockZ, ChunkLength);

            return chunk.GetBlockRef(x, y, z);
        }

        // Adds a block to the chunk
        /// Adds a block to the specified chunk coordinates.
        /// <param name="chunkX">The x-coordinate of the chunk.</param>
        /// <param name="chunkY">The y-coordinate of the chunk.</param>
        /// <param name="chunkZ">The z-coordinate of the chunk.</param>
        /// <returns>True if the block was successfully added, otherwise false.</returns>
        public bool AddBlock(int chunkX, int chunkY, int chunkZ)
        {
            Chunk chunk = _chunks[HashFunction(chunkX, chunkY, chunkZ)];

            if (chunk.ChunkX == chunkX && chunk.ChunkY == chunkY && chunk.ChunkZ == chunkZ)
            {
                return false; // Block already exists
            }

            if (chunk.Data == null)
            {
                chunk.Data = new Block[ChunkSize];
                for (int i = 0; i < ChunkSize; i++)
                {
                    chunk.Data[i] = new Block();
                }
            }

            chunk.ChunkX = chunkX;
            chunk.ChunkY = chunkY;
            chunk.ChunkZ = chunkZ;

            int blockX = chunkX * ChunkLength;
            int blockY = chunkY * ChunkLength;
            int blockZ = chunkZ * ChunkLength;

            for (int y = 0; y < ChunkLength; y++)
            {
                for (int z = 0; z < ChunkLength; z++)
                {
                    for (int x = 0; x < ChunkLength; x++)
                    {
                        chunk.GetBlockRef(x, y, z).BlockType = WorldGenerators.SinwaveWorld(blockX + x, blockY + y, blockZ + z);
                    }
                }
            }

            return true;
        }

        // Check if block exists
        /// <summary>
        /// Checks if a block exists at the specified world coordinates.
        /// </summary>
        /// <param name="blockX">The x-coordinate of the block in the world.</param>
        /// <param name="blockY">The y-coordinate of the block in the world.</param>
        /// <param name="blockZ">The z-coordinate of the block in the world.</param>
        /// <returns>True if the block exists at the specified coordinates, otherwise false.</returns>
        public bool DoesBlockExist(int blockX, int blockY, int blockZ)
        {
            int chunkX = GetChunkPosition(blockX);
            int chunkY = GetChunkPosition(blockY);
            int chunkZ = GetChunkPosition(blockZ);

            Chunk chunk = _chunks[HashFunction(chunkX, chunkY, chunkZ)];

            return chunk.ChunkX == chunkX && chunk.ChunkY == chunkY && chunk.ChunkZ == chunkZ;
        }

        // Meshing adjacent blocks
        /// <summary>
        /// Meshes all blocks adjacent to the specified block coordinates. This process is necessary to update the visual representation and ensure correct rendering of the blocks in the chunk.
        /// </summary>
        /// <param name="blockX">The X coordinate of the block position.</param>
        /// <param name="blockY">The Y coordinate of the block position.</param>
        /// <param name="blockZ">The Z coordinate of the block position.</param>
        public void MeshAdjacentBlocks(int blockX, int blockY, int blockZ)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if (DoesBlockExist(blockX + x, blockY + y, blockZ + z))
                        {
                            MeshBlock(blockX + x, blockY + y, blockZ + z);
                        }
                    }
                }
            }
        }

        // Mesh an individual block
        /// <summary>
        /// Meshes an individual block and determines which of its faces should be rendered based on the surrounding blocks.
        /// </summary>
        /// <param name="blockX">The X-coordinate of the block.</param>
        /// <param name="blockY">The Y-coordinate of the block.</param>
        /// <param name="blockZ">The Z-coordinate of the block.</param>
        public void MeshBlock(int blockX, int blockY, int blockZ)
        {
            Block currentBlock = GetBlockRef(blockX, blockY, blockZ);
            currentBlock.Face[0] = GetBlock(blockX, blockY, blockZ - 1) == 0;  // Front
            currentBlock.Face[1] = GetBlock(blockX + 1, blockY, blockZ) == 0;  // Right
            currentBlock.Face[2] = GetBlock(blockX, blockY, blockZ + 1) == 0;  // Back
            currentBlock.Face[3] = GetBlock(blockX - 1, blockY, blockZ) == 0;  // Left
            currentBlock.Face[4] = GetBlock(blockX, blockY - 1, blockZ) == 0;  // Bottom
            currentBlock.Face[5] = GetBlock(blockX, blockY + 1, blockZ) == 0;  // Top
        }

        // Render the world
        public void Render(Camera camera, float brightness, Vector3 sunPosition)
        {
            Vector3 sunDirection = Vector3.Normalize(sunPosition);
            float[] faceBrightness =
            {
                brightness * (2 + Vector3.Dot(sunDirection, new Vector3(0, 0, -1))) / 2,
                brightness * (2 + Vector3.Dot(sunDirection, new Vector3(1, 0, 0))) / 2,
                brightness * (2 + Vector3.Dot(sunDirection, new Vector3(0, 0, 1))) / 2,
                brightness * (2 + Vector3.Dot(sunDirection, new Vector3(-1, 0, 0))) / 2,
                brightness * (2 + Vector3.Dot(sunDirection, new Vector3(0, -1, 0))) / 2,
                (brightness + 0.2f) * (2 + Vector3.Dot(sunDirection, new Vector3(0, 1, 0))) / 2
            };

            for (int i = 0; i < _mapSize; i++)
            {
                Chunk chunk = _chunks[i];
                Vector3 chunkPos = new Vector3(chunk.ChunkX * ChunkLength, chunk.ChunkY * ChunkLength, chunk.ChunkZ * ChunkLength);
                chunkPos = Vector3.Transform(chunkPos - camera.Position, camera.RotXAxis() * camera.RotYAxis());

                if (chunk.Data == null || chunkPos.Z < -ChunkLength * 1.73f) continue;

                int blockX = chunk.ChunkX * ChunkLength;
                int blockY = chunk.ChunkY * ChunkLength;
                int blockZ = chunk.ChunkZ * ChunkLength;

                for (int y = 0; y < ChunkLength; y++)
                {
                    for (int z = 0; z < ChunkLength; z++)
                    {
                        for (int x = 0; x < ChunkLength; x++)
                        {
                            Block block = chunk.GetBlockRef(x, y, z);
                            Debug.Assert(block is not null);
                            if (block.BlockType == 0) continue;

                            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                            {
                                if (!block.Face[faceIndex]) continue;

                                Vector3 position = new Vector3(x + blockX, y + blockY, z + blockZ);
                                Vector3[] vertices =
                                {
                                    Cube.VerticeData[faceIndex * 4 + 0],
                                    Cube.VerticeData[faceIndex * 4 + 1],
                                    Cube.VerticeData[faceIndex * 4 + 2],
                                    Cube.VerticeData[faceIndex * 4 + 3]
                                };

                                _game.Engine.DrawPlainUV(position, vertices, (char)Cube.BlockType[6 * block.BlockType + faceIndex], faceBrightness[faceIndex], camera);
                            }
                        }
                    }
                }
            }
        }

        // Helper method to get the chunk position in the world space
        public int GetChunkPosition(int blockP)
        {
            return (blockP / ChunkLength) - (blockP % ChunkLength != 0 && blockP < 0 ? 1 : 0);
        }

        // Modulo operation to handle negative values properly
        private int Mod(int value, int mod)
        {
            int result = value % mod;
            return result < 0 ? result + mod : result;
        }

        public void MeshChunk(int chunkX, int chunkY, int chunkZ)
        {
            Chunk chunk = _chunks[HashFunction(chunkX, chunkY, chunkZ)];

            if (!(chunk.ChunkX == chunkX && chunk.ChunkY == chunkY && chunk.ChunkZ == chunkZ))
            {
                throw new Exception("Incorrect chunk indexing in MeshChunk");
            }

            int blockX = chunkX * ChunkLength;
            int blockY = chunkY * ChunkLength;
            int blockZ = chunkZ * ChunkLength;

            for (int y = 0; y < ChunkLength; y++)
            {
                for (int z = 0; z < ChunkLength; z++)
                {
                    for (int x = 0; x < ChunkLength; x++)
                    {
                        MeshBlock(blockX + x, blockY + y, blockZ + z);
                    }
                }
            }
        }

        /// Places a tree in the game world at the specified coordinates.
        /// The method will iterate through all the blocks that form the tree, calculate the world position for each
        /// tree block, and if the position is valid, it will set the block to the correct type (e.g., wood or leaves).
        /// <param name="x">The x-coordinate where the tree will be placed.</param>
        /// <param name="y">The y-coordinate where the tree will be placed.</param>
        /// <param name="z">The z-coordinate where the tree will be placed.</param>
        public void PlaceTree(int x, int y, int z)
        {
            // Iterate through all the blocks that form the tree (both trunk and leaves)
            for (int i = 0; i < Cube.TreeBlocks.Count; i++)
            {
                // Calculate the position of the tree block in the world
                Vector3 treeBlockPosition = Cube.TreeBlocks[i] + new Vector3(x, y, z);

                // Check if the block exists at that position
                if (DoesBlockExist((int)treeBlockPosition.X, (int)treeBlockPosition.Y, (int)treeBlockPosition.Z))
                {
                    // Get a reference to the block in the chunk and set its block type (e.g., wood or leaves)
                    Block block = GetBlockRef((int)treeBlockPosition.X, (int)treeBlockPosition.Y, (int)treeBlockPosition.Z);
                    block.BlockType = Cube.TreeAssemble[i];

                    // Mesh the adjacent blocks so they render correctly
                    MeshAdjacentBlocks((int)treeBlockPosition.X, (int)treeBlockPosition.Y, (int)treeBlockPosition.Z);
                }
            }
        }

    }