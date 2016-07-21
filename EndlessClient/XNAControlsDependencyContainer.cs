// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib;
using EOLib.DependencyInjection;
using Microsoft.Practices.Unity;
using Microsoft.Xna.Framework;

namespace EndlessClient
{
    public class XNAControlsDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //todo: investigate adding support for Unity container to XNAControls 
            //    or some other way to replace this grossness
            var game = container.Resolve<IEndlessGame>();
            XNAControls.XNAControls.Initialize((Game)game);

            XNAControls.XNAControls.IgnoreEnterForDialogs = true;
            XNAControls.XNAControls.IgnoreEscForDialogs = true;
        }
    }
}
