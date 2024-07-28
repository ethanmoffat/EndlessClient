using EOLib.IO;
using Microsoft.Xna.Framework;
using System;

namespace EndlessClient.Dialogs.Extensions;

public static class EquipLocationExtensions
{
    public static Rectangle GetEquipLocationRectangle(this EquipLocation loc)
    {
        switch (loc)
        {
            case EquipLocation.Boots: return new Rectangle(87, 220, 56, 54);
            case EquipLocation.Accessory: return new Rectangle(55, 250, 23, 23);
            case EquipLocation.Gloves: return new Rectangle(22, 188, 56, 54);
            case EquipLocation.Belt: return new Rectangle(87, 188, 56, 23);
            case EquipLocation.Armor: return new Rectangle(86, 82, 56, 98);
            case EquipLocation.Necklace: return new Rectangle(152, 51, 56, 23);
            case EquipLocation.Hat: return new Rectangle(87, 21, 56, 54);
            case EquipLocation.Shield: return new Rectangle(152, 82, 56, 98);
            case EquipLocation.Weapon: return new Rectangle(22, 82, 56, 98);
            case EquipLocation.Ring1: return new Rectangle(152, 190, 23, 23);
            case EquipLocation.Ring2: return new Rectangle(185, 190, 23, 23);
            case EquipLocation.Armlet1: return new Rectangle(152, 220, 23, 23);
            case EquipLocation.Armlet2: return new Rectangle(185, 220, 23, 23);
            case EquipLocation.Bracer1: return new Rectangle(152, 250, 23, 23);
            case EquipLocation.Bracer2: return new Rectangle(185, 250, 23, 23);
            default: throw new ArgumentOutOfRangeException(nameof(loc), "That is not a valid equipment location");
        }
    }
}