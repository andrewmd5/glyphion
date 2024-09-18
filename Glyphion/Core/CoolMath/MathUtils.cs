namespace Glyphion.Core.CoolMath;

/// Provides utility mathematical functions for the Glyphion engine.
/// /
internal static class MathUtils
{
    /// Calculates the Z value using the provided formula that incorporates the near and far clipping planes.
    /// <param name="z">The input z coordinate value used in the formula.</param>
    /// <returns>The computed z value after applying the formula.</returns>
    public static float ZFormula(float z)
    {
        const float far = 200f;
        const float near = 0.1f;
        return (far + near) / (far - near) + (1f / z) * ((-2f * far * near) / (far - near));
    }
}