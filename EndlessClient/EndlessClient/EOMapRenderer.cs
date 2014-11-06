using System;
using System.Collections.Generic;
using System.Linq;
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
		public enum Flags
		{
			IsTileSpec = 0, //indicates that a normal tile spec is returned
			IsWarpSpec = 1, //indicates that a normal warp spec is returned
			IsOtherPlayer = 2, //other player is in the way, spec/warp are invalid
			IsOtherNPC = 4 //other npc is in the way, spec/warp are invalid
		}

		public Flags ReturnValue;

		public TileSpec Spec;
		public EOLib.Warp Warp;
	}

	public class EOMapRenderer : DrawableGameComponent
	{
		public List<MapItem> MapItems { get; private set; }
		private readonly List<Character> otherPlayers = new List<Character>();
		private readonly List<EOCharacterRenderer> otherRenderers = new List<EOCharacterRenderer>();
		public List<NPC> NPCs { get; private set; }

		public MapFile MapRef { get; private set; }
		
		private readonly SpriteBatch sb;

		private Rectangle _animSourceRect;
		private bool _playerShouldBeTransparent; //set in _doMapDrawing if the player coordinates are within a wall/roof draw area

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
			NPCs = new List<NPC>();

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
			NPCs.Clear();

			//need to reset door-related parameters when changing maps.
			_door.doorOpened = false;
			_door = null;
			_doorY = 0;
			_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
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

		public void RemoveOtherPlayer(short id)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == id)) != null)
			{
				otherPlayers.Remove(c);
				otherRenderers.RemoveAll(rend => rend.Character == c);
			}
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
				EOCharacterRenderer renderer = otherRenderers.Where(rend => rend.Character == c).ElementAt(0);
				if (renderer != null)
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

		public TileInfo CheckCoordinates(byte destX, byte destY)
		{
			//TileSpec ts = MapRef.TileRows[destY].tiles[destX].spec;
			//if(ts == ??) check which tilespecs should allow walking
			if (NPCs.Any(npc => npc.X == destX && npc.Y == destY))
			{
				return new TileInfo {ReturnValue = TileInfo.Flags.IsOtherNPC};
			}
			if (otherPlayers.Any(player => player.X == destX && player.Y == destY))
			{
				return new TileInfo { ReturnValue = TileInfo.Flags.IsOtherPlayer };
			}

			List<WarpRow> warpRows = MapRef.WarpRows.FindAll(wr => wr.y == destY && wr.tiles.FindAll(t => t.x == destX).Count == 1);
			if (warpRows.Count == 1)
			{
				EOLib.Warp warp = warpRows[0].tiles.Find(ww => ww.x == destX);
				if (warpRows[0].tiles.Count > 0 && warp.x != EOLib.Warp.Empty.x)
				{
					TileInfo newInfo = new TileInfo
					{
						ReturnValue = TileInfo.Flags.IsWarpSpec,
						Warp = warp
					};
					return newInfo;
				}
			}

			List<TileRow> rows = MapRef.TileRows.FindAll(tr => tr.y == destY && tr.tiles.FindAll(t => t.x == destX).Count == 1);
			if (rows.Count == 1) //should only be 1 result
			{
				Tile tile = rows[0].tiles.Find(tt => tt.x == destX);
				if (rows[0].tiles.Count > 0 && tile.x != new Tile().x)
				{
					return new TileInfo { ReturnValue = TileInfo.Flags.IsTileSpec, Spec = tile.spec };
				}
			}

			return destX <= MapRef.Width && destY <= MapRef.Height //don't need to check zero bounds: because byte type is always positive (unsigned)
				? new TileInfo {ReturnValue = TileInfo.Flags.IsTileSpec, Spec = TileSpec.None}
				: new TileInfo {ReturnValue = TileInfo.Flags.IsTileSpec, Spec = TileSpec.MapEdge};
		}

		public bool PlayerBehindSomething(Character _char)
		{
			if (_char != World.Instance.MainPlayer.ActiveCharacter)
				return false;

			return _playerShouldBeTransparent;
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

		public override void Update(GameTime gameTime)
		{
			World.Instance.ActiveCharacterRenderer.Update(gameTime);
			IEnumerable<EOCharacterRenderer> toAdd = otherRenderers.Where(rend => !Game.Components.Contains(rend));
			foreach (EOCharacterRenderer rend in toAdd)
				rend.Update(gameTime); //do update logic here: other renderers will NOT be added to Game's components

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
			_playerShouldBeTransparent = false;
			sb.Begin();
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			List<Character> otherChars = new List<Character>(otherPlayers); //copy of list

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
						Vector2 pos = _getDrawCoordinates(j, i, c);
						sb.Draw(GFXLoader.TextureFromResource(GFXTypes.MapTiles, MapRef.FillTile, true), new Vector2(pos.X - 1, pos.Y - 2), Color.White);
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

			//todo: blend overlapped region of ActiveCharacterRenderer with whatever map objects are going on (kind of transparent type of thing going on? yeah?)

			//TODO: cursor (follows mouse pointer)
			//sb.Draw(GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 24, true), );
			
			//overlay/mask  objects
			List<GFXRow> overlayObjRows = MapRef.GfxRows[2].Where(yGFXQuery).ToList();
			foreach (GFXRow row in overlayObjRows)
			{
				List<GFX> overlayObj = row.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in overlayObj)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapOverlay, obj.tile, true);
					Vector2 pos = _getDrawCoordinates(obj.x, row.y, c);
					pos = new Vector2(pos.X + 16, pos.Y - 11);
					_drawIfBehindSomething(otherChars, obj.x, row.y, gfx.Bounds.SetPosition(pos));
					sb.Draw(gfx, pos, Color.White);
				}
			}

			//walls - two layers: facing different directions
			//this layer faces to the right
			List<GFXRow> wallRows1 = MapRef.GfxRows[4].Where(yGFXQuery).ToList();
			foreach(GFXRow wallRow in wallRows1)
			{
				List<GFX> walls = wallRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in walls)
				{
					int gfxNum = obj.tile;
					if (_door != null && _door.x == obj.x && _doorY == wallRow.y && _door.doorOpened)
						gfxNum++;

					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
					Vector2 loc = _getDrawCoordinates(obj.x, wallRow.y, c);
					loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 47, loc.Y - (gfx.Height - 29));
					_drawIfBehindSomething(otherChars, obj.x, wallRow.y, gfx.Bounds.SetPosition(loc));
					sb.Draw(gfx, loc, Color.White);
				}
			}
			//this layer faces to the down
			List<GFXRow> wallRows2 = MapRef.GfxRows[3].Where(yGFXQuery).ToList();
			foreach (GFXRow wallRow in wallRows2)
			{
				List<GFX> walls = wallRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in walls)
				{
					int gfxNum = obj.tile;
					if (_door != null && _door.x == obj.x && _doorY == wallRow.y && _door.doorOpened)
						gfxNum++;

					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
					Vector2 loc = _getDrawCoordinates(obj.x, wallRow.y, c);
					loc = new Vector2(loc.X - (int) Math.Round(gfx.Width/2.0) + 15, loc.Y - (gfx.Height - 29));
					_drawIfBehindSomething(otherChars, obj.x, wallRow.y, gfx.Bounds.SetPosition(loc));
					sb.Draw(gfx, loc, Color.White);
				}
			}

			//shadows
			//TODO: Load configuration value determining whether or not to show shadows
			List<GFXRow> shadowRows = MapRef.GfxRows[7].Where(yGFXQuery).ToList();
			foreach (GFXRow shadowRow in shadowRows)
			{
				List<GFX> shadows = shadowRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX shadow in shadows)
				{
					Vector2 loc = _getDrawCoordinates(shadow.x, shadowRow.y, c);
					sb.Draw(GFXLoader.TextureFromResource(GFXTypes.Shadows, shadow.tile, true), new Vector2(loc.X - 24, loc.Y - 12), Color.FromNonPremultiplied(255, 255, 255, 60));
				}
			}
			
			//map objects
			List<GFXRow> mapObjs = MapRef.GfxRows[1].Where(yGFXQuery).ToList();
			foreach (GFXRow mapObjRow in mapObjs)
			{
				List<GFX> objs = mapObjRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in objs)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapObjects, obj.tile, true);
					Vector2 loc = _getDrawCoordinates(obj.x, mapObjRow.y, c);
					loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 29, loc.Y - (gfx.Height - 28));
					_drawIfBehindSomething(otherChars, obj.x, mapObjRow.y, gfx.Bounds.SetPosition(loc));
					sb.Draw(gfx, loc, Color.White);
				}
			}

			//roofs (after objects - for outdoor maps, which actually have roofs, this makes more sense)
			List<GFXRow> roofRows = MapRef.GfxRows[5].Where(yGFXQuery).ToList();
			foreach (GFXRow roofRow in roofRows)
			{
				List<GFX> roofs = roofRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX roof in roofs)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWallTop, roof.tile, true);
					Vector2 loc = _getDrawCoordinates(roof.x, roofRow.y, c);
					loc = new Vector2(loc.X - 2, loc.Y - 63);
					_drawIfBehindSomething(otherChars, roof.x, roofRow.y, gfx.Bounds.SetPosition(loc));
					sb.Draw(gfx, loc, Color.White);
				}
			}

			//overlay tiles (counters, etc)
			List<GFXRow> rows = MapRef.GfxRows[6].Where(yGFXQuery).ToList();
			foreach (GFXRow row in rows)
			{
				List<GFX> tiles = row.tiles.Where(xGFXQuery).ToList();
				foreach (GFX tile in tiles)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapTiles, tile.tile, true);
					Vector2 loc = _getDrawCoordinates(tile.x, row.y, c);
					loc = new Vector2(loc.X - 2, loc.Y - 31);
					_drawIfBehindSomething(otherChars, tile.x, row.y, gfx.Bounds.SetPosition(loc));
					sb.Draw(gfx,loc, Color.White);
				}
			}

			sb.End();

			//draw any remaining characters that weren't behind something
			if (otherChars.Count > 0)
			{
				List<Character> drawBefore = otherChars.Where(_c => _c.X < c.X || _c.Y < c.Y).ToList();
				otherChars.RemoveAll(drawBefore.Contains);
				otherRenderers.Where(_r => drawBefore.Contains(_r.Character)).ToList().ForEach(_r => _r.Draw(null)); //draw before main player
				World.Instance.ActiveCharacterRenderer.Draw(null); //draw main player
				otherRenderers.Where(_r => otherChars.Contains(_r.Character)).ToList().ForEach(_r => _r.Draw(null)); //draw after main player
			}
			else
				World.Instance.ActiveCharacterRenderer.Draw(null); //just draw main player
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
		private void _drawIfBehindSomething(List<Character> otherChars, int objX, int objY, Rectangle textureBounds)
		{

			List<Character> clipChars = otherChars.FindAll(_c => _c.X < objX && _c.Y < objY);
			List<EOCharacterRenderer> drawTime = otherRenderers.Where(_r => clipChars.Contains(_r.Character)).ToList();
			foreach (EOCharacterRenderer _r in drawTime)
			{
				if (_r.DrawAreaWithOffset.Intersects(textureBounds))
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
		}
	}
}
