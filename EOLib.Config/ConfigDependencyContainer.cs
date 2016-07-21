using EOLib.DependencyInjection;
using Microsoft.Practices.Unity;

namespace EOLib.Config
{
    public class ConfigDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterInstance<IConfigurationRepository, ConfigurationRepository>()
                .RegisterInstance<IConfigurationProvider, ConfigurationRepository>();

            container
                .RegisterType<IConfigFileLoadActions, ConfigFileLoadActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            var configLoadActions = container.Resolve<IConfigFileLoadActions>();

            configLoadActions.LoadConfigFile();
        }
    }
}
