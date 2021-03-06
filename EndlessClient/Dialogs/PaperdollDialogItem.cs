﻿using System;
using EndlessClient.Old;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls.Old;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialogItem : XNAControl
    {
        private EIFRecord m_info;
        private Texture2D m_gfx;
        private Rectangle m_area;

        public EquipLocation EquipLoc { get; }
        //public short ItemID { get { return (short)(m_info ?? new ItemRecord()).ID; } }

        private readonly PacketAPI m_api;

        public PaperdollDialogItem(PacketAPI api, Rectangle location, EOPaperdollDialog parent, EIFRecord info, EquipLocation locationEnum)
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
                EOResourceID msg;
                switch (EquipLoc)
                {
                    case EquipLocation.Boots: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_BOOTS_EQUIPMENT; break;
                    case EquipLocation.Accessory: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_MISC_EQUIPMENT; break;
                    case EquipLocation.Gloves: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_GLOVES_EQUIPMENT; break;
                    case EquipLocation.Belt: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_BELT_EQUIPMENT; break;
                    case EquipLocation.Armor: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_ARMOR_EQUIPMENT; break;
                    case EquipLocation.Necklace: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_NECKLACE_EQUIPMENT; break;
                    case EquipLocation.Hat: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_HAT_EQUIPMENT; break;
                    case EquipLocation.Shield: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_SHIELD_EQUIPMENT; break;
                    case EquipLocation.Weapon: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_WEAPON_EQUIPMENT; break;
                    case EquipLocation.Ring1:
                    case EquipLocation.Ring2: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_RING_EQUIPMENT; break;
                    case EquipLocation.Armlet1:
                    case EquipLocation.Armlet2: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_ARMLET_EQUIPMENT; break;
                    case EquipLocation.Bracer1:
                    case EquipLocation.Bracer2: msg = EOResourceID.STATUS_LABEL_PAPERDOLL_BRACER_EQUIPMENT; break;
                    default: throw new ArgumentOutOfRangeException();
                }

                if (m_info != null)
                    EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, msg, ", " + m_info.Name);
                else
                    EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, msg);
            }

            //unequipping an item via right-click
            if (m_info != null && MouseOver && currentState.RightButton == ButtonState.Released &&
                PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                if (((EOPaperdollDialog)parent).CharRef == OldWorld.Instance.MainPlayer.ActiveCharacter)
                { //the parent dialog must show equipment for mainplayer
                    if (m_info.Special == ItemSpecial.Cursed)
                    {
                        EOMessageBox.Show(DialogResourceID.ITEM_IS_CURSED_ITEM, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    }
                    else if (!((EOGame)Game).Hud.InventoryFits((short)m_info.ID))
                    {
                        ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_UNEQUIP_NO_SPACE_LEFT);
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

        public void SetInfo(Rectangle location, EIFRecord info)
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
