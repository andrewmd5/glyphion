namespace Glyphion.Core
{
    /// <summary>
    /// Represents the base class for a game in the Glyphion framework.
    /// This class provides the core game loop and lifecycle management.
    /// </summary>
    public abstract class GlyphionGame
    {
        /// <summary> Provides access to the GlyphionEngine instance utilized by the game. </summary>
        public GlyphionEngine Engine { get; }

        /// <summary>
        /// Gets an instance of DeltaTime used for managing frame rate and measuring time between frames in the game.
        /// </summary>
        public DeltaTime DeltaTime { get; }

        /// <summary> Indicates whether the game is currently running. </summary>
        public bool IsRunning { get; private set; }


        /// <summary>
        /// Thread responsible for running the game loop.
        /// </summary>
        protected Thread _gameThread;


        /// <summary>
        /// Represents the main game class for TerminalCraft, inheriting from GlyphionGame.
        /// This class initializes and manages the core game components such as player, chunk manager, physics, and rendering functionality.
        /// </summary>
        public GlyphionGame(GlyphionEngineOptions options)
        {
            Engine = new GlyphionEngine(options);
            DeltaTime = new DeltaTime();
            if (options.TargetFps.HasValue)
            {
                DeltaTime.SetTargetFps(options.TargetFps.Value);
            }
            Console.Title = options.GameTitle;
            Create();
        }

        /// <summary>
        /// Called once upon game creation to initialize internal logic and facilitate custom resource imports.
        /// </summary>
        protected void Create()
        {
            IsRunning = true;
            // Internal creation logic
            InternalCreateLogic();

            // Call the implementer’s creation logic
            OnCreate();
            _gameThread.Start();
        }

        /// <summary>
        /// Called every frame before rendering.
        /// Contains internal update logic.
        /// </summary>
        protected void Update()
        {
            // Internal update logic
            InternalUpdateLogic();

            // Call the implementer’s update logic
            OnUpdate();
        }

        /// <summary>
        /// Called every frame after updating. Contains internal rendering logic.
        /// </summary>
        protected void Render()
        {
            // Call the implementer’s render logic
            OnRender();
            
            // Internal render logic
            InternalRenderLogic();
        }

        /// <summary>
        /// Internal creation logic that must always run.
        /// </summary>
        protected void InternalCreateLogic()
        {
            _gameThread = new Thread(new ThreadStart(GameLoop));
        }

        /// <summary>
        /// The primary execution loop for the game.
        /// Manages the continuous updating and rendering processes while the game is running.
        /// </summary>
        private void GameLoop()
        {
            while (IsRunning)
            {
                Update();
                Render();
            }
        }

        /// <summary>
        /// Internal update logic that must always run.
        /// Manages the update of the delta time and maintains consistent frame rate.
        /// </summary>
        protected void InternalUpdateLogic()
        {
            DeltaTime.Update();
        }

        /// <summary>
        /// Internal render logic that must always run.
        /// </summary>
        protected void InternalRenderLogic()
        {
            Engine.Render();
        }

        /// <summary>
        /// Called during the creation phase of the game. This method should be overridden by subclasses to initialize
        /// game-specific resources, configurations, and start necessary background processes. It is invoked once after
        /// the internal creation logic of the GlyphionGame.
        /// </summary>
        protected abstract void OnCreate();

        /// <summary>
        /// Implementers should provide custom update logic here.
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// Implementers should provide custom render logic here.
        /// </summary>
        protected abstract void OnRender();

        /// <summary>
        /// Stops the game loop.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Waits for the game to finish running without blocking other threads.
        /// </summary>
        public void WaitForExit()
        {
            _gameThread.Join();
        }
    }
}
