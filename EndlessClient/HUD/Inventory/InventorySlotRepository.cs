using AutomaticTypeMapper;
using EndlessClient.HUD.Panels;
using EOLib.IO.Map;

namespace EndlessClient.HUD.Inventory
{
    public interface IInventorySlotRepository
    {
        Matrix<bool> FilledSlots { get; set; }
    }

    public interface IInventorySlotProvider
    {
        IReadOnlyMatrix<bool> FilledSlots { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class InventorySlotRepository : IInventorySlotProvider, IInventorySlotRepository
    {
        public Matrix<bool> FilledSlots { get; set; }

        IReadOnlyMatrix<bool> IInventorySlotProvider.FilledSlots => FilledSlots;

        public InventorySlotRepository()
        {
            FilledSlots = new Matrix<bool>(InventoryPanel.InventoryRows, InventoryPanel.InventoryRowSlots, false);
        }
    }
}
