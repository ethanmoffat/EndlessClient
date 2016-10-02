// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.DependencyInjection;
using EOLib.IO.Actions;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.IO.Services.Serializers;
using Microsoft.Practices.Unity;

namespace EOLib.IO
{
    public class IODependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container
                .RegisterType<INumberEncoderService, NumberEncoderService>()
                .RegisterType<IMapStringEncoderService, MapStringEncoderService>()
                .RegisterType<IMapFileLoadService, MapFileLoadService>();

            container
                .RegisterType<IPubLoadService<EIFRecord>, ItemFileLoadService>()
                .RegisterType<IPubLoadService<ENFRecord>, NPCFileLoadService>()
                .RegisterType<IPubLoadService<ESFRecord>, SpellFileLoadService>()
                .RegisterType<IPubLoadService<ECFRecord>, ClassFileLoadService>()
                .RegisterType<IPubFileSaveService, PubFileSaveService>()
                .RegisterType<IMapFileSaveService, MapFileSaveService>();

            container
                .RegisterType<ISerializer<IMapFile>, MapFileSerializer>()
                .RegisterType<ISerializer<IMapFileProperties>, MapPropertiesSerializer>()
                .RegisterType<ISerializer<ChestSpawnMapEntity>, ChestSpawnMapEntitySerializer>()
                .RegisterType<ISerializer<NPCSpawnMapEntity>, NPCSpawnMapEntitySerializer>()
                .RegisterType<ISerializer<WarpMapEntity>, WarpMapEntitySerializer>()
                .RegisterType<ISerializer<SignMapEntity>, SignMapEntitySerializer>()
                .RegisterType<ISerializer<UnknownMapEntity>, UnknownMapEntitySerializer>();

            container
                .RegisterInstance<IPubFileRepository, PubFileRepository>()
                .RegisterInstance<IPubFileProvider, PubFileRepository>()
                .RegisterInstance<IEIFFileRepository, PubFileRepository>()
                .RegisterInstance<IEIFFileProvider, PubFileRepository>()
                .RegisterInstance<IENFFileRepository, PubFileRepository>()
                .RegisterInstance<IENFFileProvider, PubFileRepository>()
                .RegisterInstance<IESFFileRepository, PubFileRepository>()
                .RegisterInstance<IESFFileProvider, PubFileRepository>()
                .RegisterInstance<IECFFileRepository, PubFileRepository>()
                .RegisterInstance<IECFFileProvider, PubFileRepository>()
                .RegisterInstance<IMapFileRepository, MapFileRepository>()
                .RegisterInstance<IMapFileProvider, MapFileRepository>();

            container
                .RegisterType<IPubFileLoadActions, PubFileLoadActions>()
                .RegisterType<IMapFileLoadActions, MapFileLoadActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            var pubFileLoadActions = container.Resolve<IPubFileLoadActions>();

            try
            {
                pubFileLoadActions.LoadItemFile();
            }
            catch (IOException)
            {
                //todo: log message?
            }

            try
            {
                pubFileLoadActions.LoadNPCFile();
            }
            catch (IOException) { }

            try
            {
                pubFileLoadActions.LoadSpellFile();
            }
            catch (IOException) { }

            try
            {
                pubFileLoadActions.LoadClassFile();
            }
            catch (IOException) { }
        }
    }
}
