using AutomaticTypeMapper;
using System;

namespace EndlessClient.HUD;

public interface IStatusLabelTextRepository
{
    string StatusText { get; set; }

    DateTime SetTime { get; set; }
}

public interface IStatusLabelTextProvider
{
    string StatusText { get; }

    DateTime SetTime { get; }
}

[MappedType(BaseType = typeof(IStatusLabelTextRepository), IsSingleton = true)]
[MappedType(BaseType = typeof(IStatusLabelTextProvider), IsSingleton = true)]
public class StatusLabelTextRepository : IStatusLabelTextRepository, IStatusLabelTextProvider
{
    public string StatusText { get; set; }

    public DateTime SetTime { get; set; }

    public StatusLabelTextRepository()
    {
        StatusText = "";
        SetTime = DateTime.Now;
    }
}