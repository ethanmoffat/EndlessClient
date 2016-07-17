// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;

namespace EndlessClient.ControlSets
{
    public interface IControlSetFactory
    {
        IControlSet CreateControlsForState(GameStates newState, IControlSet currentControlSet);
    }
}
