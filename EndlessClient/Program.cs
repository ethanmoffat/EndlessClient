using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using System;
using System.Reflection;

namespace EndlessClient
{
    public static class Program
    {
        private static IGameRunner _gameRunner;

        [STAThread]
        public static void Main(string[] args)
        {
            var assemblyNames = new[]
            {
                Assembly.GetExecutingAssembly().FullName,
                "EOLib",
                "EOLib.Config",
                "EOLib.Graphics",
                "EOLib.IO",
                "EOLib.Localization",
                "EOLib.Logger"
            };

            using (ITypeRegistry registry = new UnityRegistry(assemblyNames))
            {
#if DEBUG
                _gameRunner = new DebugGameRunner(registry, args);
#else
                _gameRunner = new ReleaseGameRunner(registry, args);
#endif
                if (_gameRunner.SetupDependencies())
                    _gameRunner.RunGame();
            }
        }
    }
}