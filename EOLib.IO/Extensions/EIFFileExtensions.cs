using EOLib.IO.Pub;
using System.Linq;

namespace EOLib.IO.Extensions
{
    public static class EIFFileExtensions
    {
        public static bool IsRangedWeapon(this IPubFile<EIFRecord> itemFile, int graphic)
        {
            var weaponInfo = itemFile?.FirstOrDefault(x => x.Type == ItemType.Weapon && x.DollGraphic == graphic);
            return weaponInfo?.SubType == ItemSubType.Ranged;
        }
    }
}
