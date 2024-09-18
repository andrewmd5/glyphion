namespace TermincalCraft;

/// <summary>
/// Provides methods for generating height maps and terrain based on sine wave patterns.
/// </summary>
public static class WorldGenerators
{
    /// <summary>
    /// Calculates the height map value at the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate in the world.</param>
    /// <param name="z">The z-coordinate in the world.</param>
    /// <returns>The calculated height value at the given coordinates.</returns>
    public static float GetHeightMap(int x, int z)
    {
        return 16f + 8f * (float)(Math.Sin((float)z / 20 + Math.Sin((float)z / 20)) + 
                                  2 * Math.Sin((float)x / 30 + Math.Sin((float)x / 20)));
    }

    /// <summary>
    /// Generates a sinc wave-based terrain type at the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the block.</param>
    /// <param name="y">The y-coordinate of the block.</param>
    /// <param name="z">The z-coordinate of the block.</param>
    /// <returns>A byte representing the block type based on the terrain height.</returns>
    public static byte SinwaveWorld(int x, int y, int z)
    {
        float heightMap = GetHeightMap(x, z);
        
        if (y < heightMap && y > heightMap - 2)
            return 1; // Return 1 for specific terrain height
        else if (y < heightMap - 2)
            return 2; // Return 2 for deeper areas
        
        return 0; // Return 0 for areas outside the height range
    }
}