// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
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
            var game = (Game)container.Resolve<IEndlessGame>();
            XNAControls.GameRepository.SetGame(game);

            //todo: remove this once converted to new XNAControls code
            XNAControls.Old.XNAControls.Initialize(game);
            XNAControls.Old.XNAControls.IgnoreEnterForDialogs = true;
            XNAControls.Old.XNAControls.IgnoreEscForDialogs = true;
        }
    }
}
