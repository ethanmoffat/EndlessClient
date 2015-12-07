// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EOLib;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	//todo: track last player action time for the AFK emotes

	public class ArrowKeyListener : InputKeyListenerBase
	{
		private DateTime? _startWalkingThroughPlayerTime;

		public ArrowKeyListener()
		{
			if (Game.Components.Any(x => x is ArrowKeyListener))
				throw new InvalidOperationException("The game already contains an arrow key listener");
			Game.Components.Add(this);
		}

		public override void Update(GameTime gameTime)
		{
			if (!IgnoreInput && Character.State != CharacterActionState.Walking)
			{
				UpdateInputTime();
				KeyboardState currentKeyState = Keyboard.GetState();

				EODirection direction = _getDirectionFromKeyPress(currentKeyState);
				if (direction != EODirection.Invalid) //invalid direction: arrow key was not pressed
				{
					byte destX, destY;
					_getDestCoordinates(direction, out destX, out destY);

					if (Character.RenderData.facing != direction) //face correct direction if needed
					{
						Character.Face(direction);
					}
					else if(destX < 255 && destY < 255)
					{
						_checkSpecAndWalkIfValid(destX, destY, direction);
					}
				}
			}
			base.Update(gameTime);
		}

		private EODirection _getDirectionFromKeyPress(KeyboardState currentKeyState)
		{
			EODirection direction;
			if (IsKeyPressed(Keys.Up, currentKeyState))
				direction = EODirection.Up;
			else if (IsKeyPressed(Keys.Down, currentKeyState))
				direction = EODirection.Down;
			else if (IsKeyPressed(Keys.Left, currentKeyState))
				direction = EODirection.Left;
			else if (IsKeyPressed(Keys.Right, currentKeyState))
				direction = EODirection.Right;
			else
				direction = EODirection.Invalid;

			return direction;
		}

		private void _getDestCoordinates(EODirection direction, out byte destX, out byte destY)
		{
			switch (direction)
			{
				case EODirection.Up:
					destX = (byte) Character.X;
					destY = (byte) (Character.Y - 1);
					break;
				case EODirection.Down:
					destX = (byte) Character.X;
					destY = (byte) (Character.Y + 1);
					break;
				case EODirection.Right:
					destX = (byte) (Character.X + 1);
					destY = (byte) Character.Y;
					break;
				case EODirection.Left:
					destX = (byte) (Character.X - 1);
					destY = (byte) Character.Y;
					break;
				default:
					destX = destY = 255;
					break;
			}
		}

		private void _checkSpecAndWalkIfValid(byte destX, byte destY, EODirection direction)
		{
			var mapRend = World.Instance.ActiveMapRenderer;
			TileInfo info = mapRend.GetTileInfo(destX, destY);

			Tile tileAtDest = null;
			if (destY <= mapRend.MapRef.Height && destX <= mapRend.MapRef.Width)
				tileAtDest = mapRend.MapRef.TileLookup[destY, destX];
			var specAtDest = tileAtDest != null ? tileAtDest.spec : TileSpec.None;

			switch (info.ReturnType)
			{
				case TileInfoReturnType.IsOtherPlayer:
					if (Renderer.NoWall) goto case TileInfoReturnType.IsTileSpec;

					EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION,
						DATCONST2.STATUS_LABEL_KEEP_MOVING_THROUGH_PLAYER);
					if (_startWalkingThroughPlayerTime == null)
						_startWalkingThroughPlayerTime = DateTime.Now;
					else if ((DateTime.Now - _startWalkingThroughPlayerTime.Value).TotalSeconds > 5)
					{
						_startWalkingThroughPlayerTime = null;
						goto case TileInfoReturnType.IsTileSpec;
					}
					break;
				case TileInfoReturnType.IsOtherNPC:
					if (Renderer.NoWall) goto case TileInfoReturnType.IsTileSpec;
					break;
				case TileInfoReturnType.IsWarpSpec:
					if (Renderer.NoWall) goto case TileInfoReturnType.IsTileSpec;
					if (info.Warp.door != DoorSpec.NoDoor)
					{
						DoorSpec doorOpened;
						if (!info.Warp.doorOpened && !info.Warp.backOff)
						{
							if ((doorOpened = Character.CanOpenDoor(info.Warp)) == DoorSpec.Door)
								mapRend.StartOpenDoor(info.Warp, destX, destY);
						}
						else
						{
							//normal walking
							if ((doorOpened = Character.CanOpenDoor(info.Warp)) == DoorSpec.Door)
								_walkIfValid(TileSpec.None, direction, destX, destY);
						}

						if (doorOpened != DoorSpec.Door)
						{
							string strWhichKey = "[error key?]";
							switch (doorOpened)
							{
								case DoorSpec.LockedCrystal:
									strWhichKey = "Crystal Key";
									break;
								case DoorSpec.LockedSilver:
									strWhichKey = "Silver Key";
									break;
								case DoorSpec.LockedWraith:
									strWhichKey = "Wraith Key";
									break;
							}

							EODialog.Show(DATCONST1.DOOR_LOCKED, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
							((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING,
								DATCONST2.STATUS_LABEL_THE_DOOR_IS_LOCKED_EXCLAMATION,
								" - " + strWhichKey);
						}
					}
					else if (info.Warp.levelRequirement != 0 && Character.Stats.Level < info.Warp.levelRequirement)
					{
						EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING,
							DATCONST2.STATUS_LABEL_NOT_READY_TO_USE_ENTRANCE,
							" - LVL " + info.Warp.levelRequirement);
					}
					else
					{
						//normal walking
						_walkIfValid(TileSpec.None, direction, destX, destY);
					}
					break;
				case TileInfoReturnType.IsTileSpec:
					_walkIfValid(specAtDest, direction, destX, destY);
					break;
			}
		}

		private void _walkIfValid(TileSpec spec, EODirection dir, byte destX, byte destY)
		{
			bool walkValid = true;
			switch (spec)
			{
				case TileSpec.ChairDown: //todo: make character sit in chairs
				case TileSpec.ChairLeft:
				case TileSpec.ChairRight:
				case TileSpec.ChairUp:
				case TileSpec.ChairDownRight:
				case TileSpec.ChairUpLeft:
				case TileSpec.ChairAll:
					walkValid = Renderer.NoWall;
					break;
				case TileSpec.Chest:
					walkValid = Renderer.NoWall;
					if (!walkValid)
					{
						MapChest chest = World.Instance.ActiveMapRenderer.MapRef.Chests.Find(_c => _c.x == destX && _c.y == destY);
						if (chest != null)
						{
							string requiredKey = null;
							switch (Character.CanOpenChest(chest))
							{
								case ChestKey.Normal: requiredKey = "Normal Key"; break;
								case ChestKey.Silver: requiredKey = "Silver Key"; break;
								case ChestKey.Crystal: requiredKey = "Crystal Key"; break;
								case ChestKey.Wraith: requiredKey = "Wraith Key"; break;
								default:
									EOChestDialog.Show(((EOGame)Game).API, chest.x, chest.y);
									break;
							}

							if (requiredKey != null)
							{
								EODialog.Show(DATCONST1.CHEST_LOCKED, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
								((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION,
									" - " + requiredKey);
							}
						}
						else
						{
							EOChestDialog.Show(((EOGame)Game).API, destX, destY);
						}
					}
					break;
				case TileSpec.BankVault:
					walkValid = Renderer.NoWall;
					if (!walkValid)
					{
						EOLockerDialog.Show(((EOGame)Game).API, destX, destY);
					}
					break;
				case TileSpec.SpikesTrap:
					World.Instance.ActiveMapRenderer.AddVisibleSpikeTrap(destX, destY);
					break;
				case TileSpec.Board1: //todo: boards?
				case TileSpec.Board2:
				case TileSpec.Board3:
				case TileSpec.Board4:
				case TileSpec.Board5:
				case TileSpec.Board6:
				case TileSpec.Board7:
				case TileSpec.Board8:
					walkValid = Renderer.NoWall;
					break;
				case TileSpec.Jukebox: //todo: jukebox?
					walkValid = Renderer.NoWall;
					break;
				case TileSpec.MapEdge:
				case TileSpec.Wall:
					walkValid = Renderer.NoWall;
					break;
			}

			if (Character.State != CharacterActionState.Walking && walkValid)
			{
				Character.Walk(dir, destX, destY, Renderer.NoWall);
				Renderer.PlayerWalk(spec == TileSpec.Water, spec == TileSpec.SpikesTrap);
			}
		}
	}
}
