// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Old;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialogItem : XNAControl
    {
        private ItemRecord m_info;
        private Texture2D m_gfx;
        private Rectangle m_area;

        public EquipLocation EquipLoc { get; private set; }
        //public short ItemID { get { return (short)(m_info ?? new ItemRecord()).ID; } }

        private readonly PacketAPI m_api;

        public PaperdollDialogItem(PacketAPI api, Rectangle location, EOPaperdollDialog parent, ItemRecord info, EquipLocation locationEnum)
            : base(null, null, parent)
        {
            m_api = api;
            SetInfo(location, info);
            EquipLoc = locationEnum;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Game.IsActive) return;

            MouseState currentState = Mouse.GetState();

            if (MouseOver && !MouseOverPreviously)
            {
                DATCONST2 msg;
                switch (EquipLoc)
                {
                    case EquipLocation.Boots: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_BOOTS_EQUIPMENT; break;
                    case EquipLocation.Accessory: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_MISC_EQUIPMENT; break;
                    case EquipLocation.Gloves: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_GLOVES_EQUIPMENT; break;
                    case EquipLocation.Belt: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_BELT_EQUIPMENT; break;
                    case EquipLocation.Armor: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_ARMOR_EQUIPMENT; break;
                    case EquipLocation.Necklace: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_NECKLACE_EQUIPMENT; break;
                    case EquipLocation.Hat: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_HAT_EQUIPMENT; break;
                    case EquipLocation.Shield: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_SHIELD_EQUIPMENT; break;
                    case EquipLocation.Weapon: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_WEAPON_EQUIPMENT; break;
                    case EquipLocation.Ring1:
                    case EquipLocation.Ring2: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_RING_EQUIPMENT; break;
                    case EquipLocation.Armlet1:
                    case EquipLocation.Armlet2: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_ARMLET_EQUIPMENT; break;
                    case EquipLocation.Bracer1:
                    case EquipLocation.Bracer2: msg = DATCONST2.STATUS_LABEL_PAPERDOLL_BRACER_EQUIPMENT; break;
                    default: throw new ArgumentOutOfRangeException();
                }

                if (m_info != null)
                    EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, msg, ", " + m_info.Name);
                else
                    EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, msg);
            }

            //unequipping an item via right-click
            if (m_info != null && MouseOver && currentState.RightButton == ButtonState.Released &&
                PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                if (((EOPaperdollDialog)parent).CharRef == OldWorld.Instance.MainPlayer.ActiveCharacter)
                { //the parent dialog must show equipment for mainplayer
                    if (m_info.Special == ItemSpecial.Cursed)
                    {
                        EOMessageBox.Show(DATCONST1.ITEM_IS_CURSED_ITEM, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    }
                    else if (!((EOGame)Game).Hud.InventoryFits((short)m_info.ID))
                    {
                        ((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_ITEM_UNEQUIP_NO_SPACE_LEFT);
                    }
                    else
                    {
                        _setSize(m_area.Width, m_area.Height);
                        DrawLocation = new Vector2(m_area.X + (m_area.Width / 2 - DrawArea.Width / 2),
                            m_area.Y + (m_area.Height / 2 - DrawArea.Height / 2));

                        //put back in the inventory by the packet handler response
                        string locName = Enum.GetName(typeof(EquipLocation), EquipLoc);
                        if (!string.IsNullOrEmpty(locName))
                            m_api.UnequipItem((short)m_info.ID, (byte)(locName.Contains("2") ? 1 : 0));

                        m_info = null;
                        m_gfx = null;
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (m_gfx == null) return;
            SpriteBatch.Begin();
            SpriteBatch.Draw(m_gfx, DrawAreaWithOffset, Color.White);
            SpriteBatch.End();
        }

        public void SetInfo(Rectangle location, ItemRecord info)
        {
            m_info = info;
            if (info != null)
            {
                m_gfx = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.Items, 2 * info.Graphic, true);
            }
            m_area = location;

            if (m_gfx != null)
                _setSize(m_gfx.Width, m_gfx.Height);
            else
                _setSize(location.Width, location.Height);

            DrawLocation = new Vector2(location.X + (m_area.Width / 2 - DrawArea.Width / 2), location.Y + (m_area.Height / 2 - DrawArea.Height / 2));
        }
    }
}
