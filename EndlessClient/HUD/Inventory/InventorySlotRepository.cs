using AutomaticTypeMapper;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.IO.Map;
using System.Collections.Generic;

namespace EndlessClient.HUD.Inventory
{
    public interface IInventorySlotRepository
    {
        Matrix<bool> FilledSlots { get; set; }

        Dictionary<int, int> SlotMap { get; set; }
    }

    public interface IInventorySlotProvider
    {
        IReadOnlyMatrix<bool> FilledSlots { get; }

        IReadOnlyDictionary<int, int> SlotMap { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class InventorySlotRepository : IInventorySlotProvider, IInventorySlotRepository, IResettable
    {
        public Matrix<bool> FilledSlots { get; set; }

        public Dictionary<int, int> SlotMap { get; set; }

        IReadOnlyMatrix<bool> IInventorySlotProvider.FilledSlots => FilledSlots;

        IReadOnlyDictionary<int, int> IInventorySlotProvider.SlotMap => SlotMap;

        public InventorySlotRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            FilledSlots = new Matrix<bool>(InventoryPanel.InventoryRows, InventoryPanel.InventoryRowSlots, false);
            SlotMap = new Dictionary<int, int>();
        }
    }
}
