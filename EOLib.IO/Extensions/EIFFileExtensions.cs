using EOLib.IO.Pub;
using System.Linq;

namespace EOLib.IO.Extensions
{
    public static class EIFFileExtensions
    {
        public static bool IsShieldOnBack(this IPubFile<EIFRecord> itemFile, short graphic)
        {
            if (itemFile == null || itemFile.Data == null)
                return false;

            var shieldInfo = itemFile.Data.SingleOrDefault(x => x.Type == ItemType.Shield && x.DollGraphic == graphic);

            return shieldInfo != null &&
                    (shieldInfo.Name == "Bag" ||
                    shieldInfo.SubType == ItemSubType.Arrows ||
                    shieldInfo.SubType == ItemSubType.Wings);
        }
    }
}
