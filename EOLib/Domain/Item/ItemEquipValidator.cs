using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using System.Linq;

namespace EOLib.Domain.Item
{
    [AutoMappedType]
    public class ItemEquipValidator : IItemEquipValidator
    {
        private readonly IECFFileProvider _ecfFileProvider;
        private readonly IPaperdollProvider _paperdollProvider;

        public ItemEquipValidator(IECFFileProvider ecfFileProvider,
                                  IPaperdollProvider paperdollProvider)
        {
            _ecfFileProvider = ecfFileProvider;
            _paperdollProvider = paperdollProvider;
        }

        public (ItemEquipResult, string, bool) ValidateItemEquip(Character.Character c, EIFRecord itemData)
        {
            if (!_paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(c.ID))
            {
                // emulate client login bug: when the paperdoll doesn't exist, show an "already equipped" message
                // see: https://eoserv.net/bugs/view_bug/441
                return (ItemEquipResult.AlreadyEquipped, string.Empty, false);
            }

            var paperdoll = _paperdollProvider.VisibleCharacterPaperdolls[c.ID].Paperdoll;
            var equipLocation = itemData.GetEquipLocation();

            var isAlternateEquipLocation = false;

            switch (itemData.Type)
            {
                case ItemType.Armlet:
                case ItemType.Bracer:
                case ItemType.Ring:
                    if (paperdoll[equipLocation] != 0)
                    {
                        isAlternateEquipLocation = true;
                        if (paperdoll[equipLocation + 1] != 0)
                            return (ItemEquipResult.AlreadyEquipped, string.Empty, false);
                    }
                    break;
                case ItemType.Armor:
                    if (c.RenderProperties.Gender != itemData.Gender)
                        return (ItemEquipResult.WrongGender, string.Empty, false);
                    break;
            }

            var reqs = new int[6];
            var reqNames = new[] { "STR", "INT", "WIS", "AGI", "CON", "CHA" };
            if ((reqs[0] = itemData.StrReq) > c.Stats[CharacterStat.Strength] || (reqs[1] = itemData.IntReq) > c.Stats[CharacterStat.Intelligence]
                || (reqs[2] = itemData.WisReq) > c.Stats[CharacterStat.Wisdom] || (reqs[3] = itemData.AgiReq) > c.Stats[CharacterStat.Agility]
                || (reqs[4] = itemData.ConReq) > c.Stats[CharacterStat.Constitution] || (reqs[5] = itemData.ChaReq) > c.Stats[CharacterStat.Charisma])
            {
                var req = reqs.Select((i, n) => new { Req = n, Ndx = i }).First(x => x.Req > 0);
                return (ItemEquipResult.StatRequirementNotMet, $" {reqs[req.Ndx]} {reqNames[req.Ndx]}", isAlternateEquipLocation);
            }


            if (itemData.ClassReq > 0 && itemData.ClassReq != c.ClassID)
            {
                return (ItemEquipResult.ClassRequirementNotMet, _ecfFileProvider.ECFFile[itemData.ClassReq].Name, isAlternateEquipLocation);
            }

            if (paperdoll[equipLocation] != 0 && !isAlternateEquipLocation)
            {
                return (ItemEquipResult.AlreadyEquipped, string.Empty, isAlternateEquipLocation);
            }

            return (ItemEquipResult.Ok, string.Empty, isAlternateEquipLocation);
        }
    }

    public interface IItemEquipValidator
    {
        (ItemEquipResult Result, string Detail, bool IsAlternateEquipLocation) ValidateItemEquip(Character.Character mainCharacter, EIFRecord itemData);
    }
}