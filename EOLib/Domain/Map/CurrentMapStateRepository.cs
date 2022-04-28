using AutomaticTypeMapper;
using EOLib.Domain.Character;
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

        HashSet<IItem> MapItems { get; set; }

        HashSet<IWarp> OpenDoors { get; set;  }

        HashSet<IWarp> PendingDoors { get; set; }

        HashSet<MapCoordinate> VisibleSpikeTraps { get; set; }

        WarpState MapWarpState { get; set; }

        HashSet<short> UnknownPlayerIDs { get; set; }

        HashSet<byte> UnknownNPCIndexes { get; set; }
    }

    public interface ICurrentMapStateProvider
    {
        short CurrentMapID { get; }

        bool ShowMiniMap { get; }

        short JailMapID { get; }

        IReadOnlyDictionary<int, Character.Character> Characters { get; }

        IReadOnlyCollection<NPC.NPC> NPCs { get; }

        IReadOnlyCollection<IItem> MapItems { get; }

        IReadOnlyCollection<IWarp> OpenDoors { get; }

        IReadOnlyCollection<IWarp> PendingDoors { get; }

        IReadOnlyCollection<MapCoordinate> VisibleSpikeTraps { get; }

        WarpState MapWarpState { get; }

        HashSet<short> UnknownPlayerIDs { get; }

        HashSet<byte> UnknownNPCIndexes { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapStateProvider, IResettable
    {
        public short CurrentMapID { get; set; }

        public bool ShowMiniMap { get; set; }

        public short JailMapID { get; set; }

        public Dictionary<int, Character.Character> Characters { get; set; }

        public HashSet<NPC.NPC> NPCs { get; set; }

        public HashSet<IItem> MapItems { get; set; }

        public HashSet<IWarp> OpenDoors { get; set; }

        public HashSet<IWarp> PendingDoors { get; set; }

        public HashSet<MapCoordinate> VisibleSpikeTraps { get; set;  }

        public WarpState MapWarpState { get; set; }

        public HashSet<short> UnknownPlayerIDs { get; set; }

        public HashSet<byte> UnknownNPCIndexes { get; set; }

        IReadOnlyDictionary<int, Character.Character> ICurrentMapStateProvider.Characters => Characters;

        IReadOnlyCollection<NPC.NPC> ICurrentMapStateProvider.NPCs => NPCs;

        IReadOnlyCollection<IItem> ICurrentMapStateProvider.MapItems => MapItems;

        IReadOnlyCollection<IWarp> ICurrentMapStateProvider.OpenDoors => OpenDoors;

        IReadOnlyCollection<IWarp> ICurrentMapStateProvider.PendingDoors => PendingDoors;

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
            MapItems = new HashSet<IItem>();
            OpenDoors = new HashSet<IWarp>();
            PendingDoors = new HashSet<IWarp>();
            VisibleSpikeTraps = new HashSet<MapCoordinate>();
            UnknownPlayerIDs = new HashSet<short>();
            UnknownNPCIndexes = new HashSet<byte>();

            MapWarpState = WarpState.None;
        }
    }
}
