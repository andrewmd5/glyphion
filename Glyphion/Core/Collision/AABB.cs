using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

namespace Glyphion.Core.Collision
{
    /// <summary>
    /// Represents an Axis-Aligned Bounding Box (AABB) used for collision detection in a 3D space.
    /// </summary>
    /// <remarks>
    /// This class offers functionalities to create, manipulate, and test collisions with axis-aligned bounding boxes.
    /// Commonly used in physics simulations and game development.
    /// </remarks>
    public class AABB
    {
        /// <summary>
        /// Gets or sets the position of the Axis-Aligned Bounding Box (AABB) in 3D space.
        /// </summary>
        /// <value>
        /// The position of the AABB, represented as a <see cref="Vector3"/> structure.
        /// </value>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the width of the AABB (Axis-Aligned Bounding Box) in 3D space.
        /// </summary>
        /// <value>
        /// The width of the AABB, represented as a single-precision floating point number.
        /// </value>
        public float Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the AABB (Axis-Aligned Bounding Box).
        /// </summary>
        /// <value>
        /// The height of the AABB, represented as a single-precision floating-point number.
        /// </value>
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the length of the AABB (Axis-Aligned Bounding Box).
        /// </summary>
        /// <value>
        /// The length of the AABB, represented as a float.
        /// </value>
        public float Length { get; set; }

        // Constructors
        /// <summary>
        /// Represents an Axis-Aligned Bounding Box (AABB) used in collision detection.
        /// </summary>
        /// <remarks>
        /// This class provides methods and properties to define and manipulate an axis-aligned
        /// bounding box, which is commonly used in various collision detection scenarios.
        /// </remarks>
        public AABB(Vector3 position, float width, float height, float length)
        {
            Position = position;
            Width = width;
            Height = height;
            Length = length;
        }

        /// <summary>
        /// Represents an Axis-Aligned Bounding Box (AABB) used in collision detection.
        /// </summary>
        /// <remarks>
        /// This class provides methods and properties to define and manipulate an Axis-Aligned
        /// Bounding Box, which is commonly used in various collision detection scenarios.
        /// </remarks>
        public AABB(Vector3 position, float width, float height)
            : this(position, width, height, width)
        {
        }

        /// <summary>
        /// Represents an Axis-Aligned Bounding Box (AABB) used in collision detection.
        /// </summary>
        /// <remarks>
        /// This class provides methods and properties to define and manipulate an axis-aligned
        /// bounding box, which is commonly used in various collision detection scenarios.
        /// </remarks>
        public AABB(Vector3 position, float width)
            : this(position, width, width, width)
        {
        }

        /// <summary>
        /// Tests for collision with another Axis-Aligned Bounding Box (AABB).
        /// </summary>
        /// <param name="other">The other AABB to test against.</param>
        /// <returns>True if a collision is detected; otherwise, false.</returns>
        public unsafe bool CollisionTest(AABB other)
        {
            // Fallback to scalar comparison if SIMD is not supported
            return ScalarCollisionTest(other);
        }


        // Fallback scalar method for collision detection
        /// <summary>
        /// Fallback scalar method for collision detection between this AABB and another AABB.
        /// </summary>
        /// <param name="other">The other AABB to test against.</param>
        /// <returns>True if a collision is detected; otherwise, false.</returns>
        private bool ScalarCollisionTest(AABB other)
        {
            bool xOverlap = Position.X <= other.Position.X + other.Width && Position.X + Width >= other.Position.X;
            bool yOverlap = Position.Y <= other.Position.Y + other.Height && Position.Y + Height >= other.Position.Y;
            bool zOverlap = Position.Z <= other.Position.Z + other.Length && Position.Z + Length >= other.Position.Z;

            return xOverlap && yOverlap && zOverlap;
        }

        /// <summary>
        /// Calculates collision response using SIMD where possible.
        /// </summary>
        /// <param name="other">The other AABB involved in the collision.</param>
        /// <param name="faceHit">Outputs the index of the face that was hit.</param>
        /// <returns>The new position after collision resolution.</returns>
        public Vector3 Collide(AABB other, out int faceHit)
        {
            // Use SIMD for calculating face distances
            float[] faceDistances = new float[6];

            faceDistances[0] = other.Position.Z + other.Length - Position.Z; // -Z face
            faceDistances[1] = Position.X + Width - other.Position.X; // +X face
            faceDistances[2] = Position.Z + Length - other.Position.Z;          // +Z face
            faceDistances[3] = other.Position.X + other.Width - Position.X;     // -X face
            faceDistances[4] = Position.Y + Height - other.Position.Y;          // +Y face
            faceDistances[5] = other.Position.Y + other.Height - Position.Y;    // -Y face

            // SIMD can help speed up the search for the smallest face distance
            faceHit = 0;
            float smallestDistance = faceDistances[0];

            if (Sse.IsSupported || AdvSimd.IsSupported)
            {
                Vector128<float> distances = Vector128.Create(faceDistances[0], faceDistances[1], faceDistances[2], faceDistances[3]);
                Vector128<float> minDistance = distances;

                for (int i = 1; i < 6; i++)
                {
                    if (faceDistances[i] < smallestDistance)
                    {
                        smallestDistance = faceDistances[i];
                        faceHit = i;
                    }
                }
            }
            else
            {
                // Fallback scalar approach
                for (int i = 1; i < faceDistances.Length; i++)
                {
                    if (faceDistances[i] < smallestDistance)
                    {
                        smallestDistance = faceDistances[i];
                        faceHit = i;
                    }
                }
            }

            const float epsilon = 0.002f; // Small offset to prevent sticking

            // Resolve collision based on the face that was hit
            return faceHit switch
            {
                0 => // -Z face
                    new Vector3(Position.X, Position.Y, other.Position.Z + other.Length + epsilon),
                1 => // +X face
                    new Vector3(other.Position.X - Width - epsilon, Position.Y, Position.Z),
                2 => // +Z face
                    new Vector3(Position.X, Position.Y, other.Position.Z - Length - epsilon),
                3 => // -X face
                    new Vector3(other.Position.X + other.Width + epsilon, Position.Y, Position.Z),
                4 => // +Y face
                    new Vector3(Position.X, other.Position.Y - Height - epsilon, Position.Z),
                5 => // -Y face
                    new Vector3(Position.X, other.Position.Y + other.Height + epsilon, Position.Z),
                _ => Position
            };
        }
    }
}
