// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using Microsoft.Practices.Unity;

namespace EOLib.Localization
{
    public class LocalizationDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container
                .RegisterType<ILocalizedStringFinder, LocalizedStringFinder>()
                .RegisterInstance<IDataFileRepository, DataFileRepository>()
                .RegisterInstance<IDataFileProvider, DataFileRepository>()
                .RegisterType<IDataFileLoadActions, DataFileLoadActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            var fileLoadActions = container.Resolve<IDataFileLoadActions>();

            fileLoadActions.LoadDataFiles();
        }
    }
}
