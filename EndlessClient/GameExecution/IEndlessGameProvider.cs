using AutomaticTypeMapper;

namespace EndlessClient.GameExecution;

public interface IEndlessGameRepository
{
    IEndlessGame Game { get; set; }
}

public interface IEndlessGameProvider
{
    IEndlessGame Game { get; }
}

[MappedType(BaseType = typeof(IEndlessGameRepository), IsSingleton = true)]
[MappedType(BaseType = typeof(IEndlessGameProvider), IsSingleton = true)]
public class EndlessGameRepository : IEndlessGameRepository, IEndlessGameProvider
{
    public IEndlessGame Game { get; set; }
}