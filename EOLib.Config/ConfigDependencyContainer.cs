using EOLib.DependencyInjection;
using Unity;

namespace EOLib.Config
{
    public class ConfigDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container) {}

        public void InitializeDependencies(IUnityContainer container)
        {
            var configLoadActions = container.Resolve<IConfigFileLoadActions>();

            configLoadActions.LoadConfigFile();
        }
    }
}
