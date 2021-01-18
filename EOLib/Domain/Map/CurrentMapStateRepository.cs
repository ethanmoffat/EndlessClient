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

        HashSet<ICharacter> Characters { get; set; }

        HashSet<INPC> NPCs { get; set; }

        HashSet<IItem> MapItems { get; set; }

        HashSet<IWarp> OpenDoors { get; set;  }

        HashSet<IWarp> PendingDoors { get; set; }

        WarpState MapWarpState { get; set; }
    }

    public interface ICurrentMapStateProvider
    {
        short CurrentMapID { get; }

        bool ShowMiniMap { get; }

        IReadOnlyCollection<ICharacter> Characters { get; }

        IReadOnlyCollection<INPC> NPCs { get; }

        IReadOnlyCollection<IItem> MapItems { get; }

        IReadOnlyCollection<IWarp> OpenDoors { get; }

        IReadOnlyCollection<IWarp> PendingDoors { get; }

        WarpState MapWarpState { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapStateProvider, IResettable
    {
        public short CurrentMapID { get; set; }

        public bool ShowMiniMap { get; set; }

        public HashSet<ICharacter> Characters { get; set; }

        public HashSet<INPC> NPCs { get; set; }

        public HashSet<IItem> MapItems { get; set; }

        public HashSet<IWarp> OpenDoors { get; set; }

        public HashSet<IWarp> PendingDoors { get; set; }

        public WarpState MapWarpState { get; set; }

        IReadOnlyCollection<ICharacter> ICurrentMapStateProvider.Characters => Characters;

        IReadOnlyCollection<INPC> ICurrentMapStateProvider.NPCs => NPCs;

        IReadOnlyCollection<IItem> ICurrentMapStateProvider.MapItems => MapItems;

        IReadOnlyCollection<IWarp> ICurrentMapStateProvider.OpenDoors => OpenDoors;

        IReadOnlyCollection<IWarp> ICurrentMapStateProvider.PendingDoors => PendingDoors;

        public CurrentMapStateRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            CurrentMapID = 0;
            ShowMiniMap = false;

            Characters = new HashSet<ICharacter>();
            NPCs = new HashSet<INPC>();
            MapItems = new HashSet<IItem>();
            OpenDoors = new HashSet<IWarp>();
            PendingDoors = new HashSet<IWarp>();

            MapWarpState = WarpState.None;
        }
    }
}
