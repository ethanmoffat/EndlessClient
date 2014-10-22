using System;
using System.Collections.Generic;
using System.Linq;
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

	public class EOMapRenderer : DrawableGameComponent
	{
		public List<MapItem> MapItems { get; set; }
		private readonly List<Character> otherPlayers = new List<Character>();
		public List<NPC> NPCs { get; set; }

		public MapFile MapRef { get; set; }
		
		private readonly SpriteBatch sb;

		private Rectangle _animSourceRect;

		public EOMapRenderer(EOGame g, MapFile mapObj)
			: base(g)
		{
			if(g == null)
				throw new NullReferenceException("The game must not be null");

			MapRef = mapObj;
			MapItems = new List<MapItem>();
			NPCs = new List<NPC>();

			sb = new SpriteBatch(Game.GraphicsDevice);

			_animSourceRect = new Rectangle(0, 0, 64, 32);

			Visible = true;
			g.Components.Add(this);
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
		}

		public void AddOtherPlayer(Character c, WarpAnimation anim = WarpAnimation.None)
		{
			if(otherPlayers.Find(x => x.Name == c.Name && x.ID == c.ID) == null)
				otherPlayers.Add(c);

			//TODO: Add whatever magic is necessary to make the player appear all pretty (with animation)
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
			Func<GFX, bool> xGFXQuery = gfx => gfx.x >= c.X - Constants.ViewLength && gfx.x <= c.X + Constants.ViewLength;
			Func<GFX, bool> xGFXQuery3 = gfx => gfx.x >= c.X - Constants.ViewLength && gfx.x <= c.X + Constants.ViewLength * 3;
			// Queries (func) for the gfxrow items within range of the character's Y coordinate
			Func<GFXRow, bool> yGFXQuery = row => row.y >= c.Y - Constants.ViewLength && row.y <= c.Y + Constants.ViewLength;
			Func<GFXRow, bool> yGFXQuery3 = row => row.y >= c.Y - Constants.ViewLength && row.y <= c.Y + Constants.ViewLength * 3;

			//render fill tile first
			if (MapRef.FillTile > 0)
			{
				//for (int i = c.Y - Constants.ViewLength; i < c.Y + Constants.ViewLength; ++i)
				for (int i = 1; i <= MapRef.Height; ++i)
				{
					//for (int j = c.X - Constants.ViewLength; j < c.X - Constants.ViewLength; ++j)
					for (int j = 1; j <= MapRef.Width; ++j)
					{
						//if (i > 0 && j > 0 && i <= MapRef.Height && j <= MapRef.Width)
						//{
							sb.Draw(GFXLoader.TextureFromResource(GFXTypes.MapTiles, MapRef.FillTile, true),_getDrawCoordinates(j, i, c), Color.White);
						//}
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
					if(nextTile.Width > 64)
						sb.Draw(nextTile, _getDrawCoordinates(tile.x, row.y, c), _animSourceRect, Color.White);
					else
						sb.Draw(nextTile, _getDrawCoordinates(tile.x, row.y, c), Color.White);
				}
			}

			//items next!
			List<MapItem> items = MapItems.Where(item => xGFXQuery(new GFX {x = item.x}) && yGFXQuery(new GFXRow {y = item.y})).ToList();
			foreach (MapItem item in items)
			{
				ItemRecord itemData = (ItemRecord)World.Instance.EIF.Data.Find(i => i is ItemRecord && (i as ItemRecord).ID == item.id);
				Vector2 itemPos = _getDrawCoordinates(item.x, item.y, c);
				if (itemData.Type == ItemType.Money)
				{
					int moneyOffset = item.amount >= 1000 ? 4 : (
						item.amount >= 100 ? 3 : (
						item.amount >= 10 ? 2 : (
						item.amount >= 3 ? 1 : 0)));

					Texture2D moneyMoneyMan = GFXLoader.TextureFromResource(GFXTypes.Items, 269 + (2*moneyOffset), true);
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

			//overlay objects
			List<GFXRow> overlayObjRows = MapRef.GfxRows[2].Where(yGFXQuery).ToList();
			foreach (GFXRow row in overlayObjRows)
			{
				List<GFX> overlayObj = row.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in overlayObj)
				{
					sb.Draw(GFXLoader.TextureFromResource(GFXTypes.MapOverlay, obj.tile, true), _getDrawCoordinates(obj.x, row.y, c), Color.White);
				}
			}

			//walls - two layers: facing different directions
			List<GFXRow> wallRows1 = MapRef.GfxRows[4].Where(yGFXQuery).ToList();
			foreach(GFXRow wallRow in wallRows1)
			{
				List<GFX> walls = wallRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in walls)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, obj.tile, true);
					Vector2 loc = _getDrawCoordinates(obj.x, wallRow.y, c); 
					//Ground.Draw2D(Resource.GetMapTexture(graphics, Resource.ResourceReader.Gfx006, Gfx.tile, true), 
					//		new Point(0, 0), 0.0f, 
					//		new Point((Gfx.x * 32) - (GfxRow.y * 32) + (288) - World.World.offset_x - Resource.GetBitmap(graphics, Resource.ResourceReader.Gfx006, Gfx.tile).Width / 2 + 46, 
					//				  (GfxRow.y * 16) + (Gfx.x * 16) + (144) - World.World.offset_y - (Resource.GetBitmap(graphics, Resource.ResourceReader.Gfx006, Gfx.tile).Height - 33)),
					//		Color.White);
					sb.Draw(gfx, new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 46, loc.Y - (gfx.Height - 33)), Color.White);
				}
			}
			List<GFXRow> wallRows2 = MapRef.GfxRows[3].Where(yGFXQuery).ToList();
			foreach (GFXRow wallRow in wallRows2)
			{
				List<GFX> walls = wallRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX obj in walls)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, obj.tile, true);
					Vector2 loc = _getDrawCoordinates(obj.x, wallRow.y, c);
					sb.Draw(gfx, new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 14, loc.Y - (gfx.Height - 33)), Color.White);
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

			//roofs
			List<GFXRow> roofRows = MapRef.GfxRows[5].Where(yGFXQuery).ToList();
			foreach (GFXRow roofRow in roofRows)
			{
				List<GFX> roofs = roofRow.tiles.Where(xGFXQuery).ToList();
				foreach (GFX roof in roofs)
				{
					Vector2 loc = _getDrawCoordinates(roof.x, roofRow.y, c);
					sb.Draw(GFXLoader.TextureFromResource(GFXTypes.MapWallTop, roof.tile, true), new Vector2(loc.X - 2, loc.Y - 63), Color.White);
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
					sb.Draw(gfx, new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 28, loc.Y - (gfx.Height - 29)), Color.White);
				}
			}
			
			//overlay tiles
			List<GFXRow> rows = MapRef.GfxRows[6].Where(yGFXQuery).ToList();
			foreach (GFXRow row in rows)
			{
				List<GFX> tiles = row.tiles.Where(xGFXQuery).ToList();
				foreach (GFX tile in tiles)
				{
					Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, tile.tile, true);
					Vector2 loc = _getDrawCoordinates(tile.x, row.y, c);
					sb.Draw(gfx, new Vector2(loc.X - 2, loc.Y - 31), Color.White);
				}
			}

			sb.End();
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
	}
}
