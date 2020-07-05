using LiteLoader.DependencyInjection;
using LiteLoader.Logging;
using System;
using System.IO;

namespace LiteLoader
{
    internal sealed class LiteLoader : ILiteLoader
    {
        #region Information

        public string RootDirectory { get; }

        public string FrameworkDirectory { get; }

        public string ModuleDirectory { get; }

        public string TemporaryDirectory { get; }

        public LogLevel LogLevel { get; }

        public string GameModule { get; }

        #endregion

        #region Services

        public IDynamicServiceProvider ServiceProvider { get; }

        public ILogger RootLogger { get; private set; }

        #endregion

        #region I / O

        public LiteLoader(string gameAssembly)
        {
            LogLevel = LogLevel.Development;
            TemporaryDirectory = Path.Combine(Path.GetTempPath(), "LiteLoader");
            TemporaryDirectory = Path.Combine(TemporaryDirectory, Guid.NewGuid().ToString("B"));
#if !NET35
            CancellationSource = new System.Threading.CancellationTokenSource();
#endif
            ServiceProvider = new DynamicServiceProvider();

            // Setup Directories
            RootDirectory = Environment.CurrentDirectory;

            if (RootDirectory.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
            {
                RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (RootDirectory == null)
            {
                throw new InvalidProgramException("Unable to identify root directory");
            }

            FrameworkDirectory = Path.Combine(RootDirectory, "LiteLoader");
            ModuleDirectory = Path.Combine(FrameworkDirectory, "Modules");
            string gameModule = Path.Combine(ModuleDirectory, gameAssembly + ".dll");

            //if (!File.Exists(gameModule))
            //{
            //    throw new IOException($"Game module not found | {gameModule}");
            //}

            FileInfo g = new FileInfo(gameModule);
            GameModule = g.FullName;
        }

        internal void Load()
        {
            RootLogger = new CompoundLogger();
            if (!Directory.Exists(TemporaryDirectory))
            {
                Directory.CreateDirectory(TemporaryDirectory);
            }

            if (!Directory.Exists(FrameworkDirectory))
            {
                Directory.CreateDirectory(FrameworkDirectory);
            }

            if (!Directory.Exists(ModuleDirectory))
            {
                Directory.CreateDirectory(ModuleDirectory);
            }

            ((CompoundLogger)RootLogger).AddLogger(new FileLogger(Path.Combine(FrameworkDirectory, "Core.log"), 1000000, TimeSpan.FromDays(30)));
            ServiceProvider.AddSingleton<IExecutionEngine, ExecutionEngine>();
        }

        internal void Unload()
        {
#if !NET35
            CancellationSource.Cancel(false);
#endif
        }

#if !NET35

        private System.Threading.CancellationTokenSource CancellationSource { get; }

        public System.Threading.CancellationToken GenerateCancellationToken()
        {
            return CancellationSource.Token;
        }

#endif

        #endregion
    }
}
