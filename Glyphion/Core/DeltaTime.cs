using System.Diagnostics;

namespace Glyphion.Core;

/// <summary>
/// A class to measure delta time (time between frames) and manage frame rate for game development.
/// </summary>
public class DeltaTime
{
    /// <summary>
    /// Stopwatch used to measure the elapsed time for calculating delta time and enforcing frame rate.
    /// </summary>
    private readonly Stopwatch _timer;

    /// <summary>
    /// The time at which the last frame was completed.
    /// This is used to calculate the delta time between frames.
    /// </summary>
    private TimeSpan _lastFrameTime;

    /// <summary>
    /// Stores the time, in seconds, that it took to complete the last frame.
    /// Updated every frame during the <c>Update</c> method.
    /// </summary>
    private float _deltaTime = 0f; // Time for last frame in seconds

    /// <summary>
    /// Accumulates the sum of frames per second (FPS) over 100 frames
    /// to calculate the average FPS.
    /// </summary>
    private float _fpsSum = 0f;

    /// <summary>
    /// Counter for the number of frames processed.
    /// </summary>
    private int _frameCount = 0;

    /// <summary>
    /// Holds a counter to keep track of the number of frames processed.
    /// </summary>
    private int _fpsCounter = 0;

    /// <summary>
    /// The target frame time in seconds. This value determines the desired time
    /// each frame should take to maintain a consistent frame rate. If set to a
    /// non-zero positive value and frame rate limiting is enabled, the system
    /// will adjust execution to meet this target frame time. A value of 0 means
    /// no frame rate limiting.
    /// </summary>
    private float _targetFrameTime = 0f; // Target frame time in seconds

    /// <summary>
    /// Determines if the frame rate is limited to a specified target. When set to true,
    /// the application enforces the target frame rate by introducing delays as needed to
    /// maintain consistent frame timing.
    /// </summary>
    private bool _isFrameLimited = false;

    /// <summary>
    /// Manages the timing of frames and calculates the delta time between frames.
    /// </summary>
    public DeltaTime()
    {
        _timer = Stopwatch.StartNew();
        
        _lastFrameTime = _timer.Elapsed;
    }

    /// <summary>
    /// Updates the delta time and enforces the target frame rate if set.
    /// </summary>
    public void Update()
    {
        // Calculate the delta time (time between the last and current frame)
        var currentFrameTime = _timer.Elapsed;
        _deltaTime = (float)(currentFrameTime - _lastFrameTime).TotalSeconds;
        _lastFrameTime = currentFrameTime;

        if (_isFrameLimited && _targetFrameTime > 0f)
        {
            EnforceFrameRate();
        }
    }

    /// <summary>
    /// Enforces the target frame rate by sleeping or busy-waiting as needed.
    /// </summary>
    private void EnforceFrameRate()
    {
        var elapsedTime = _timer.Elapsed.TotalSeconds - _lastFrameTime.TotalSeconds;
        
        if (elapsedTime < _targetFrameTime)
        {
            var remainingTime = _targetFrameTime - elapsedTime;

            if (remainingTime > 0.001)
            {
                // Sleep for the remaining time minus a small buffer to avoid overshooting
                Thread.Sleep((int)((remainingTime - 0.001) * 1000));
            }

            // Busy-wait for the very small remaining time to avoid oversleeping
            while ((_timer.Elapsed.TotalSeconds - _lastFrameTime.TotalSeconds) < _targetFrameTime)
            {
                Thread.SpinWait(1); // Minimal spin wait for fine-grain timing
            }
        }
    }

    /// <summary>
    /// Displays the average FPS every 100 frames.
    /// </summary>
    /// <returns>
    /// A string representing the average FPS.
    /// </returns>
    public string GetFps()
    {
        _frameCount++;
        _fpsSum += 1f / Math.Max(_deltaTime, 1e-6f); // Avoid division by zero

        if (_frameCount >= 100)
        {
            var avgFps = _fpsSum / 100f;
            // Reset counters
            _fpsSum = 0f;
            _frameCount = 0;
           return $"Average FPS: {avgFps:F2}";

       
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the time it took to complete the last frame (in seconds).
    /// </summary>
    /// <returns>The delta time for the last frame in seconds.</returns>
    public float GetDeltaTime()
    {
        return _deltaTime;
    }

    /// <summary>
    /// Gets the total elapsed time since the DeltaTime instance was created (in seconds).
    /// </summary>
    /// <returns>The total elapsed time in seconds.</returns>
    public float GetElapsedTime()
    {
        return (float)_timer.Elapsed.TotalSeconds;
    }

    /// <summary>
    /// Sets the target frame rate (FPS). A value of 0 disables frame rate limiting.
    /// </summary>
    /// <param name="fps">The desired frames per second. Set to 0 to disable frame rate limiting.</param>
    internal void SetTargetFps(float fps)
    {
        if (fps <= 0f)
        {
            _isFrameLimited = false;
            _targetFrameTime = 0f;
        }
        else
        {
            _isFrameLimited = true;
            _targetFrameTime = 1f / fps; // Convert FPS to seconds per frame
        }
    }
}
