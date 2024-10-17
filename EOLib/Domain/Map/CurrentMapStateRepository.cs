using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using Optional;

namespace EOLib.Domain.Map
{
    public interface ICurrentMapStateRepository
    {
        int CurrentMapID { get; set; }

        bool ShowMiniMap { get; set; }

        int JailMapID { get; set; }

        bool IsJail { get; }

        MapEntityCollectionHashSet<Character.Character> Characters { get; set; }

        MapEntityCollectionHashSet<NPC.NPC> NPCs { get; set; }

        MapEntityCollectionHashSet<MapItem> MapItems { get; set; }

        HashSet<Warp> OpenDoors { get; set; }

        HashSet<Warp> PendingDoors { get; set; }

        Option<DateTime> LastTimedSpikeEvent { get; set; }

        WarpState MapWarpState { get; set; }

        Option<int> MapWarpSession { get; set; }

        Option<int> MapWarpID { get; set; }

        Option<DateTime> MapWarpTime { get; set; }

        bool IsSleepWarp { get; set; }

        HashSet<int> UnknownPlayerIDs { get; set; }

        HashSet<int> UnknownNPCIndexes { get; set; }
    }

    public interface ICurrentMapStateProvider
    {
        int CurrentMapID { get; }

        bool ShowMiniMap { get; }

        int JailMapID { get; }

        bool IsJail { get; }

        IReadOnlyMapEntityCollection<Character.Character> Characters { get; }

        IReadOnlyMapEntityCollection<NPC.NPC> NPCs { get; }

        IReadOnlyMapEntityCollection<MapItem> MapItems { get; }

        IReadOnlyCollection<Warp> OpenDoors { get; }

        IReadOnlyCollection<Warp> PendingDoors { get; }

        Option<DateTime> LastTimedSpikeEvent { get; set; }

        WarpState MapWarpState { get; }

        Option<int> MapWarpSession { get; }

        Option<int> MapWarpID { get; }

        Option<DateTime> MapWarpTime { get; }

        bool IsSleepWarp { get; }

        IReadOnlyCollection<int> UnknownPlayerIDs { get; }

        IReadOnlyCollection<int> UnknownNPCIndexes { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapStateProvider, IResettable
    {
        public int CurrentMapID { get; set; }

        public bool ShowMiniMap { get; set; }

        public int JailMapID { get; set; }

        public bool IsJail => JailMapID == CurrentMapID;

        public MapEntityCollectionHashSet<Character.Character> Characters { get; set; }

        public MapEntityCollectionHashSet<NPC.NPC> NPCs { get; set; }

        public MapEntityCollectionHashSet<MapItem> MapItems { get; set; }

        public HashSet<Warp> OpenDoors { get; set; }

        public HashSet<Warp> PendingDoors { get; set; }

        public Option<DateTime> LastTimedSpikeEvent { get; set; }

        public WarpState MapWarpState { get; set; }

        public Option<int> MapWarpSession { get; set; }

        public Option<int> MapWarpID { get; set; }

        public Option<DateTime> MapWarpTime { get; set; }

        public bool IsSleepWarp { get; set; }

        public HashSet<int> UnknownPlayerIDs { get; set; }

        public HashSet<int> UnknownNPCIndexes { get; set; }

        IReadOnlyMapEntityCollection<Character.Character> ICurrentMapStateProvider.Characters => Characters;

        IReadOnlyMapEntityCollection<NPC.NPC> ICurrentMapStateProvider.NPCs => NPCs;

        IReadOnlyMapEntityCollection<MapItem> ICurrentMapStateProvider.MapItems => MapItems;

        IReadOnlyCollection<Warp> ICurrentMapStateProvider.OpenDoors => OpenDoors;

        IReadOnlyCollection<Warp> ICurrentMapStateProvider.PendingDoors => PendingDoors;

        IReadOnlyCollection<int> ICurrentMapStateProvider.UnknownPlayerIDs => UnknownPlayerIDs;

        IReadOnlyCollection<int> ICurrentMapStateProvider.UnknownNPCIndexes => UnknownNPCIndexes;

        public CurrentMapStateRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            CurrentMapID = 0;
            ShowMiniMap = false;
            JailMapID = 0;

            Characters = new MapEntityCollectionHashSet<Character.Character>(x => x.ID, x => new MapCoordinate(x.X, x.Y));
            NPCs = new MapEntityCollectionHashSet<NPC.NPC>(x => x.Index, x => new MapCoordinate(x.X, x.Y));
            MapItems = new MapEntityCollectionHashSet<MapItem>(x => x.UniqueID, x => new MapCoordinate(x.X, x.Y));
            OpenDoors = new HashSet<Warp>();
            PendingDoors = new HashSet<Warp>();
            LastTimedSpikeEvent = Option.None<DateTime>();
            UnknownPlayerIDs = new HashSet<int>();
            UnknownNPCIndexes = new HashSet<int>();

            MapWarpState = WarpState.None;
            MapWarpSession = Option.None<int>();
            MapWarpID = Option.None<int>();
            IsSleepWarp = false;
        }
    }
}
