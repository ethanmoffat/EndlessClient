using AutomaticTypeMapper;
using EOLib.Localization;

namespace EndlessClient.Initialization;

[MappedType(BaseType = typeof(IGameInitializer))]
public class LocalizationInitializer : IGameInitializer
{
    private readonly IDataFileLoadActions _dataFileLoadActions;

    public LocalizationInitializer(IDataFileLoadActions dataFileLoadActions)
    {
        _dataFileLoadActions = dataFileLoadActions;
    }

    public void Initialize()
    {
        _dataFileLoadActions.LoadDataFiles();
    }
}