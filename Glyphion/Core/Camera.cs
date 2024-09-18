using System.Numerics;

namespace Glyphion.Core
{
    /// <summary>
    /// Represents an abstract camera in a 3D space with position, direction, and view properties.
    /// </summary>
    public abstract class Camera
{
    /// <summary>
    /// Gets or sets the position of the camera in 3D space.
    /// </summary>
    /// <remarks>
    /// The position is represented as a <see cref="Vector3"/> structure, which includes the X, Y, and Z coordinates.
    /// </remarks>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Property representing the current view angles of the Camera.
    /// </summary>
    /// <remarks>
    /// The view angles are stored as a vector of two components (X and Y),
    /// corresponding to the horizontal and vertical rotations respectively.
    /// Updating this property will change the direction in which the Camera is looking.
    /// The vertical view angle is clamped to prevent over-rotation.
    /// </remarks>
    public Vector2 View { get; protected set; }

    /// <summary>
    /// Gets or sets the direction vector of the camera.
    /// </summary>
    /// <remarks>
    /// This property represents the orientation of the camera in a 3D space.
    /// It is typically calculated based on the camera's view angles and is used
    /// for various operations like movement, ray casting, and alignment with the
    /// viewing direction.
    /// </remarks>
    public Vector3 Direction { get;protected set; }

    /// <summary>
    /// Represents a camera in 3D space, providing functionality for positioning
    /// and viewing direction, as well as rotation around the X and Y axes.
    /// </summary>
    public Camera()
    {
        Position = Vector3.Zero;
        View = Vector2.Zero;
        Direction = Vector3.UnitZ;
    }

    /// <summary>
    /// Represents a camera with position, viewing direction, and view parameters.
    /// </summary>
    public Camera(Vector3 position, Vector2 view, Vector3 direction)
    {
        Position = position;
        View = view;
        Direction = direction;
    }

    /// Rotates the view along the X-axis by using the Y component of the camera's view.
    /// This method generates a rotation matrix that defines a rotation around the X-axis.
    /// The angle of rotation is determined by the Y component of the camera's current view.
    /// <returns>
    /// A 4x4 matrix representing the rotation transformation along the X-axis.
    /// </returns>
    public Matrix4x4 RotXAxis()
    {
        float cosY = (float)Math.Cos(View.Y);
        float sinY = (float)Math.Sin(View.Y);

        return new Matrix4x4(
            1,     0,      0,      0,
            0,   cosY,  -sinY,     0,
            0,   sinY,   cosY,     0,
            0,     0,      0,      1
        );
    }

    /// <summary>
    /// Creates a rotation matrix for rotating vectors around the Y-axis
    /// based on the current view angle of the camera.
    /// </summary>
    /// <returns>
    /// A <see cref="Matrix4x4"/> representing the rotation matrix around
    /// the Y-axis.
    /// </returns>
    public Matrix4x4 RotYAxis()
    {
        float cosX = (float)Math.Cos(View.X);
        float sinX = (float)Math.Sin(View.X);

        return new Matrix4x4(
            cosX,    0,  sinX,    0,
              0,     1,    0,     0,
           -sinX,    0,  cosX,    0,
              0,     0,    0,     1
        );
    }

    /// <summary>
    /// Updates the camera's position and orientation based on user input to allow free movement.
    /// </summary>
    /// <param name="frameTime">The time elapsed since the last frame, used to ensure smooth and consistent movement.</param>
    public abstract void FreeCam(float frameTime);
}
}
