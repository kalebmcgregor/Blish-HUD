using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Graphics {
    public readonly ref struct GraphicsDeviceContext {

        private readonly GraphicsService _service;

        private readonly bool _highPriority;

        private readonly int _openingThread;

        /// <summary>
        /// Constructs a new graphics device context, that automatically
        /// calls <see cref="GraphicsService.LendGraphicsDevice(bool)"/> on creation
        /// and <see cref="GraphicsService.ReturnGraphicsDevice"/> when disposed.
        /// </summary>
        /// <param name="service">The graphics service instance to use.</param>
        /// <param name="highPriority">A value indicating whether to acquire a high priority instance.</param>
        internal GraphicsDeviceContext(GraphicsService service, bool highPriority) {
            _openingThread = System.Threading.Thread.CurrentThread.ManagedThreadId;

            _service       = service;
            _highPriority  = highPriority || Program.IsMainThread;

            Logger.GetLogger(typeof(GraphicsDeviceContext)).Info($"+++ Lend Lock " + (_highPriority ? "HIGH" : "LOW") + $" on thread {_openingThread}");
            GraphicsDevice = _service.LendGraphicsDevice(_highPriority);
        }

        /// <summary>
        /// Get the GraphicsDevice associated with this context.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Disposes of this graphics context, calling <see cref="GraphicsService.ReturnGraphicsDevice"/>
        /// </summary>
        public void Dispose() {
            Logger.GetLogger(typeof(GraphicsDeviceContext)).Info($"--- Lend Exit " + (_highPriority ? "HIGH" : "LOW") + $" on thread {_openingThread}");

            if (_openingThread != System.Threading.Thread.CurrentThread.ManagedThreadId) {
                Logger.GetLogger(typeof(GraphicsDeviceContext)).Info("EXIT IS HAPPENING ON THE WRONG THREAD!");
            }

            _service.ReturnGraphicsDevice(_highPriority);

        }
    }
}
