using AutomaticTypeMapper;

namespace EndlessClient.GameExecution;

/// <summary>
/// A game runner that does not catch any exceptions
/// </summary>
public class DebugGameRunner : GameRunnerBase
{
    public DebugGameRunner(ITypeRegistry registry, string[] args)
        : base(registry, args) { }
}