// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using System;
using EndlessClient.GameExecution;
using System.Reflection;

namespace EndlessClient
{
    public static class Program
    {
        private static IGameRunner _gameRunner;

        [STAThread]
        public static void Main()
        {
            var assemblyNames = new []
            {
                Assembly.GetExecutingAssembly().FullName,
                "EOLib.Config",
                "EOLib.Graphics",
                "EOLib.IO",
                "EOLib.Localization"
            };

            using (ITypeRegistry registry = new UnityRegistry(assemblyNames))
            {
#if DEBUG
                _gameRunner = new DebugGameRunner(registry);
#else
                _gameRunner = new ReleaseGameRunner(registry);
#endif
                if (_gameRunner.SetupDependencies())
                    _gameRunner.RunGame();
            }
        }
    }
}