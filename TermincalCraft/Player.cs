using System.Diagnostics;
using System.Numerics;
using Glyphion.Core;
using Glyphion.Core.Collision;
using Glyphion.Core.IO;

namespace TermincalCraft;

/// <summary>
/// Represents a player in the game, extending the Camera class to provide player-specific
/// functionality such as block selection, casting rays for interaction, collision handling with the world,
/// and player controls.
/// </summary>
public class Player : Camera
{
    /// <summary>
    /// Indicates whether the player is currently in flying mode.
    /// </summary>
    private bool isFlying = false;

    /// <summary>
    /// A boolean indicating whether the player is currently on the ground.
    /// </summary>
    private bool onGround = false;

    /// <summary>
    /// Represents the velocity of the player in the game world.
    /// It is a Vector3 object describing movement speed along the X, Y, and Z axes.
    /// Velocity is used for various player interactions such as collision detection
    /// and movement updates.
    /// </summary>
    private Vector3 velocity = Vector3.Zero;

    /// <summary>
    /// Represents the velocity of the player's view direction in a 2D space.
    /// This variable is used to control the speed and direction of the player's view movements.
    /// </summary>
    private Vector2 viewVelocity = Vector2.Zero;

    /// <summary>
    /// Represents the currently selected block type for placement in the game world.
    /// </summary>
    public byte blockSelected = 1;

    /// <summary>
    /// Represents the time elapsed since the last block placement or removal action.
    /// </summary>
    private float placeTime = 0;

    // Constants for virtual key codes (for arrow keys)
    /// <summary>
    /// Represents the virtual key code for the left arrow key.
    /// </summary>
    private const int VK_LEFT = 0x25;

    /// <summary>
    /// Specifies the virtual-key code for the right arrow key.
    /// This key is typically used to move the view or the player to the right in games.
    /// </summary>
    private const int VK_RIGHT = 0x27;

    /// <summary>
    /// Virtual key code for the "Up Arrow" key.
    /// </summary>
    private const int VK_UP = 0x26;

    /// <summary>
    /// Represents the virtual key code for the "Down Arrow" key.
    /// Used to detect when the "Down Arrow" key is pressed in keyboard input operations.
    /// </summary>
    private const int VK_DOWN = 0x28;

    /// <summary>
    /// Casts a ray from the player's position in the direction they are facing.
    /// The method determines if a block is hit within a certain distance and allows for block placement or removal within the game world.
    /// </summary>
    /// <param name="world">The game world represented by the ChunkManager object.</param>
    /// <param name="frameTime">The time elapsed since the last frame update.</param>
    public void CastRay(ChunkManager world, float frameTime)
    {
        Vector3 rayStart = Position + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 rayDir = Direction;
        float maxDistance = 5f;
        float rayStep = 0.1f;

        Vector3 currentPos = rayStart;
        Vector3 lastPos = rayStart;
        float distanceTraveled = 0f;

        while (distanceTraveled < maxDistance)
        {
            int x = FloatToInt(currentPos.X);
            int y = FloatToInt(currentPos.Y);
            int z = FloatToInt(currentPos.Z);

            if (world.GetBlock(x, y, z) != 0)
            {
                break;
            }

            lastPos = currentPos;
            currentPos += rayDir * rayStep;
            distanceTraveled += rayStep;
        }

        if (distanceTraveled >= maxDistance)
        {
            return; // No block in range
        }

        // Handle block placement and removal
        if (Keyboard.IsKeyDown(0x0D) && placeTime > 0.2f)  // Enter key for placement
        {
            int px = FloatToInt(lastPos.X);
            int py = FloatToInt(lastPos.Y);
            int pz = FloatToInt(lastPos.Z);

            if (world.DoesBlockExist(px, py, pz) && world.GetBlock(px, py, pz) == 0)
            {
                world.GetBlockRef(px, py, pz).BlockType = blockSelected;
                world.MeshAdjacentBlocks(px, py, pz);
            }
            placeTime = 0;
        }
        else if (Keyboard.IsKeyDown(0x08) && placeTime > 0.2f)  // Backspace key for removal
        {
            int px = FloatToInt(currentPos.X);
            int py = FloatToInt(currentPos.Y);
            int pz = FloatToInt(currentPos.Z);

            if (world.DoesBlockExist(px, py, pz) && world.GetBlock(px, py, pz) != 0)
            {
                world.GetBlockRef(px, py, pz).BlockType = 0;
                world.MeshAdjacentBlocks(px, py, pz);
            }
            placeTime = 0;
        }

        placeTime += frameTime;
    }

    /// <summary>
    /// Checks for collisions between the player and the blocks in the world,
    /// adjusting the player's position and velocity based on any collisions detected.
    /// </summary>
    /// <param name="world">The ChunkManager instance representing the game world.</param>
    public void WorldCollision(ChunkManager world)
    {
        AABB playerAABB = new AABB(Position - new Vector3(0, 1, 0), 0.8f, 1.8f);
        int pX = FloatToInt(Position.X);
        int pY = FloatToInt(Position.Y);
        int pZ = FloatToInt(Position.Z);

        for (int y = pY - 1; y <= pY + 1; y++)
        {
            for (int z = pZ - 1; z <= pZ + 1; z++)
            {
                for (int x = pX - 1; x <= pX + 1; x++)
                {
                    var block = world.GetBlock(x, y, z);
                    if (block == 0) continue;

                    AABB blockAABB = new AABB(new Vector3(x, y, z), 1);
                    if (playerAABB.CollisionTest(blockAABB))
                    {
                        int faceHit = -1;
                        playerAABB.Position = playerAABB.Collide(blockAABB, out faceHit);

                        switch (faceHit)
                        {
                            case 0: velocity.Z = 0; break;
                            case 1: velocity.X = 0; break;
                            case 2: velocity.Z = 0; break;
                            case 3: velocity.X = 0; break;
                            case 5:
                                velocity.Y = 0;
                                onGround = true;
                                break;
                        }
                    }
                }
            }
        }
        Position = playerAABB.Position + new Vector3(0, 1, 0);
    }

    /// <summary>
    /// Handles the player's control logic including movement, view direction, flying, jumping, and block selection.
    /// </summary>
    /// <param name="frameTime">The time elapsed since the last frame, used for smooth and consistent movement and updates.</param>
    public void Controls(float frameTime)
{
    Vector2 movementDirection = Vector2.Zero;
    float speed = 6;
    float acceleration = 50;
    float sensitivity = 2;
    float friction = 8;

    // Update view direction based on arrow key input
    if (Keyboard.IsKeyDown(VK_LEFT))
        View = View with { X = View.X - sensitivity * frameTime };
    if (Keyboard.IsKeyDown(VK_RIGHT))
        View = View with { X = View.X + sensitivity * frameTime };
    if (Keyboard.IsKeyDown(VK_DOWN))
        View = View with { Y = View.Y - sensitivity * frameTime };
    if (Keyboard.IsKeyDown(VK_UP))
        View = View with { Y = View.Y + sensitivity * frameTime };

    // Clamp vertical view to prevent over-rotation
    View = View with { Y = Math.Clamp(View.Y, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f) };

    // Calculate direction vector based on updated view
    Matrix4x4 rotY = Matrix4x4.CreateRotationY(View.X);
    Matrix4x4 rotX = Matrix4x4.CreateRotationX(View.Y);
    Direction = Vector3.Transform(Vector3.Transform(Vector3.UnitZ, rotX), rotY);

    // Calculate right vector for strafing
    Vector3 right = Vector3.Cross(Vector3.UnitY, Direction);

    // Camera movement
    if (Keyboard.IsKeyDown('W'))
        movementDirection += new Vector2(Direction.X, Direction.Z);
    if (Keyboard.IsKeyDown('S'))
        movementDirection -= new Vector2(Direction.X, Direction.Z);
    if (Keyboard.IsKeyDown('A'))
        movementDirection -= new Vector2(right.X, right.Z);
    if (Keyboard.IsKeyDown('D'))
        movementDirection += new Vector2(right.X, right.Z);

    if (movementDirection.Length() > 1)
        movementDirection = Vector2.Normalize(movementDirection);

    // Rest of your control logic (flying, jumping, etc.) remains the same
    if (Keyboard.IsKeyDown('C'))
        velocity.Y = -5;
    if (Keyboard.IsKeyDown('F'))
        isFlying = true;
    if (Keyboard.IsKeyDown('X'))
        isFlying = false;
    if (Keyboard.IsKeyDown(' ') && (onGround || isFlying))
    {
        velocity.Y = 6;
        onGround = false;
    }

    // Flying and normal movement
    if (isFlying)
    {
        if (velocity.X * velocity.X + velocity.Z * velocity.Z < speed * speed)
        {
            velocity.X += acceleration * 0.2f * movementDirection.X * frameTime;
            velocity.Z += acceleration * 0.2f * movementDirection.Y * frameTime;
        }
        velocity.Y -= friction * velocity.Y * frameTime;
        velocity.X -= friction * 0.2f * velocity.X * frameTime;
        velocity.Z -= friction * 0.2f * velocity.Z * frameTime;
    }
    else
    {
        if (velocity.X * velocity.X + velocity.Z * velocity.Z < speed * speed)
        {
            velocity.X += acceleration * movementDirection.X * frameTime;
            velocity.Z += acceleration * movementDirection.Y * frameTime;
        }
        velocity.Y -= 15 * frameTime;
        velocity.X -= friction * velocity.X * frameTime;
        velocity.Z -= friction * velocity.Z * frameTime;
    }

    Position += velocity * frameTime;

    // Block Selection (unchanged)
    if (Keyboard.IsKeyDown('1')) blockSelected = 1;
    if (Keyboard.IsKeyDown('2')) blockSelected = 2;
    if (Keyboard.IsKeyDown('3')) blockSelected = 3;
    if (Keyboard.IsKeyDown('4')) blockSelected = 4;
    if (Keyboard.IsKeyDown('5')) blockSelected = 5;
    if (Keyboard.IsKeyDown('6')) blockSelected = 6;
    if (Keyboard.IsKeyDown('7')) blockSelected = 7;
}

    /// <summary>
    /// Converts a floating-point value to an integer value, with special handling for negative values.
    /// </summary>
    /// <param name="value">The floating-point value to convert.</param>
    /// <returns>The converted integer value.</returns>
    private int FloatToInt(float value)
    {
        return value < 0 ? (int)(value - 1) : (int)value;
    }

    /// <summary>
    /// Enables the free camera mode, allowing unrestricted movement through the game world based on the frame time.
    /// </summary>
    /// <param name="frameTime">The delta time for the current frame, used to calculate movement.</param>
    public override void FreeCam(float frameTime)
    {
        throw new NotImplementedException();
    }
}