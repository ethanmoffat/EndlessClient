// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.HUD;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class EOPaperdollDialog : EODialogBase
    {
        public static EOPaperdollDialog Instance { get; private set; }

        public static void Show(PacketAPI api, Character character, PaperdollDisplayData data)
        {
            if (Instance != null)
                return;
            Instance = new EOPaperdollDialog(api, character, data);
            Instance.DialogClosing += (o, e) => Instance = null;
        }

        public Character CharRef { get; private set; }

        private readonly Texture2D m_characterIcon;

        private static readonly Rectangle m_characterIconRect = new Rectangle(227, 258, 44, 21);

        private EOPaperdollDialog(PacketAPI api, Character character, PaperdollDisplayData data)
            : base(api)
        {
            if (Instance != null)
                throw new InvalidOperationException("Paperdoll is already open!");
            Instance = this;

            CharRef = character;

            Texture2D bgSprites = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 49);
            _setSize(bgSprites.Width, bgSprites.Height / 2);

            Color[] dat = new Color[DrawArea.Width * DrawArea.Height];
            bgTexture = new Texture2D(Game.GraphicsDevice, DrawArea.Width, DrawArea.Height);
            bgSprites.GetData(0, DrawArea.WithPosition(new Vector2(0, CharRef.RenderData.gender * DrawArea.Height)), dat, 0, dat.Length);
            bgTexture.SetData(dat);

            //not using caption/message since we have other shit to take care of

            //ok button
            XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(276, 253), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok)) { Visible = true };
            ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
            ok.SetParent(this);
            dlgButtons.Add(ok);

            //items
            for (int i = (int)EquipLocation.Boots; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
            {
                ItemRecord info = OldWorld.Instance.EIF.GetRecordByID(CharRef.PaperDoll[i]);

                Rectangle itemArea = _getEquipLocRectangle((EquipLocation)i);

                //create item using itemArea
                if (CharRef.PaperDoll[i] > 0)
                {
                    // ReSharper disable once UnusedVariable
                    PaperdollDialogItem nextItem = new PaperdollDialogItem(m_api, itemArea, this, info, (EquipLocation)i); //auto-added as child of this dialog
                }
                else
                {
                    // ReSharper disable once UnusedVariable
                    PaperdollDialogItem nextItem = new PaperdollDialogItem(m_api, itemArea, this, null, (EquipLocation)i);
                }
            }

            //labels next
            XNALabel[] labels =
            {
                new XNALabel(new Rectangle(228, 22, 1, 1), Constants.FontSize08pt5)
                {
                    Text = CharRef.Name.Length > 0 ? char.ToUpper(CharRef.Name[0]) + CharRef.Name.Substring(1) : ""
                }, //name
                new XNALabel(new Rectangle(228, 52, 1, 1), Constants.FontSize08pt5)
                {
                    Text = data.Home.Length > 0 ? char.ToUpper(data.Home[0]) + data.Home.Substring(1) : ""
                }, //home
                new XNALabel(new Rectangle(228, 82, 1, 1), Constants.FontSize08pt5)
                {
                    Text = (OldWorld.Instance.ECF.GetRecordByID(CharRef.Class) ?? new ClassRecord(0)).Name
                }, //class
                new XNALabel(new Rectangle(228, 112, 1, 1), Constants.FontSize08pt5)
                {
                    Text = data.Partner.Length > 0 ? char.ToUpper(data.Partner[0]) + data.Partner.Substring(1) : ""
                }, //partner
                new XNALabel(new Rectangle(228, 142, 1, 1), Constants.FontSize08pt5)
                {
                    Text = CharRef.Title.Length > 0 ? char.ToUpper(CharRef.Title[0]) + CharRef.Title.Substring(1) : ""
                }, //title
                new XNALabel(new Rectangle(228, 202, 1, 1), Constants.FontSize08pt5)
                {
                    Text = data.Guild.Length > 0 ? char.ToUpper(data.Guild[0]) + data.Guild.Substring(1) : ""
                }, //guild
                new XNALabel(new Rectangle(228, 232, 1, 1), Constants.FontSize08pt5)
                {
                    Text = data.Rank.Length > 0 ? char.ToUpper(data.Rank[0]) + data.Rank.Substring(1) : ""
                } //rank
            };

            labels.ToList().ForEach(_l => { _l.ForeColor = ColorConstants.LightGrayText; _l.SetParent(this); });

            ChatType iconType = OldChatRenderer.GetChatTypeFromPaperdollIcon(data.Icon);
            m_characterIcon = OldChatTab.GetChatIcon(iconType);

            //should not be centered vertically: only display in game window
            //first center in the game display window, then move it 15px from top, THEN call end constructor logic
            //if not done in this order some items DrawAreaWithOffset field does not get updated properly when setting DrawLocation
            Center(Game.GraphicsDevice);
            DrawLocation = new Vector2(DrawLocation.X, 15);
            endConstructor(false);
        }

        public override void Update(GameTime gt)
        {
            if (!Game.IsActive) return;

            if (EOGame.Instance.Hud.IsInventoryDragging())
            {
                shouldClickDrag = false;
                SuppressParentClickDrag(true);
            }
            else
            {
                shouldClickDrag = true;
                SuppressParentClickDrag(false);
            }

            base.Update(gt);
        }

        public override void Draw(GameTime gt)
        {
            base.Draw(gt);
            SpriteBatch.Begin();
            SpriteBatch.Draw(m_characterIcon,
                new Vector2(
                    DrawAreaWithOffset.X + m_characterIconRect.X + (m_characterIconRect.Width / 2) - (m_characterIcon.Width / 2),
                    DrawAreaWithOffset.Y + m_characterIconRect.Y + (m_characterIconRect.Height / 2) - (m_characterIcon.Height / 2)),
                Color.White);
            SpriteBatch.End();
        }

        public void SetItem(EquipLocation loc, ItemRecord info)
        {
            PaperdollDialogItem itemToUpdate = (PaperdollDialogItem)children.Find(_ctrl =>
            {
                PaperdollDialogItem item = _ctrl as PaperdollDialogItem;
                if (item == null) return false;
                return item.EquipLoc == loc;
            });
            if (itemToUpdate != null)
                itemToUpdate.SetInfo(_getEquipLocRectangle(loc), info);
        }

        private static Rectangle _getEquipLocRectangle(EquipLocation loc)
        {
            Rectangle itemArea;
            switch (loc)
            {
                case EquipLocation.Boots:
                    itemArea = new Rectangle(87, 220, 56, 54);
                    break;
                case EquipLocation.Accessory:
                    itemArea = new Rectangle(55, 250, 23, 23);
                    break;
                case EquipLocation.Gloves:
                    itemArea = new Rectangle(22, 188, 56, 54);
                    break;
                case EquipLocation.Belt:
                    itemArea = new Rectangle(87, 188, 56, 23);
                    break;
                case EquipLocation.Armor:
                    itemArea = new Rectangle(86, 82, 56, 98);
                    break;
                case EquipLocation.Necklace:
                    itemArea = new Rectangle(152, 51, 56, 23);
                    break;
                case EquipLocation.Hat:
                    itemArea = new Rectangle(87, 21, 56, 54);
                    break;
                case EquipLocation.Shield:
                    itemArea = new Rectangle(152, 82, 56, 98);
                    break;
                case EquipLocation.Weapon:
                    itemArea = new Rectangle(22, 82, 56, 98);
                    break;
                case EquipLocation.Ring1:
                    itemArea = new Rectangle(152, 190, 23, 23);
                    break;
                case EquipLocation.Ring2:
                    itemArea = new Rectangle(185, 190, 23, 23);
                    break;
                case EquipLocation.Armlet1:
                    itemArea = new Rectangle(152, 220, 23, 23);
                    break;
                case EquipLocation.Armlet2:
                    itemArea = new Rectangle(185, 220, 23, 23);
                    break;
                case EquipLocation.Bracer1:
                    itemArea = new Rectangle(152, 250, 23, 23);
                    break;
                case EquipLocation.Bracer2:
                    itemArea = new Rectangle(185, 250, 23, 23);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("loc", "That is not a valid equipment location");
            }
            return itemArea;
        }
    }
}
