// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

/*
 * SPECIAL NOTE:
 *   This file is a hack solution. Basically, I need to get around a circular dependency:
 *      MainButtonController->GameStateActions->ControlSetFactory->MainButtonController
 *   This seems to be the best way around it, by making a provider interface and injecting
 *      the provider into ControlSetFactory instead of the controller itself.
 */

namespace EndlessClient.Controllers.Repositories
{
    public interface IMainButtonControllerRepository
    {
        IMainButtonController MainButtonController { get; set; }
    }

    public class MainButtonControllerRepository : IMainButtonControllerRepository
    {
        public IMainButtonController MainButtonController { get; set; }
    }
}
