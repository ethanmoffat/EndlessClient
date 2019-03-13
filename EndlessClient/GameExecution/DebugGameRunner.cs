// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;

namespace EndlessClient.GameExecution
{
    /// <summary>
    /// A game runner that does not catch any exceptions
    /// </summary>
    public class DebugGameRunner : GameRunnerBase
    {
        public DebugGameRunner(ITypeRegistry registry)
            : base(registry) { }
    }
}
