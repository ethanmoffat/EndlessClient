// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;

namespace EOLib.Domain.Map
{
    public interface ICurrentMapStateRepository
    {
        short CurrentMapID { get; set; }

        bool ShowMiniMap { get; set; }

        List<ICharacter> Characters { get; set; }

        List<INPC> NPCs { get; set; }

        List<IItem> MapItems { get; set; }

        List<IWarp> OpenDoors { get; set;  }

        List<IWarp> PendingDoors { get; set; }

        WarpState MapWarpState { get; set; }
    }

    public interface ICurrentMapStateProvider
    {
        short CurrentMapID { get; }

        bool ShowMiniMap { get; }

        IReadOnlyList<ICharacter> Characters { get; }

        IReadOnlyList<INPC> NPCs { get; }

        IReadOnlyList<IItem> MapItems { get; }

        IReadOnlyList<IWarp> OpenDoors { get; }

        IReadOnlyList<IWarp> PendingDoors { get; }

        WarpState MapWarpState { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapStateProvider, IResettable
    {
        public short CurrentMapID { get; set; }

        public bool ShowMiniMap { get; set; }

        public List<ICharacter> Characters { get; set; }

        public List<INPC> NPCs { get; set; }

        public List<IItem> MapItems { get; set; }

        public List<IWarp> OpenDoors { get; set; }

        public List<IWarp> PendingDoors { get; set; }

        public WarpState MapWarpState { get; set; }

        IReadOnlyList<ICharacter> ICurrentMapStateProvider.Characters => Characters;

        IReadOnlyList<INPC> ICurrentMapStateProvider.NPCs => NPCs;

        IReadOnlyList<IItem> ICurrentMapStateProvider.MapItems => MapItems;

        IReadOnlyList<IWarp> ICurrentMapStateProvider.OpenDoors => OpenDoors;

        IReadOnlyList<IWarp> ICurrentMapStateProvider.PendingDoors => PendingDoors;

        public CurrentMapStateRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            CurrentMapID = 0;
            ShowMiniMap = false;

            Characters = new List<ICharacter>();
            NPCs = new List<INPC>();
            MapItems = new List<IItem>();
            OpenDoors = new List<IWarp>();
            PendingDoors = new List<IWarp>();

            MapWarpState = WarpState.None;
        }
    }
}
