using AutomaticTypeMapper;
using EOLib.Config;

namespace EndlessClient.GameExecution
{
    /// <summary>
    /// A game runner that does not catch any exceptions
    /// </summary>
    public class DebugGameRunner : GameRunnerBase
    {
        public DebugGameRunner(ITypeRegistry registry, string[] args)
            : base(registry, args) { }

        public override bool SetupDependencies()
        {
            var result = base.SetupDependencies();
            _registry.Resolve<IConfigurationRepository>().DebugCrashes = true;
            return result;
        }
    }
}
