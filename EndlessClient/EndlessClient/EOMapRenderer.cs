using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using EndlessClient.Handlers;
using EOLib;
using EOLib.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient
{
	public enum WarpAnimation
	{
		None,
		Scroll,
		Admin,
		Invalid = 255
	}

	//returned from CheckCoordinates
	//convenience wrapper
	public struct TileInfo
	{
		public enum ReturnType //this struct is used sort of like a union - different data returned in different cases
		{
			IsTileSpec, //indicates that a normal tile spec is returned
			IsWarpSpec, //indicates that a normal warp spec is returned
			IsOtherPlayer, //other player is in the way, spec/warp are invalid
			IsOtherNPC //other npc is in the way, spec/warp are invalid
		}

		public ReturnType ReturnValue;

		public TileSpec Spec;
		public EOLib.Warp Warp;
	}

	public class EOMapRenderer : DrawableGameComponent
	{
		public List<MapItem> MapItems { get; private set; }
		private readonly List<Character> otherPlayers = new List<Character>();
		private readonly List<EOCharacterRenderer> otherRenderers = new List<EOCharacterRenderer>();
		private readonly List<NPC> npcList = new List<NPC>();

		public MapFile MapRef { get; private set; }
		
		private readonly SpriteBatch sb;

		private Rectangle _animSourceRect;
		private RenderTarget2D _playerTransparentTarget; //set in _doMapDrawing if the player coordinates are within a wall/roof draw area
		private BlendState _playerBlend;

		private readonly Timer _doorTimer;
		private EOLib.Warp _door;
		private byte _doorY; //since y-coord not stored in Warp object...

		public EOMapRenderer(Game g, MapFile mapObj)
			: base(g)
		{
			if(g == null)
				throw new NullReferenceException("The game must not be null");
			if(!(g is EOGame))
				throw new ArgumentException("The game must be an EOGame instance");

			MapRef = mapObj;
			MapItems = new List<MapItem>();

			sb = new SpriteBatch(Game.GraphicsDevice);

			_animSourceRect = new Rectangle(0, 0, 64, 32);

			Visible = true;
			g.Components.Add(this);

			_doorTimer = new Timer(_doorTimerCallback);
		}

		//super basic implementation for passing on chat to the game's actual HUD
		//map renderer will have to show the speech bubble
		public void RenderChatMessage(TalkType messageType, int playerID, string message, ChatType chatType = ChatType.None)
		{
			//convert the messageType into a valid ChatTab to pass everything on to
			ChatTabs tab;
			switch (messageType)
			{
				case TalkType.Local: tab = ChatTabs.Local; break;
				case TalkType.Party: tab = ChatTabs.Group; break;
				default: throw new NotImplementedException();
			}

			//get the character name for the player ID that was received
			string playerName = otherPlayers.Find(x => x.ID == playerID).Name;

			if (EOGame.Instance.Hud == null)
				return;
			EOGame.Instance.Hud.AddChat(tab, playerName, message, chatType);

			//TODO: Add whatever magic is necessary to make chat bubble appear (different colors/transparencies for group and public)
		}

		//renders a chat message from the local mainplayer
		public void RenderLocalChatMessage(string message)
		{
			//show just the speech bubble, since this should be called from the HUD and rendered there already
		}

		public void SetActiveMap(MapFile newActiveMap)
		{
			MapRef = newActiveMap;
			MapItems.Clear();
			otherPlayers.Clear();
			npcList.Clear();

			//need to reset door-related parameters when changing maps.
			if (_door != null)
			{
				_door.doorOpened = false;
				_door = null;
				_doorY = 0;
				_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}

		public void AddOtherPlayer(Character c, WarpAnimation anim = WarpAnimation.None)
		{
			Character other;
			if ((other = otherPlayers.Find(x => x.Name == c.Name && x.ID == c.ID)) == null)
			{
				otherPlayers.Add(c);
				otherRenderers.Add(new EOCharacterRenderer(Game, c));
				otherRenderers[otherRenderers.Count - 1].Visible = true;
				otherRenderers[otherRenderers.Count - 1].Initialize();
			}
			else
			{
				other.ApplyData(c);
			}

			//TODO: Add whatever magic is necessary to make the player appear all pretty (with animation)
		}

		public void RemoveOtherPlayer(short id, WarpAnimation anim = WarpAnimation.None)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == id)) != null)
			{
				otherPlayers.Remove(c);
				otherRenderers.RemoveAll(rend => rend.Character == c);
			}

			//TODO: Add warp animation when valid
		}

		public void ClearOtherPlayers()
		{
			otherRenderers.Clear();
			otherPlayers.Clear();
		}

		public void OtherPlayerFace(short ID, EODirection direction)
		{
			Character c;
			if((c = otherPlayers.Find(cc => cc.ID == ID)) != null)
			{
				c.RenderData.SetDirection(direction);
			}
		}

		public void OtherPlayerWalk(short ID, EODirection direction, byte x, byte y)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == ID)) != null)
			{
				c.Walk(direction, x, y);
				List<EOCharacterRenderer> rends = otherRenderers.Where(rend => rend.Character == c).ToList();
				EOCharacterRenderer renderer;
				if (rends.Count > 0 && (renderer = rends[0]) != null)
				{
					renderer.PlayerWalk();//do the actual drawing of the other player walking
				}
			}
		}

		public void UpdateOtherPlayers()
		{
			//when mainplayer walks, tell other players to update!
			otherPlayers.ForEach(x => x.RenderData.SetUpdate(true));
		}

		public void UpdateOtherPlayer(short playerId, bool sound, CharRenderData newRenderData)
		{
			Character c =  playerId == World.Instance.MainPlayer.ActiveCharacter.ID ? World.Instance.MainPlayer.ActiveCharacter : otherPlayers.Find(cc => cc.ID == playerId);
			if (c != null)
			{
				c.EquipItem(ItemType.Boots, 0, newRenderData.boots, true);
				c.EquipItem(ItemType.Armor, 0, newRenderData.armor, true);
				c.EquipItem(ItemType.Hat, 0, newRenderData.hat, true);
				c.EquipItem(ItemType.Shield, 0, newRenderData.shield, true);
				c.EquipItem(ItemType.Weapon, 0, newRenderData.weapon, true);
				//todo: play sound?
			}
		}

		public void UpdateOtherPlayer(short playerId, byte hairColor, byte hairStyle = 255)
		{
			Character c = playerId == World.Instance.MainPlayer.ActiveCharacter.ID ? World.Instance.MainPlayer.ActiveCharacter : otherPlayers.Find(cc => cc.ID == playerId);
			if (c != null)
			{
				c.RenderData.SetHairColor(hairColor);
				if (hairStyle != 255) c.RenderData.SetHairStyle(hairStyle);
			}
		}

		public void AddOtherNPC(NPC newGuy)
		{
			NPC exists;
			if ((exists = npcList.Find(_npc => _npc.Index == newGuy.Index)) == null)
			{
				newGuy.Initialize();
				newGuy.Visible = true;
				npcList.Add(newGuy);
			}
			else
			{
				npcList.Remove(exists);
				newGuy.Initialize();
				newGuy.Visible = true;
				npcList.Add(newGuy);
			}
		}

		public void RemoveOtherNPC(byte index)
		{
			NPC npc = npcList.Find(_npc => _npc.Index == index);
			if (npc != null)
				npcList.Remove(npc);
		}

		public void ClearOtherNPCs()
		{
			npcList.Clear();
		}

		public void NPCWalk(byte index, byte x, byte y, EODirection dir)
		{
			NPC toWalk;
			if ((toWalk = npcList.Find(_npc => _npc.Index == index)) != null && !toWalk.Walking)
			{
				toWalk.Walk(x, y, dir); //does the rendering AND the data update (since it's coupled together for NPCs)
			}
		}

		public TileInfo CheckCoordinates(byte destX, byte destY)
		{
			if (npcList.Any(npc => npc.X == destX && npc.Y == destY))
			{
				return new TileInfo {ReturnValue = TileInfo.ReturnType.IsOtherNPC};
			}
			if (otherPlayers.Any(player => player.X == destX && player.Y == destY))
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsOtherPlayer };
			}

			List<WarpRow> warpRows = MapRef.WarpRows.FindAll(wr => wr.y == destY && wr.tiles.FindAll(t => t.x == destX).Count == 1);
			if (warpRows.Count == 1)
			{
				EOLib.Warp warp = warpRows[0].tiles.Find(ww => ww.x == destX);
				if (warpRows[0].tiles.Count > 0 && warp.x != EOLib.Warp.Empty.x)
				{
					TileInfo newInfo = new TileInfo
					{
						ReturnValue = TileInfo.ReturnType.IsWarpSpec,
						Warp = warp
					};
					return newInfo;
				}
			}

			List<TileRow> rows = MapRef.TileRows.FindAll(tr => tr.y == destY && tr.tiles.FindAll(t => t.x == destX).Count == 1);
			if (rows.Count == 1) //should only be 1 result
			{
				Tile tile = rows[0].tiles.Find(tt => tt.x == destX);
				if (rows[0].tiles.Count > 0)
				{
					return new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = tile.spec };
				}
			}

			return destX <= MapRef.Width && destY <= MapRef.Height //don't need to check zero bounds: because byte type is always positive (unsigned)
				? new TileInfo {ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = TileSpec.None}
				: new TileInfo {ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = TileSpec.MapEdge};
		}
		
		public void ToggleMapView()
		{
			//todo: determine whether or not the minimap is visible, toggle flag to draw the minimap
		}

		public void OpenDoor(byte x, short y)
		{
			if (_door != null && _door.doorOpened)
			{
				_door.doorOpened = false;
				_door.backOff = false;
				_doorY = 0;
			}

			WarpRow row;
			if ((row = MapRef.WarpRows.Find(wr => wr.y == y)).tiles.Count > 0)
			{
				if ((_door = row.tiles.Find(w => w.x == x)) != null)
				{
					_door.doorOpened = true;
					_doorY = (byte)y;
					_doorTimer.Change(3000, 0);
				}
			}
		}

		private void _doorTimerCallback(object state)
		{
			_door.doorOpened = false;
			_door.backOff = false; //back-off from sending a door packet.
			_doorY = 0;
			_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public override void Initialize()
		{
			_playerTransparentTarget = new RenderTarget2D(Game.GraphicsDevice, 
				Game.GraphicsDevice.PresentationParameters.BackBufferWidth, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None);

			_playerBlend = new BlendState
			{
				BlendFactor = new Color(255,255,255,64),

				AlphaSourceBlend = Blend.One,
				AlphaDestinationBlend = Blend.One,
				AlphaBlendFunction = BlendFunction.Add,

				ColorSourceBlend = Blend.BlendFactor,
				ColorDestinationBlend = Blend.One
			};
			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			World.Instance.ActiveCharacterRenderer.Update(gameTime);
			IEnumerable<EOCharacterRenderer> toAdd = otherRenderers.Where(rend => !Game.Components.Contains(rend));
			foreach (EOCharacterRenderer rend in toAdd)
				rend.Update(gameTime); //do update logic here: other renderers will NOT be added to Game's components

			npcList.Where(_npc => !Game.Components.Contains(_npc)).ToList().ForEach(_n => _n.Update(gameTime));

			_doMapRenderTargetDrawing(); //if any player has been updated redraw the render target

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (MapRef != null)
			{
				_doMapDrawing();
			}

			base.Draw(gameTime);
		}
		
		// Special Thanks: HotDog's client. Used heavily as a reference for numeric offsets/techniques
		private void _doMapDrawing()
		{
			sb.Begin();
			Character c = World.Instance.MainPlayer.ActiveCharacter;

			// Queries (func) for the gfx items within range of the character's X coordinate
			Func<GFX, bool> xGFXQuery = gfx => gfx.x >= c.X - Constants.ViewLength && gfx.x <= c.X + Constants.ViewLength && gfx.x <= MapRef.Width;
			// Queries (func) for the gfxrow items within range of the character's Y coordinate
			Func<GFXRow, bool> yGFXQuery = row => row.y >= c.Y - Constants.ViewLength && row.y <= c.Y + Constants.ViewLength && row.y <= MapRef.Height;

			//render fill tile first
			if (MapRef.FillTile > 0)
			{
				for (int i = 0; i <= MapRef.Height; ++i)
				{
					for (int j = 0; j <= MapRef.Width; ++j)
					{
						GFXRow tr;
						if ((tr = MapRef.GfxRows[0].Find(_tr => _tr.y == i)).y == i)
						{
							GFX t;
							if (tr.tiles != null && (t = tr.tiles.Find(_t => _t.x == j)) != null && t.x == j)
							{
								if (t.tile == 0)
									continue;
							}
						}

						Vector2 pos = _getDrawCoordinates(j, i, c);
						sb.Draw(GFXLoader.TextureFromResource(GFXTypes.MapTiles, MapRef.FillTile, true), new Vector2(pos.X - 1, pos.Y - 2),
							Color.White);
					}
				}
			}

			//ground layer next
			//use linq queries to filter the collections to only the relevant tiles
			List<GFXRow> ground = MapRef.GfxRows[0].Where(yGFXQuery).ToList();
			foreach (GFXRow row in ground)
			{
				List<GFX> tiles = row.tiles.Where(xGFXQuery).ToList();
				foreach (GFX tile in tiles)
				{
					if (tile.tile == 0)
						continue;

					//render tile.tile at tile.x, row.y
					Texture2D nextTile = GFXLoader.TextureFromResource(GFXTypes.MapTiles, tile.tile, true);
					Vector2 pos = _getDrawCoordinates(tile.x, row.y, c);
					if(nextTile.Width > 64)
						sb.Draw(nextTile, new Vector2(pos.X - 1, pos.Y - 2), _animSourceRect, Color.White);
					else
						sb.Draw(nextTile, new Vector2(pos.X - 1, pos.Y - 2), Color.White);
				}
			}

			//items next!
			List<MapItem> items = MapItems.Where(item => xGFXQuery(new GFX {x = item.x}) && yGFXQuery(new GFXRow {y = item.y})).ToList();
			foreach (MapItem item in items)
			{
				ItemRecord itemData = (ItemRecord)World.Instance.EIF.Data.Find(i => i is ItemRecord && (i as ItemRecord).ID == item.id);
				Vector2 itemPos = _getDrawCoordinates(item.x + 1, item.y, c);
				if (itemData.Type == ItemType.Money)
				{
					int gfx = item.amount >= 100000 ? 4 : (
						item.amount >= 10000 ? 3 : (
						item.amount >= 100 ? 2 : (
						item.amount >= 2 ? 1 : 0)));

					Texture2D moneyMoneyMan = GFXLoader.TextureFromResource(GFXTypes.Items, 269 + 2 * gfx, true);
					sb.Draw(moneyMoneyMan, 
						new Vector2(itemPos.X - (int)Math.Round(moneyMoneyMan.Width / 2.0), itemPos.Y - (int)Math.Round(moneyMoneyMan.Height / 2.0)), 
						Color.White);
				}
				else
				{
					Texture2D itemTexture = GFXLoader.TextureFromResource(GFXTypes.Items, 2*itemData.Graphic - 1, true);
					sb.Draw(itemTexture, new Vector2(itemPos.X - (int)Math.Round(itemTexture.Width / 2.0), itemPos.Y - (int)Math.Round(itemTexture.Height / 2.0)), Color.White);
				}
			}
			
			//TODO: cursor (follows mouse pointer)
			//sb.Draw(GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 24, true), );

			sb.End();
			
			sb.Begin();
			sb.Draw(_playerTransparentTarget, Vector2.Zero, Color.White);
			sb.End();
		}

		//The map render target works as follows:
		// 1. Clear render target as transparent
		// 2. Draw all map objects to this render target
		// 3. Draw any players (including main) to render target
		//    a. other renderers are drawn inbetween objects as needed
		//    b. main player is blended using special blendstate with any object it overlaps
		private void _doMapRenderTargetDrawing()
		{
			//todo: need to animate certain tiles! (spikes, wall flames, etc)
			//also, certain spikes only appear when a player is over them...yikes.

			if (MapRef == null) return;
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			List<Character> otherChars = new List<Character>(otherPlayers); //copy of list
			List<NPC> otherNpcs = new List<NPC>(npcList);

			// Queries (func) for the gfx items within range of the character's X coordinate
			Func<GFX, bool> xGFXQuery = gfx => gfx.x >= c.X - Constants.ViewLength && gfx.x <= c.X + Constants.ViewLength && gfx.x <= MapRef.Width;
			// Queries (func) for the gfxrow items within range of the character's Y coordinate
			Func<GFXRow, bool> yGFXQuery = row => row.y >= c.Y - Constants.ViewLength && row.y <= c.Y + Constants.ViewLength && row.y <= MapRef.Height;
			GraphicsDevice.SetRenderTarget(_playerTransparentTarget);
			GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
			sb.Begin();

			bool mainBehindSomething = false;
			//Get all the row lists in-range of player up front. Retrieved here in order to be rendered
			List<GFXRow> overlayObjRows = MapRef.GfxRows[2].Where(yGFXQuery).ToList();
			List<GFXRow> wallRowsRight = MapRef.GfxRows[4].Where(yGFXQuery).ToList();
			List<GFXRow> wallRowsDown = MapRef.GfxRows[3].Where(yGFXQuery).ToList();
			List<GFXRow> shadowRows = MapRef.GfxRows[7].Where(yGFXQuery).ToList();
			List<GFXRow> mapObjRows = MapRef.GfxRows[1].Where(yGFXQuery).ToList();
			List<GFXRow> roofRows = MapRef.GfxRows[5].Where(yGFXQuery).ToList();
			List<GFXRow> overlayTileRows = MapRef.GfxRows[6].Where(yGFXQuery).ToList();

			for (int rowIndex = 0; rowIndex <= MapRef.Height; ++rowIndex) //11-7-14: Changed rendering to do row by row for all layers
			{
				GFXRow row; //reused for each layer

				//overlay/mask  objects
				if ((row = overlayObjRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> overlayObj = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX obj in overlayObj)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapOverlay, obj.tile, true);
						Vector2 pos = _getDrawCoordinates(obj.x, row.y, c);
						pos = new Vector2(pos.X + 16, pos.Y - 11);
						_drawIfBehindSomething(otherChars, otherNpcs, obj.x, row.y, gfx.Bounds.SetPosition(pos));
						sb.Draw(gfx, pos, Color.White);

						if (!mainBehindSomething)
							mainBehindSomething = _characterIsBehindSomething(World.Instance.ActiveCharacterRenderer, obj.x, row.y,
								gfx.Bounds.SetPosition(pos));
					}
				}

				//walls - two layers: facing different directions
				//this layer faces to the right
				if ((row = wallRowsRight.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> walls = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX obj in walls)
					{
						int gfxNum = obj.tile;
						if (_door != null && _door.x == obj.x && _doorY == row.y && _door.doorOpened)
							gfxNum++;

						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
						Vector2 loc = _getDrawCoordinates(obj.x, row.y, c);
						loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 47, loc.Y - (gfx.Height - 29));
						_drawIfBehindSomething(otherChars, otherNpcs, obj.x, row.y, gfx.Bounds.SetPosition(loc));
						sb.Draw(gfx, loc, Color.White);

						if (!mainBehindSomething)
							mainBehindSomething = _characterIsBehindSomething(World.Instance.ActiveCharacterRenderer, obj.x, row.y,
								gfx.Bounds.SetPosition(loc));
					}
				}
				//this layer faces to the down
				if ((row = wallRowsDown.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> walls = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX obj in walls)
					{
						int gfxNum = obj.tile;
						if (_door != null && _door.x == obj.x && _doorY == row.y && _door.doorOpened)
							gfxNum++;

						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
						Vector2 loc = _getDrawCoordinates(obj.x, row.y, c);
						loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 15, loc.Y - (gfx.Height - 29));
						_drawIfBehindSomething(otherChars, otherNpcs, obj.x, row.y, gfx.Bounds.SetPosition(loc));
						sb.Draw(gfx, loc, Color.White);

						if (!mainBehindSomething)
							mainBehindSomething = _characterIsBehindSomething(World.Instance.ActiveCharacterRenderer, obj.x, row.y,
								gfx.Bounds.SetPosition(loc));
					}
				}

				//shadows
				//TODO: Load configuration value determining whether or not to show shadows
				if ((row = shadowRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> shadows = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX shadow in shadows)
					{
						Vector2 loc = _getDrawCoordinates(shadow.x, row.y, c);
						sb.Draw(GFXLoader.TextureFromResource(GFXTypes.Shadows, shadow.tile, true), new Vector2(loc.X - 24, loc.Y - 12), Color.FromNonPremultiplied(255, 255, 255, 60));
					}
				}

				//map objects
				if ((row = mapObjRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> objs = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX obj in objs)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapObjects, obj.tile, true);
						Vector2 loc = _getDrawCoordinates(obj.x, row.y, c);
						loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 29, loc.Y - (gfx.Height - 28));
						_drawIfBehindSomething(otherChars, otherNpcs, obj.x, row.y, gfx.Bounds.SetPosition(loc));
						sb.Draw(gfx, loc, Color.White);

						if (!mainBehindSomething)
							mainBehindSomething = _characterIsBehindSomething(World.Instance.ActiveCharacterRenderer, obj.x, row.y,
								gfx.Bounds.SetPosition(loc));
					}
				}

				//roofs (after objects - for outdoor maps, which actually have roofs, this makes more sense)
				if ((row = roofRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> roofs = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX roof in roofs)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWallTop, roof.tile, true);
						Vector2 loc = _getDrawCoordinates(roof.x, row.y, c);
						loc = new Vector2(loc.X - 2, loc.Y - 63);
						_drawIfBehindSomething(otherChars, otherNpcs, roof.x, row.y, gfx.Bounds.SetPosition(loc));
						sb.Draw(gfx, loc, Color.White);

						if (!mainBehindSomething)
							mainBehindSomething = _characterIsBehindSomething(World.Instance.ActiveCharacterRenderer, roof.x, row.y,
								gfx.Bounds.SetPosition(loc));
					}
				}

				//overlay tiles (counters, etc)
				if ((row = overlayTileRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> tiles = row.tiles.Where(xGFXQuery).ToList();
					foreach (GFX tile in tiles)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapTiles, tile.tile, true);
						Vector2 loc = _getDrawCoordinates(tile.x, row.y, c);
						loc = new Vector2(loc.X - 2, loc.Y - 31);
						_drawIfBehindSomething(otherChars, otherNpcs, tile.x, row.y, gfx.Bounds.SetPosition(loc));
						sb.Draw(gfx, loc, Color.White);

						if (!mainBehindSomething)
							mainBehindSomething = _characterIsBehindSomething(World.Instance.ActiveCharacterRenderer, tile.x, row.y,
								gfx.Bounds.SetPosition(loc));
					}
				}
			}

			sb.End();

			BlendState mainPlayerBlend = mainBehindSomething ? _playerBlend : BlendState.AlphaBlend;

			//draw any remaining characters/npcs that weren't behind something
			if (otherChars.Count > 0 || otherNpcs.Count > 0)
			{
				List<Character> drawBefore = otherChars.Where(_c => _c.X < c.X || _c.Y < c.Y).ToList();
				List<NPC> drawNPCBefore = otherNpcs.Where(_n => _n.X < c.X || _n.Y < c.Y).ToList();
				otherChars.RemoveAll(drawBefore.Contains);
				otherNpcs.RemoveAll(drawNPCBefore.Contains);
				otherRenderers.Where(_r => drawBefore.Contains(_r.Character)).ToList().ForEach(_r => _r.Draw(null)); //draw before main player
				drawNPCBefore.ForEach(_n => _n.Draw(null));

				sb.Begin(SpriteSortMode.Deferred, mainPlayerBlend);
				World.Instance.ActiveCharacterRenderer.Draw(sb, null, true); //draw main player
				sb.End();

				otherRenderers.Where(_r => otherChars.Contains(_r.Character)).ToList().ForEach(_r => _r.Draw(null)); //draw after main player
				otherNpcs.ForEach(_n => _n.Draw(null));
			}
			else //just draw main player
			{
				sb.Begin(SpriteSortMode.Deferred, mainPlayerBlend);
				World.Instance.ActiveCharacterRenderer.Draw(sb, null, true);
				sb.End();
			}

			GraphicsDevice.SetRenderTarget(null);
		}

		/// <summary>
		/// does the offset for tiles/items
		/// <para>(x * 32 - y * 32 + 288 - c.OffsetX), (y * 16 + x * 16 + 144 - c.OffsetY)</para>
		/// <para>Additional offsets for some gfx will need to be made - this Vector2 is a starting point with calculations required for ALL gfx</para>
		/// </summary>
		private Vector2 _getDrawCoordinates(int x, int y, Character c)
		{
			return new Vector2((x * 32) - (y * 32) + 288 - c.OffsetX, (y * 16) + (x * 16) + 144 - c.OffsetY);
		}

		//This is a very specific helper method, must be called during _doMapDrawing in specific places
		//Not really for other use, I just didn't want to have to copy/paste a lot of code
		private void _drawIfBehindSomething(List<Character> otherChars, List<NPC> otherNPCs, int objX, int objY, Rectangle textureBounds)
		{
			//List<Character> clipChars = otherChars.FindAll(_c => _c.X < objX && _c.Y < objY);
			//List<EOCharacterRenderer> drawTime = otherRenderers.Where(_r => clipChars.Contains(_r.Character)).ToList();
			foreach (EOCharacterRenderer _r in otherRenderers/*drawTime*/)
			{
				//if (_r.DrawAreaWithOffset.Intersects(textureBounds))
				if(otherChars.Contains(_r.Character) && _characterIsBehindSomething(_r, objX, objY, textureBounds))
				{
					sb.End();
					//character is behind the texture to be rendered AND character overlaps with texture to be rendered
					//so, draw character first and remove it from the list to be drawn
					//any characters that aren't behind anything will be drawn at the end
					_r.Draw(null);
					sb.Begin();
					otherChars.Remove(_r.Character);
				}
			}

			for(int i = otherNPCs.Count - 1; i >= 0; --i) //can't do foreach b/c modifying collection
			{
				NPC _n = otherNPCs[i];
				if (_npcIsBehindSomething(_n, objX, objY, textureBounds))
				{
					sb.End();
					_n.Draw(null);
					sb.Begin();
					otherNPCs.Remove(_n);
				}
			}
		}

		private bool _characterIsBehindSomething(EOCharacterRenderer rend, int objX, int objY, Rectangle TextureBounds)
		{
			return (rend.Character.X < objX && rend.Character.Y < objY) && rend.DrawAreaWithOffset.Intersects(TextureBounds);
		}

		private bool _npcIsBehindSomething(NPC npc, int objX, int objY, Rectangle TextureBounds)
		{
			return (npc.X < objX && npc.Y < objY) && npc.DrawArea.Intersects(TextureBounds);
		}

		public Character GetOtherPlayer(short playerId)
		{
			return otherPlayers.Find(_c => _c.ID == playerId);
		}
	}
}
