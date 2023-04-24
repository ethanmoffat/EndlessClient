using AutomaticTypeMapper;
using Optional;
using System;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public interface ICurrentMapStateRepository
    {
        int CurrentMapID { get; set; }

        bool ShowMiniMap { get; set; }

        int JailMapID { get; set; }

        bool IsJail { get; }

        Dictionary<int, Character.Character> Characters { get; set; }

        HashSet<NPC.NPC> NPCs { get; set; }

        HashSet<MapItem> MapItems { get; set; }

        HashSet<Warp> OpenDoors { get; set;  }

        HashSet<Warp> PendingDoors { get; set; }

        HashSet<MapCoordinate> VisibleSpikeTraps { get; set; }

        WarpState MapWarpState { get; set; }

        Option<int> MapWarpSession { get; set; }

        Option<int> MapWarpID { get; set; }

        Option<DateTime> MapWarpTime { get; set; }

        HashSet<int> UnknownPlayerIDs { get; set; }

        HashSet<int> UnknownNPCIndexes { get; set; }
    }

    public interface ICurrentMapStateProvider
    {
        int CurrentMapID { get; }

        bool ShowMiniMap { get; }

        int JailMapID { get; }

        bool IsJail { get; }

        IReadOnlyDictionary<int, Character.Character> Characters { get; }

        IReadOnlyCollection<NPC.NPC> NPCs { get; }

        IReadOnlyCollection<MapItem> MapItems { get; }

        IReadOnlyCollection<Warp> OpenDoors { get; }

        IReadOnlyCollection<Warp> PendingDoors { get; }

        IReadOnlyCollection<MapCoordinate> VisibleSpikeTraps { get; }

        WarpState MapWarpState { get; }

        Option<int> MapWarpSession { get; }

        Option<int> MapWarpID { get; }

        Option<DateTime> MapWarpTime { get; }

        HashSet<int> UnknownPlayerIDs { get; }

        HashSet<int> UnknownNPCIndexes { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapStateProvider, IResettable
    {
        public int CurrentMapID { get; set; }

        public bool ShowMiniMap { get; set; }

        public int JailMapID { get; set; }

        public bool IsJail => JailMapID == CurrentMapID;

        public Dictionary<int, Character.Character> Characters { get; set; }

        public HashSet<NPC.NPC> NPCs { get; set; }

        public HashSet<MapItem> MapItems { get; set; }

        public HashSet<Warp> OpenDoors { get; set; }

        public HashSet<Warp> PendingDoors { get; set; }

        public HashSet<MapCoordinate> VisibleSpikeTraps { get; set;  }

        public WarpState MapWarpState { get; set; }

        public Option<int> MapWarpSession { get; set; }

        public Option<int> MapWarpID { get; set; }

        public Option<DateTime> MapWarpTime { get; set; }

        public HashSet<int> UnknownPlayerIDs { get; set; }

        public HashSet<int> UnknownNPCIndexes { get; set; }

        IReadOnlyDictionary<int, Character.Character> ICurrentMapStateProvider.Characters => Characters;

        IReadOnlyCollection<NPC.NPC> ICurrentMapStateProvider.NPCs => NPCs;

        IReadOnlyCollection<MapItem> ICurrentMapStateProvider.MapItems => MapItems;

        IReadOnlyCollection<Warp> ICurrentMapStateProvider.OpenDoors => OpenDoors;

        IReadOnlyCollection<Warp> ICurrentMapStateProvider.PendingDoors => PendingDoors;

        IReadOnlyCollection<MapCoordinate> ICurrentMapStateProvider.VisibleSpikeTraps => VisibleSpikeTraps;

        public CurrentMapStateRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            CurrentMapID = 0;
            ShowMiniMap = false;
            JailMapID = 0;

            Characters = new Dictionary<int, Character.Character>();
            NPCs = new HashSet<NPC.NPC>();
            MapItems = new HashSet<MapItem>();
            OpenDoors = new HashSet<Warp>();
            PendingDoors = new HashSet<Warp>();
            VisibleSpikeTraps = new HashSet<MapCoordinate>();
            UnknownPlayerIDs = new HashSet<int>();
            UnknownNPCIndexes = new HashSet<int>();

            MapWarpState = WarpState.None;
            MapWarpSession = Option.None<int>();
            MapWarpID = Option.None<int>();
        }
    }
}
