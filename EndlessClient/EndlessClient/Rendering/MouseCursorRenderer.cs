// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Inventory;
using EOLib;
using EOLib.Data.Map;
using EOLib.Graphics;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Rendering
{
	public sealed class MouseCursorRenderer : IDisposable
	{
		private readonly EOGame _game;
		private readonly MapRenderer _parentMapRenderer;

		private readonly Texture2D _mouseCursor;
		private readonly XNALabel _itemHoverName;
		private readonly EOMapContextMenu _contextMenu;

		private Vector2 _cursorPos;
		private int _gridX, _gridY;
		private Rectangle _cursorSourceRect;
		private MouseState _prevState;
		private bool _hideCursor;

		private MapFile MapRef { get { return _parentMapRenderer.MapRef; } }

		public Point GridCoords
		{
			get { return new Point(_gridX, _gridY); }
		}

		public MouseCursorRenderer(EOGame game, MapRenderer parentMapRenderer)
		{
			_game = game;
			_parentMapRenderer = parentMapRenderer;

			_mouseCursor = game.GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 24, true);
			_itemHoverName = new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08pt75)
			{
				Visible = true,
				Text = "",
				ForeColor = Color.White,
				DrawOrder = (int)ControlDrawLayer.BaseLayer + 3,
				AutoSize = false
			};

			_cursorSourceRect = new Rectangle(0, 0, _mouseCursor.Width / 5, _mouseCursor.Height);

			_contextMenu = new EOMapContextMenu(_game.API);
		}

		public void ShowContextMenu(CharacterRenderer player)
		{
			_contextMenu.SetCharacterRenderer(player);
		}

		public void Update()
		{
			var ms = Mouse.GetState();
			UpdateCursorInfo(ms);
			_prevState = ms;
		}

		public void Draw(SpriteBatch sb, bool beginHasBeenCalled = true)
		{
			if (_hideCursor)
				return;

			if (_gridX >= 0 && _gridY >= 0 && _gridX <= MapRef.Width && _gridY <= MapRef.Height)
			{
				//don't draw cursor if context menu is visible and the context menu has the mouse over it
				if (!(_contextMenu.Visible && _contextMenu.MouseOver))
				{
					if (!beginHasBeenCalled)
						sb.Begin();

					sb.Draw(_mouseCursor, _cursorPos, _cursorSourceRect, Color.White);

					if (!beginHasBeenCalled)
						sb.End();
				}
			}
		}

		private void UpdateCursorInfo(MouseState ms)
		{
			//don't do the cursor if there is a dialog open or the mouse is over the context menu
			if (XNAControl.Dialogs.Count > 0 || (_contextMenu.Visible && _contextMenu.MouseOver))
				return;

			Character c = World.Instance.MainPlayer.ActiveCharacter;
			SetGridCoordsBasedOnMousePosition(ms, c);

			_cursorPos = MapRenderer.GetDrawCoordinatesFromGridUnits(_gridX, _gridY, c);

			if (_gridX >= 0 && _gridX <= MapRef.Width && _gridY >= 0 && _gridY <= MapRef.Height)
			{
				bool mouseClicked = ms.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed;
				//bool rightClicked = ms.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed;

				//don't handle mouse clicks for map if there is a dialog being shown
				mouseClicked &= XNAControl.Dialogs.Count == 0;
				//rightClicked &= XNAControl.Dialogs.Count == 0;

				var ti = _parentMapRenderer.GetTileInfo((byte)_gridX, (byte)_gridY);
				switch (ti.ReturnType)
				{
					case TileInfoReturnType.IsOtherPlayer:
					case TileInfoReturnType.IsOtherNPC:
						_cursorSourceRect.Location = new Point(_mouseCursor.Width / 5, 0);
						break;
					default: //TileSpec, warp, sign
						if (_gridX == c.X && _gridY == c.Y)
							goto case TileInfoReturnType.IsOtherPlayer; //same logic if it's the active character

						_hideCursor = false;
						if (ti.ReturnType == TileInfoReturnType.IsTileSpec)
						{
							UpdateCursorForTileSpec(ti, mouseClicked, c);
						}
						else if (ti.ReturnType == TileInfoReturnType.IsMapSign)
						{
							var signInfo = (MapSign)ti.MapElement;
							_hideCursor = true;
							if (mouseClicked)
								EOMessageBox.Show(signInfo.message, signInfo.title, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
						}
						else
							_cursorSourceRect.Location = new Point(0, 0);

						if (mouseClicked && World.Instance.MainPlayer.ActiveCharacter.NeedsSpellTarget)
						{
							//cancel spell targeting if an invalid target was selected
							World.Instance.MainPlayer.ActiveCharacter.SelectSpell(-1);
						}

						break;
				}

				UpdateDisplayedMapItemName(mouseClicked);

				if (_itemHoverName.Text.Length > 0 && !_game.Components.Contains(_itemHoverName))
					_game.Components.Add(_itemHoverName);
			}
		}

		private void UpdateDisplayedMapItemName(bool mouseClicked)
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;

			var topMapItem = _parentMapRenderer.GetMapItemAt(_gridX, _gridY);

			if (topMapItem.HasValue)
			{
				MapItem mi = topMapItem.Value;
				_cursorSourceRect.Location = new Point(2 * (_mouseCursor.Width / 5), 0);

				string itemName = EOInventoryItem.GetNameString(mi.id, mi.amount);
				if (_itemHoverName.Text != itemName)
				{
					_itemHoverName.Visible = true;
					_itemHoverName.Text = EOInventoryItem.GetNameString(mi.id, mi.amount);
					_itemHoverName.ResizeBasedOnText();
					_itemHoverName.ForeColor = EOInventoryItem.GetItemTextColor(mi.id);
				}
				_itemHoverName.DrawLocation = new Vector2(
					_cursorPos.X + 32 - _itemHoverName.ActualWidth / 2f,
					_cursorPos.Y - _itemHoverName.ActualHeight - 4);

				if (mouseClicked)
				{
					if ((World.Instance.MainPlayer.ActiveCharacter.ID != mi.playerID && mi.playerID != 0) &&
						(mi.npcDrop && (DateTime.Now - mi.time).TotalSeconds <= World.Instance.NPCDropProtectTime) ||
						(!mi.npcDrop && (DateTime.Now - mi.time).TotalSeconds <= World.Instance.PlayerDropProtectTime))
					{
						Character charRef = _parentMapRenderer.GetOtherPlayerByID((short)mi.playerID);
						DATCONST2 msg = charRef == null
							? DATCONST2.STATUS_LABEL_ITEM_PICKUP_PROTECTED
							: DATCONST2.STATUS_LABEL_ITEM_PICKUP_PROTECTED_BY;
						string extra = charRef == null ? "" : charRef.Name;
						EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, msg, extra);
					}
					else
					{
						if (!EOGame.Instance.Hud.InventoryFits(mi.id))
						{
							EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION,
								DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
						}
						else if (c.Weight + (World.Instance.EIF.GetItemRecordByID(mi.id).Weight * mi.amount) > c.MaxWeight)
						{
							EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.DIALOG_ITS_TOO_HEAVY_WEIGHT);
						}
						else if (!_game.API.GetItem(mi.uid)) //server validates drop protection anyway
							EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
					}
				}
			}
			else if (_itemHoverName.Visible)
			{
				_itemHoverName.Visible = false;
				_itemHoverName.Text = " ";
			}
		}

		private void UpdateCursorForTileSpec(ITileInfo ti, bool mouseClicked, Character c)
		{
			switch (ti.Spec)
			{
				case TileSpec.Wall:
				case TileSpec.JammedDoor:
				case TileSpec.MapEdge:
				case TileSpec.FakeWall:
					//hide cursor
					_hideCursor = true;
					break;
				case TileSpec.Chest:
					//chest click action
					_cursorSourceRect.Location = new Point(_mouseCursor.Width / 5, 0);
					if (mouseClicked && Math.Max(c.X - _gridX, c.Y - _gridY) <= 1 && (_gridX == c.X || _gridY == c.Y))
					//must be directly adjacent
					{
						MapChest chest = MapRef.Chests.Find(_mc => _mc.x == _gridX && _mc.y == _gridY);
						if (chest == null) break;

						string requiredKey = null;
						switch (World.Instance.MainPlayer.ActiveCharacter.CanOpenChest(chest))
						{
							case ChestKey.Normal:
								requiredKey = "Normal Key";
								break;
							case ChestKey.Silver:
								requiredKey = "Silver Key";
								break;
							case ChestKey.Crystal:
								requiredKey = "Crystal Key";
								break;
							case ChestKey.Wraith:
								requiredKey = "Wraith Key";
								break;
							default:
								ChestDialog.Show(_game.API, chest.x, chest.y);
								break;
						}

						if (requiredKey != null)
						{
							EOMessageBox.Show(DATCONST1.CHEST_LOCKED, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
							_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING,
								DATCONST2.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION,
								" - " + requiredKey);
						}
					}
					break;
				case TileSpec.BankVault:
					_cursorSourceRect.Location = new Point(_mouseCursor.Width / 5, 0);
					if (mouseClicked && Math.Max(c.X - _gridX, c.Y - _gridY) <= 1 && (_gridX == c.X || _gridY == c.Y))
					{
						LockerDialog.Show(_game.API, (byte)_gridX, (byte)_gridY);
					}
					break;
				case TileSpec.ChairDown:
				case TileSpec.ChairLeft:
				case TileSpec.ChairRight:
				case TileSpec.ChairUp:
				case TileSpec.ChairDownRight:
				case TileSpec.ChairUpLeft:
				case TileSpec.ChairAll:
				case TileSpec.Board1:
				case TileSpec.Board2:
				case TileSpec.Board3:
				case TileSpec.Board4:
				case TileSpec.Board5:
				case TileSpec.Board6:
				case TileSpec.Board7:
				case TileSpec.Board8:
				case TileSpec.Jukebox:
					//highlight cursor
					_cursorSourceRect.Location = new Point(_mouseCursor.Width / 5, 0);
					break;
				case TileSpec.Jump:
				case TileSpec.Water:
				case TileSpec.Arena:
				case TileSpec.AmbientSource:
				case TileSpec.SpikesStatic:
				case TileSpec.SpikesTrap:
				case TileSpec.SpikesTimed:
				case TileSpec.None:
					//normal cursor
					_cursorSourceRect.Location = new Point(0, 0);
					break;
			}
		}

		private void SetGridCoordsBasedOnMousePosition(MouseState ms, Character c)
		{
			//need to solve this system of equations to get x, y on the grid
			//(x * 32) - (y * 32) + 288 - c.OffsetX, => pixX = 32x - 32y + 288 - c.OffsetX
			//(y * 16) + (x * 16) + 144 - c.OffsetY  => 2pixY = 32y + 32x + 288 - 2c.OffsetY
			//										 => 2pixY + pixX = 64x + 576 - c.OffsetX - 2c.OffsetY
			//										 => 2pixY + pixX - 576 + c.OffsetX + 2c.OffsetY = 64x
			//										 => _gridX = (pixX + 2pixY - 576 + c.OffsetX + 2c.OffsetY) / 64; <=
			//pixY = (_gridX * 16) + (_gridY * 16) + 144 - c.OffsetY =>
			//(pixY - (_gridX * 16) - 144 + c.OffsetY) / 16 = _gridY

			//center the cursor on the mouse pointer
			var msX = ms.X - _cursorSourceRect.Width/2;
			var msY = ms.Y - _cursorSourceRect.Height/2;
			//align cursor to grid based on mouse position
			_gridX = (int) Math.Round((msX + 2*msY - 576 + c.OffsetX + 2*c.OffsetY)/64.0);
			_gridY = (int) Math.Round((msY - _gridX*16 - 144 + c.OffsetY)/16.0);
		}

		#region IDisposable

		~MouseCursorRenderer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_itemHoverName.Close();
				_contextMenu.Close();
			}
		}

		#endregion
	}
}
