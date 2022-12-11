using AutomaticTypeMapper;
using Optional;
using System;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public interface ICurrentMapStateRepository
    {
        short CurrentMapID { get; set; }

        bool ShowMiniMap { get; set; }

        short JailMapID { get; set; }

        Dictionary<int, Character.Character> Characters { get; set; }

        HashSet<NPC.NPC> NPCs { get; set; }

        HashSet<MapItem> MapItems { get; set; }

        HashSet<Warp> OpenDoors { get; set;  }

        HashSet<Warp> PendingDoors { get; set; }

        HashSet<MapCoordinate> VisibleSpikeTraps { get; set; }

        WarpState MapWarpState { get; set; }

        Option<short> MapWarpSession { get; set; }

        Option<short> MapWarpID { get; set; }

        Option<DateTime> MapWarpTime { get; set; }

        HashSet<short> UnknownPlayerIDs { get; set; }

        HashSet<byte> UnknownNPCIndexes { get; set; }
    }

    public interface ICurrentMapStateProvider
    {
        short CurrentMapID { get; }

        bool ShowMiniMap { get; }

        short JailMapID { get; }

        bool IsJail { get; }

        IReadOnlyDictionary<int, Character.Character> Characters { get; }

        IReadOnlyCollection<NPC.NPC> NPCs { get; }

        IReadOnlyCollection<MapItem> MapItems { get; }

        IReadOnlyCollection<Warp> OpenDoors { get; }

        IReadOnlyCollection<Warp> PendingDoors { get; }

        IReadOnlyCollection<MapCoordinate> VisibleSpikeTraps { get; }

        WarpState MapWarpState { get; }

        Option<short> MapWarpSession { get; }

        Option<short> MapWarpID { get; }

        Option<DateTime> MapWarpTime { get; }

        HashSet<short> UnknownPlayerIDs { get; }

        HashSet<byte> UnknownNPCIndexes { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapStateProvider, IResettable
    {
        public short CurrentMapID { get; set; }

        public bool ShowMiniMap { get; set; }

        public short JailMapID { get; set; }

        public bool IsJail => JailMapID == CurrentMapID;

        public Dictionary<int, Character.Character> Characters { get; set; }

        public HashSet<NPC.NPC> NPCs { get; set; }

        public HashSet<MapItem> MapItems { get; set; }

        public HashSet<Warp> OpenDoors { get; set; }

        public HashSet<Warp> PendingDoors { get; set; }

        public HashSet<MapCoordinate> VisibleSpikeTraps { get; set;  }

        public WarpState MapWarpState { get; set; }

        public Option<short> MapWarpSession { get; set; }

        public Option<short> MapWarpID { get; set; }

        public Option<DateTime> MapWarpTime { get; set; }

        public HashSet<short> UnknownPlayerIDs { get; set; }

        public HashSet<byte> UnknownNPCIndexes { get; set; }

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
            UnknownPlayerIDs = new HashSet<short>();
            UnknownNPCIndexes = new HashSet<byte>();

            MapWarpState = WarpState.None;
            MapWarpSession = Option.None<short>();
            MapWarpID = Option.None<short>();
        }
    }
}
