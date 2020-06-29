using LiteLoader.DependencyInjection;
using System;
using System.IO;
using System.Threading;

namespace LiteLoader
{
    internal sealed class LiteLoader : ILiteLoader
    {
        #region Information

        public string RootDirectory { get; }

        public string FrameworkDirectory { get; }

        public string ModuleDirectory { get; }

        public string GameModule { get; }

        #endregion

        #region Services

        public IDynamicServiceProvider ServiceProvider { get; }

        #endregion

        #region I / O

        public LiteLoader(string gameAssembly)
        {
#if !NET35
            CancellationSource = new CancellationTokenSource();
#endif
            ServiceProvider = new DynamicServiceProvider();
            ServiceProvider.AddSingleton<IExecutionEngine, ExecutionEngine>();

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

            if (!File.Exists(gameModule))
            {
                throw new IOException($"Game module not found | {gameModule}");
            }

            FileInfo g = new FileInfo(gameModule);
            GameModule = g.FullName;
        }

        internal void Load()
        {
        }

        internal void Unload()
        {
#if !NET35
            CancellationSource.Cancel(false);
#endif
        }

#if !NET35

        private CancellationTokenSource CancellationSource { get; }

        public CancellationToken GenerateCancellationToken()
        {
            return CancellationSource.Token;
        }

#endif

        #endregion
    }
}
