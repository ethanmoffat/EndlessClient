using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using EOLib;
using EOLib.Data;
using EOLib.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EndlessClient
{
	//returned from CheckCoordinates
	//convenience wrapper
	public struct TileInfo
	{
		public enum ReturnType //this struct is used sort of like a union - different data returned in different cases
		{
			IsTileSpec, //indicates that a normal tile spec is returned
			IsWarpSpec, //indicates that a normal warp spec is returned
			IsOtherPlayer, //other player is in the way, spec/warp are invalid
			IsOtherNPC, //other npc is in the way, spec/warp are invalid
			IsMapSign
		}

		public ReturnType ReturnValue;

		public TileSpec Spec;
		public Warp Warp;
		public NPC NPC;
		public MapSign Sign;
	}

	public class EOMapRenderer : DrawableGameComponent
	{
		private class WaterEffect
		{
			public static readonly Texture2D WaterTexture = GFXLoader.TextureFromResource(GFXTypes.Spells, EffectSprite.EFFECT_GFX_WATER_TILE, true);
			private static readonly int WidthDelta = WaterTexture.Width/EffectSprite.EFFECT_GFX_WATER_FRAMES;

			private DateTime LastUpdate;
			private int Frame;

			public Rectangle SourceRectangle { get; private set; }
			public int X { get; private set; }
			public int Y { get; private set; }

			public WaterEffect(int x, int y)
			{
				X = x;
				Y = y;
				LastUpdate = DateTime.Now;
				
				Frame = 0;
				SourceRectangle = new Rectangle(0, 0, WidthDelta, WaterTexture.Height);
			}

			public void IncrementFrameIfNeeded()
			{
				if ((DateTime.Now - LastUpdate).TotalMilliseconds >= 100)
				{
					Frame++;
					if (Frame >= EffectSprite.EFFECT_GFX_WATER_FRAMES)
					{
						return;
					}

					SourceRectangle = new Rectangle(Frame * WidthDelta, 0, WidthDelta, WaterTexture.Height);
					LastUpdate = DateTime.Now;
				}
			}

			public bool DoneAnimating()
			{
				return Frame >= EffectSprite.EFFECT_GFX_WATER_FRAMES;
			}

			public void ResetFrameCounter()
			{
				Frame = 0;
			}
		}

		//collections
		private readonly Dictionary<Point, List<MapItem>> MapItems = new Dictionary<Point, List<MapItem>>();
		private readonly List<Character> otherPlayers = new List<Character>();
		private readonly List<EOCharacterRenderer> otherRenderers = new List<EOCharacterRenderer>();
		private readonly List<NPC> npcList = new List<NPC>();
		private static readonly object npcListLock = new object();

		public MapFile MapRef { get; private set; }
		private bool m_needDispMapName;
		
		//cursor members
		private Vector2 cursorPos;
		private int gridX, gridY;
		private readonly Texture2D mouseCursor, statusIcons;
		private Rectangle _cursorSourceRect;
		private readonly XNALabel _mouseoverName;
		private MouseState _prevState;
		private bool _hideCursor;
		//public cursor members
		public bool MouseOver
		{
			get
			{
				MouseState ms = Mouse.GetState();
				return EOGame.Instance.IsActive && ms.X > 0 && ms.Y > 0 && ms.X < 640 && ms.Y < 320;
			}
		}
		public Point GridCoords
		{
			get { return new Point(gridX, gridY); }
		}

		//rendering members
		private RenderTarget2D _rtMapObjAbovePlayer, _rtMapObjBelowPlayer;
		private BlendState _playerBlend;
		private SpriteBatch sb;

		private DateTime? m_mapLoadTime;
		private int m_transitionMetric;

		//animated tile/wall members
		private Vector2 _tileSrc;
		private int _wallSrcIndex;
		private TimeSpan? lastAnimUpdate;
		private readonly Dictionary<Point, WaterEffect> _waterTiles = new Dictionary<Point,WaterEffect>();

		//door members
		private readonly Timer _doorTimer;
		private Warp _door;
		private byte _doorY; //since y-coord not stored in Warp object...

		private ManualResetEventSlim m_drawingEvent;
		private EOMapContextMenu m_contextMenu;

		private readonly PacketAPI m_api;

		private MiniMapRenderer m_miniMapRenderer;

		public EOMapRenderer(Game g, PacketAPI apiHandle)
			: base(g)
		{
			if(g == null)
				throw new NullReferenceException("The game must not be null");
			if(!(g is EOGame))
				throw new ArgumentException("The game must be an EOGame instance");
			if(apiHandle == null || !apiHandle.Initialized)
				throw new ArgumentException("Invalid PacketAPI object");
			m_api = apiHandle;

			sb = new SpriteBatch(Game.GraphicsDevice);

			mouseCursor = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 24, true);
			statusIcons = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 46, true);
			_cursorSourceRect = new Rectangle(0, 0, mouseCursor.Width / 5, mouseCursor.Height);
			_mouseoverName = new XNALabel(new Rectangle(1, 1, 1, 1), "Microsoft Sans Serif", 8.75f)
			{
				Visible = true,
				Text = "",
				ForeColor = System.Drawing.Color.White,
				DrawOrder = (int)ControlDrawLayer.BaseLayer + 3,
				AutoSize = false
			};

			m_drawingEvent = new ManualResetEventSlim(true);
			Visible = false;

			_doorTimer = new Timer(_doorTimerCallback);
		}

		#region /* PUBLIC INTERFACE -- CHAT + MAP RELATED */
		public void RenderChatMessage(TalkType messageType, int playerID, string message, ChatType chatType = ChatType.None)
		{
			//convert the messageType into a valid ChatTab to pass everything on to
			ChatTabs tab;
			switch (messageType)
			{
				case TalkType.NPC:
				case TalkType.Local: tab = ChatTabs.Local; break;
				case TalkType.Party: tab = ChatTabs.Group; break;
				default: throw new NotImplementedException();
			}

			DrawableGameComponent dgc;
			string playerName = null;
			if (messageType == TalkType.NPC)
			{
				lock(npcListLock)
					dgc = npcList.Find(_npc => _npc.Index == playerID);
				if (dgc != null)
					playerName = ((NPC) dgc).Data.Name;
			}
			else
			{
				dgc = otherRenderers.Find(_rend => _rend.Character.ID == playerID);
				if (dgc != null)
					playerName = ((EOCharacterRenderer)dgc).Character.Name;
			}

			if (playerName == null) return;

			if(playerName.Length > 1)
				playerName = char.ToUpper(playerName[0]) + playerName.Substring(1);

			if (EOGame.Instance.Hud == null)
				return;

			message = EOChatRenderer.Filter(message, false);

			if (message != null)
			{
				EOGame.Instance.Hud.AddChat(tab, playerName, message, chatType);
				if (messageType == TalkType.Party)
				{
					//party chat also adds to local with the PM color
					EOGame.Instance.Hud.AddChat(ChatTabs.Local, playerName, message, chatType, ChatColor.PM);
				}
				MakeSpeechBubble(dgc, message, messageType == TalkType.Party);
			}
		}

		public void MakeSpeechBubble(DrawableGameComponent follow, string message, bool groupChat)
		{
			if (!World.Instance.ShowChatBubbles)
				return;

			if (follow == null)
				follow = World.Instance.ActiveCharacterRenderer; /* Calling with null assumes Active Character */

			//show just the speech bubble, since this should be called from the HUD and rendered there already

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
			if (follow is EOCharacterRenderer)
				((EOCharacterRenderer)follow).SetChatBubbleText(message, groupChat);
			else if (follow is NPC)
				((NPC)follow).SetChatBubbleText(message, groupChat);
// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
		}

		public void SetActiveMap(MapFile newActiveMap)
		{
			if(newActiveMap == null)
				throw new ArgumentNullException("newActiveMap", "The active map may not be null!");

			m_drawingEvent.Wait();
			m_drawingEvent.Reset();

			if(MapRef != null && MapRef.AmbientNoise != 0)
				EOGame.Instance.SoundManager.StopLoopingSoundEffect(MapRef.AmbientNoise);

			MapRef = newActiveMap;

			if (m_miniMapRenderer == null)
				m_miniMapRenderer = new MiniMapRenderer(MapRef, sb, this);
			else
				m_miniMapRenderer.Map = MapRef;

			MapItems.Clear();
			otherRenderers.ForEach(_rend => _rend.Dispose());
			otherRenderers.Clear();
			otherPlayers.Clear();
			lock (npcListLock)
			{
				npcList.ForEach(_npc => _npc.Dispose());
				npcList.Clear();
			}

			//need to reset door-related members when changing maps.
			if (_door != null)
			{
				_door.doorOpened = false;
				_door.backOff = false;
				_door = null;
				_doorY = 0;
				_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}

			m_mapLoadTime = DateTime.Now;
			m_transitionMetric = 1;
			if (!MapRef.MapAvailable)
				m_miniMapRenderer.Visible = false;

			if (MapRef.Name.Length > 0)
			{
				if (EOGame.Instance.Hud != null)
					EOGame.Instance.Hud.AddChat(ChatTabs.System, "", World.GetString(DATCONST2.STATUS_LABEL_YOU_ENTERED) + " " + MapRef.Name, ChatType.NoteLeftArrow);
				else
					m_needDispMapName = true;
			}

			PlayOrStopBackgroundMusic();
			PlayOrStopAmbientNoise();

			m_drawingEvent.Set();
		}

		public TileInfo CheckCoordinates(byte destX, byte destY)
		{
			lock (npcListLock)
			{
				NPC npc;
				if ((npc = npcList.Find(_npc => _npc.X == destX && _npc.Y == destY)) != null && !npc.Dying)
				{
					return new TileInfo { ReturnValue = TileInfo.ReturnType.IsOtherNPC, NPC = npc };
				}
			}

			if (otherPlayers.Any(player => player.X == destX && player.Y == destY))
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsOtherPlayer };
			}

			Warp warp = MapRef.WarpLookup[destY, destX];
			if (warp != null)
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsWarpSpec, Warp = warp };
			}

			MapSign sign = MapRef.Signs.Find(_ms => _ms.x == destX && _ms.y == destY);
			if (sign.x == destX && sign.y == destY)
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsMapSign, Sign = sign };
			}

			Tile tile = MapRef.TileLookup[destY, destX];
			if (tile != null)
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = tile.spec };
			}

			return destX <= MapRef.Width && destY <= MapRef.Height //don't need to check zero bounds: because byte type is always positive (unsigned)
				? new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = TileSpec.None }
				: new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = TileSpec.MapEdge };
		}

		public void ToggleMapView()
		{
			if(MapRef.MapAvailable)
				m_miniMapRenderer.Visible = !m_miniMapRenderer.Visible;
			else
				EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_NO_MAP_OF_AREA);
		}

		public void AddMapItem(MapItem newItem)
		{
			if (newItem.npcDrop && newItem.id > 0)
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(newItem.id);
				EOGame.Instance.Hud.AddChat(ChatTabs.System, "",
					string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_THE_NPC_DROPPED), newItem.amount, rec.Name),
					ChatType.DownArrow);
			}

			Point key = new Point(newItem.x, newItem.y);
			if(!MapItems.ContainsKey(key))
				MapItems.Add(key, new List<MapItem>());

			int index = MapItems[key].FindIndex(_mi => _mi.uid == newItem.uid);
			if (index < 0)
				MapItems[key].Add(newItem);
		}

		public void RemoveMapItem(short uid)
		{
			List<Point> pts = MapItems.Keys.Where(_key => MapItems[_key].Find(_mi => _mi.uid == uid).uid == uid).ToList();
			if(pts.Count > 1)
				throw new AmbiguousMatchException("Multiple MapItems shared that uid. Something is wrong.");
			//should only be one result
			
			List<MapItem> res = MapItems[pts[0]];
			for (int i = res.Count - 1; i >= 0; --i)
			{
				if (res[i].uid == uid)
				{
					RemoveMapItem(res[i]);
					break;
				}
			}
		}

		private void RemoveMapItem(MapItem oldItem)
		{
			Point key = new Point(oldItem.x, oldItem.y);
			if (!MapItems.ContainsKey(key))
				return;
			MapItems[key].Remove(oldItem);
			if (MapItems[key].Count == 0)
				MapItems.Remove(key);
		}

		public void UpdateMapItemAmount(short uid, int amountTaken)
		{
			List<Point> pts = MapItems.Keys.Where(_key => MapItems[_key].Find(_mi => _mi.uid == uid).uid == uid).ToList();
			if(pts.Count > 1)
				throw new AmbiguousMatchException("Multiple MapItems shared that uid. Something is wrong.");

			List<MapItem> res = MapItems[pts[0]];
			MapItem toRemove = res.Find(_mi => _mi.uid == uid);
			res.Remove(toRemove);
			toRemove = new MapItem
			{
				amount = toRemove.amount - amountTaken,
				id = toRemove.id,
				npcDrop = toRemove.npcDrop,
				playerID = toRemove.playerID,
				time = toRemove.time,
				uid = toRemove.uid,
				x = toRemove.x,
				y = toRemove.y
			};
			//still some left. add it back.
			if(toRemove.amount > 0)
				res.Add(toRemove);
		}

		public void ClearMapItems()
		{
			MapItems.Clear();
		}

		public void PlayOrStopBackgroundMusic()
		{
			if (!World.Instance.MusicEnabled)
			{
				EOGame.Instance.SoundManager.StopBackgroundMusic();
				return;
			}

			//not sure what MusicExtra field is supposed to be for
			if (MapRef.Music > 0)
			{
				//sound manager accounts for zero-based indices when playing music
				EOGame.Instance.SoundManager.PlayBackgroundMusic(MapRef.Music);
			}
			else
			{
				EOGame.Instance.SoundManager.StopBackgroundMusic();
			}
		}

		public void PlayOrStopAmbientNoise()
		{
			if (!World.Instance.SoundEnabled)
			{
				if(MapRef.AmbientNoise > 0)
					EOGame.Instance.SoundManager.StopLoopingSoundEffect(MapRef.AmbientNoise);
				return;
			}

			if(MapRef.AmbientNoise > 0)
				EOGame.Instance.SoundManager.PlayLoopingSoundEffect(MapRef.AmbientNoise);
		}

		#endregion

		#region /* PUBLIC INTERFACE -- OTHER PLAYERS */
		public void AddOtherPlayer(CharacterData c, WarpAnimation anim = WarpAnimation.None)
		{
			Character other;
			EOCharacterRenderer otherRend = null;
			if ((other = otherPlayers.Find(x => x.Name == c.Name && x.ID == c.ID)) == null)
			{
				otherPlayers.Add(other = new Character(m_api, c));
				otherRenderers.Add(otherRend = new EOCharacterRenderer(other));
				otherRenderers[otherRenderers.Count - 1].Visible = true;
				otherRenderers[otherRenderers.Count - 1].Initialize();
				other.RenderData.SetUpdate(true);
			}
			else
			{
				other.ApplyData(c);
			}

			if (anim == WarpAnimation.Admin && otherRend != null)
			{
				//otherRend.ShowEffect(12);
			}
		}

		public void RemoveOtherPlayer(short id, WarpAnimation anim = WarpAnimation.None)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == id)) != null)
			{
				otherPlayers.Remove(c);
				int ndx = otherRenderers.FindIndex(rend => rend.Character == c);
				otherRenderers[ndx].HideChatBubble();
				otherRenderers.RemoveAt(ndx);
			}

			//TODO: Add warp animation when valid
		}

		public void ClearOtherPlayers()
		{
			otherRenderers.ForEach(_rend => _rend.HideChatBubble());
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
			EOCharacterRenderer rend = otherRenderers.Find(_rend => _rend.Character.ID == ID);
			if (rend != null)
			{
				rend.Character.Walk(direction, x, y, false);

				TileInfo ti = CheckCoordinates(rend.Character.DestX, rend.Character.DestY);
				rend.PlayerWalk(ti.ReturnValue == TileInfo.ReturnType.IsTileSpec && ti.Spec == TileSpec.Water);
			}
		}

		public void OtherPlayerAttack(short ID, EODirection direction)
		{
			EOCharacterRenderer rend = otherRenderers.Find(_rend => _rend.Character.ID == ID);
			if (rend != null)
			{
				rend.Character.Attack(direction);

				TileInfo info = CheckCoordinates((byte) rend.Character.X, (byte) rend.Character.Y);
				rend.PlayerAttack(info.ReturnValue == TileInfo.ReturnType.IsTileSpec && info.Spec == TileSpec.Water);
			}
		}

		public void OtherPlayerEmote(short playerID, Emote emote)
		{
			EOCharacterRenderer rend = otherRenderers.Find(cc => cc.Character.ID == playerID);
			if (rend != null)
			{
				rend.Character.Emote(emote);
				rend.PlayerEmote();
			}
		}

		public void OtherPlayerHide(short ID, bool hidden)
		{
			Character c = otherPlayers.Find(_char => _char.ID == ID);
			if (c != null)
			{
				c.RenderData.SetHidden(hidden);
			}
		}

		public void OtherPlayerHeal(short ID, int healAmount, int pctHealth)
		{
			EOCharacterRenderer rend = ID == World.Instance.MainPlayer.ActiveCharacter.ID ? World.Instance.ActiveCharacterRenderer :
				otherRenderers.Find(_rend => _rend.Character.ID == ID);

			if (rend == null) return; //couldn't find other player :(

			if (healAmount > 0)
			{
				rend.Character.Stats.SetHP((short)Math.Max(rend.Character.Stats.hp + healAmount, rend.Character.Stats.maxhp));
				if (rend.Character == World.Instance.MainPlayer.ActiveCharacter)
				{
					//update health in UI
					EOGame.Instance.Hud.RefreshStats();
				}
				rend.SetDamageCounterValue(healAmount, pctHealth, true);
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
				c.SetDisplayItemsFromRenderData(newRenderData);
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

		public Character GetOtherPlayer(short playerId)
		{
			return otherPlayers.Find(_c => _c.ID == playerId);
		}

		//shows the water splashies at the 'x, y' coordinates specified
		public void NewWaterEffect(byte x, byte y)
		{
			Point pt = new Point(x, y);
			if (_waterTiles.ContainsKey(pt))
				_waterTiles[pt].ResetFrameCounter();
			else
				_waterTiles.Add(pt, new WaterEffect(x, y));
		}
		#endregion

		#region/* PUBLIC INTERFACE -- OTHER NPCS */

		public NPC GetNPCAt(int x, int y)
		{
			return npcList.Find(_npc => _npc.X == x && _npc.Y == y);
		}

		public void AddOtherNPC(NPCData data)
		{
			lock (npcListLock)
			{
				NPC exists;
				if ((exists = npcList.Find(_npc => _npc.Index == data.Index)) == null)
				{
					exists = new NPC(data);
					exists.Initialize();
					npcList.Add(exists);
				}
				else
				{
					exists.ApplyData(data);
				}
				
				exists.Visible = true;
			}
		}

		public void RemoveOtherNPC(byte index, int damage = 0)
		{
			lock (npcListLock)
			{
				NPC npc = npcList.Find(_npc => _npc.Index == index);
				if (npc != null)
				{
					if (damage > 0) //npc was killed
					{
						npc.HP = Math.Max(npc.HP - damage, 0);
						npc.FadeAway();
						npc.Opponent = null;
						npc.SetDamageCounterValue(damage, 0);
					}
					else //npc is out of view or done fading away
					{
						npc.Visible = false;
						npc.HideChatBubble();
					}
				}
			}
		}

		public void ClearOtherNPCs()
		{
			lock (npcListLock)
			{
				foreach (NPC n in npcList)
				{
					n.Visible = false;
					n.Dispose();
				}
				npcList.Clear();
			}
		}

		public void NPCWalk(byte index, byte x, byte y, EODirection dir)
		{
			lock (npcListLock)
			{
				NPC toWalk = npcList.Find(_npc => _npc.Index == index);
				if (toWalk != null && !toWalk.Walking)
				{
					toWalk.Walk(x, y, dir);
				}
			}
		}

		public void NPCAttack(byte index, bool isTargetPlayerDead, EODirection dir, short targetPlayerId, int damageToPlayer, int playerPctHealth)
		{
			lock (npcListLock)
			{
				NPC toAttack = npcList.Find(_npc => _npc.Index == index);
				if (toAttack != null && !toAttack.Attacking)
				{
					toAttack.Attack(dir);
				}
			}

			EOCharacterRenderer rend = targetPlayerId == World.Instance.MainPlayer.ActiveCharacter.ID ? World.Instance.ActiveCharacterRenderer : 
				otherRenderers.Find(_rend => _rend.Character.ID == targetPlayerId);

			if (rend == null) return; //couldn't find other player :(

			rend.Character.Stats.SetHP((short)Math.Max(rend.Character.Stats.hp - damageToPlayer, 0));
			if (rend.Character == World.Instance.MainPlayer.ActiveCharacter)
			{
				//update health in UI
				EOGame.Instance.Hud.RefreshStats();
			}
			rend.SetDamageCounterValue(damageToPlayer, playerPctHealth);

			if (isTargetPlayerDead)
				rend.Die();
		}

		public void NPCTakeDamage(short npcIndex, short fromPlayerID, EODirection fromDirection, int damageToNPC, int npcPctHealth)
		{
			lock (npcListLock)
			{
				NPC toDamage = npcList.Find(_npc => _npc.Index == npcIndex);
				if (toDamage == null) return;

				toDamage.SetDamageCounterValue(damageToNPC, npcPctHealth);
				toDamage.HP -= damageToNPC;

				EOCharacterRenderer rend = fromPlayerID == World.Instance.MainPlayer.ActiveCharacter.ID
					? World.Instance.ActiveCharacterRenderer
					: otherRenderers.Find(_rend => _rend.Character.ID == fromPlayerID);

				if (rend == null) return;

				toDamage.Opponent = rend.Character; //for fighting protection, no KSing!

				if (rend.Character.RenderData.facing != fromDirection)
					rend.Character.RenderData.SetDirection(fromDirection);
			}
		}
		#endregion

		#region /* PUBLIC INTERFACE -- DOORS */

		/// <summary>
		/// Sends the initial DOOR_OPEN packet to the server for a certain Warp. Sets the warpRef.backOff = true
		/// </summary>
		public void StartOpenDoor(Warp warpRef, byte x, byte y)
		{
			warpRef.backOff = true; //set flag to prevent hella door packets from the client
			if(!m_api.DoorOpen(x, y))
				((EOGame)Game).LostConnectionDialog();
		}

		/// <summary>
		/// Handles the opening of the door (event handler for when the DOOR_OPEN packet is received)
		/// </summary>
		public void OnDoorOpened(byte x, byte y)
		{
			if (_door != null && _door.doorOpened)
			{
				_door.doorOpened = false;
				_door.backOff = false;
				_doorY = 0;
			}

			if ((_door = MapRef.WarpLookup[y, x]) != null)
			{
				((EOGame) Game).SoundManager.GetSoundEffectRef(SoundEffectID.DoorOpen).Play();
				_door.doorOpened = true;
				_doorY = y;
				_doorTimer.Change(3000, 0);
			}
		}

		private void _doorTimerCallback(object state)
		{
			if (_door == null)
			{
				_doorY = 0;
				return;
			}

			if (_door.doorOpened)
				((EOGame) Game).SoundManager.GetSoundEffectRef(SoundEffectID.DoorClose).Play();

			_door.doorOpened = false;
			_door.backOff = false; //back-off from sending a door packet.
			_doorY = 0;
			_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}
#endregion

		/* GAME COMPONENT DERIVED METHODS */
		public override void Initialize()
		{
			_rtMapObjAbovePlayer = new RenderTarget2D(Game.GraphicsDevice, 
				Game.GraphicsDevice.PresentationParameters.BackBufferWidth, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None);

			_rtMapObjBelowPlayer = new RenderTarget2D(Game.GraphicsDevice,
				Game.GraphicsDevice.PresentationParameters.BackBufferWidth, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None);

			_playerBlend = new BlendState
			{
				BlendFactor = new Color(255, 255, 255, 64),

				AlphaSourceBlend = Blend.One,
				AlphaDestinationBlend = Blend.One,
				AlphaBlendFunction = BlendFunction.Add,

				ColorSourceBlend = Blend.BlendFactor,
				ColorDestinationBlend = Blend.One
			};

			m_contextMenu = new EOMapContextMenu(m_api);

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			//***update for all objects on map
			World.Instance.ActiveCharacterRenderer.Update(gameTime);
			IEnumerable<EOCharacterRenderer> toAdd = otherRenderers.Where(rend => !Game.Components.Contains(rend));
			foreach (EOCharacterRenderer rend in toAdd)
				rend.Update(gameTime); //do update logic here: other renderers will NOT be added to Game's components

			lock(npcListLock)
				npcList.Where(_npc => !Game.Components.Contains(_npc)).ToList().ForEach(_n => _n.Update(gameTime));

			//***do the map animations
			//lazy init
			if (lastAnimUpdate == null) lastAnimUpdate = gameTime.TotalGameTime;
			if ((gameTime.TotalGameTime - lastAnimUpdate.Value).TotalMilliseconds > 500)
			{
				_wallSrcIndex++;
				if(_wallSrcIndex == 4) _wallSrcIndex = 0;

				_tileSrc = new Vector2(64 + _tileSrc.X, 0);
				if (_tileSrc.X > 192)
					_tileSrc = Vector2.Zero;

				lastAnimUpdate = gameTime.TotalGameTime;
			}

			for (int i = _waterTiles.Values.Count - 1; i >= 0; --i)
			{
				WaterEffect eff = _waterTiles.Values.ElementAt(i);
				if (eff.DoneAnimating())
					_waterTiles.Remove(new Point(eff.X, eff.Y));
				else
					eff.IncrementFrameIfNeeded();
			}

			//***do the cursor stuff
			MouseState ms = Mouse.GetState();
			if (MouseOver) //checks bounds for map rendering area
			{
				_updateCursorInfo(ms);
			}

			if (m_needDispMapName && EOGame.Instance.Hud != null)
			{
				m_needDispMapName = false;
				EOGame.Instance.Hud.AddChat(ChatTabs.System, "", World.GetString(DATCONST2.STATUS_LABEL_YOU_ENTERED) + " " + MapRef.Name, ChatType.NoteLeftArrow);
			}
			
			//draw stuff to the render target
			m_drawingEvent.Wait(); //need to make sure that the map isn't being changed during a draw!
			m_drawingEvent.Reset();
			_drawMapObjectsAndActors();
			m_drawingEvent.Set();

			_prevState = ms;
			base.Update(gameTime);
		}

		private void _updateCursorInfo(MouseState ms)
		{
			//don't do the cursor if there is a dialog open or the mouse is over the context menu
			if (EOPaperdollDialog.Instance != null || EOChestDialog.Instance != null ||
				EOLockerDialog.Instance != null || EOBankAccountDialog.Instance != null ||
				(m_contextMenu.Visible && m_contextMenu.MouseOver))
				return;

			//need to solve this system of equations to get x, y on the grid
			//(x * 32) - (y * 32) + 288 - c.OffsetX, => pixX = 32x - 32y + 288 - c.OffsetX
			//(y * 16) + (x * 16) + 144 - c.OffsetY  => 2pixY = 32y + 32x + 288 - 2c.OffsetY
			//										 => 2pixY + pixX = 64x + 576 - c.OffsetX - 2c.OffsetY
			//										 => 2pixY + pixX - 576 + c.OffsetX + 2c.OffsetY = 64x
			//										 => gridX = (pixX + 2pixY - 576 + c.OffsetX + 2c.OffsetY) / 64; <=
			//pixY = (gridX * 16) + (gridY * 16) + 144 - c.OffsetY =>
			//(pixY - (gridX * 16) - 144 + c.OffsetY) / 16 = gridY

			Character c = World.Instance.MainPlayer.ActiveCharacter;
			//center the cursor on the mouse pointer
			int msX = ms.X - _cursorSourceRect.Width / 2;
			int msY = ms.Y - _cursorSourceRect.Height / 2;
			/*align cursor to grid based on mouse position*/
			gridX = (int)Math.Round((msX + 2 * msY - 576 + c.OffsetX + 2 * c.OffsetY) / 64.0);
			gridY = (int)Math.Round((msY - gridX * 16 - 144 + c.OffsetY) / 16.0);
			cursorPos = _getDrawCoordinates(gridX, gridY, c);
			if (gridX >= 0 && gridX <= MapRef.Width && gridY >= 0 && gridY <= MapRef.Height)
			{
				bool mouseClicked = ms.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed;
				bool rightClicked = ms.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed;
				
				//don't handle mouse clicks for map if there is a dialog being shown
				mouseClicked = mouseClicked && XNAControl.Dialogs.Count == 0;
				rightClicked = rightClicked && XNAControl.Dialogs.Count == 0;

				TileInfo ti = CheckCoordinates((byte) gridX, (byte) gridY);
				switch (ti.ReturnValue)
				{
					case TileInfo.ReturnType.IsOtherNPC:
						_cursorSourceRect.Location = new Point(mouseCursor.Width/5, 0);
						NPC npc;
						lock (npcListLock)
							if ((npc = npcList.Find(_npc => _npc.X == gridX && _npc.Y == gridY)) == null)
								break;
						_mouseoverName.Visible = true;
						_mouseoverName.Text = npc.Data.Name;
						_mouseoverName.ResizeBasedOnText();
						_mouseoverName.ForeColor = System.Drawing.Color.White;
						_mouseoverName.DrawLocation = new Vector2(
							npc.DrawArea.X + (npc.DrawArea.Width - _mouseoverName.ActualWidth) /2f,
							npc.DrawArea.Y + npc.TopPixel - _mouseoverName.Texture.Height - 4);

						if (mouseClicked)
						{
							switch (npc.Data.Type)
							{
								case NPCType.Shop:
									EOShopDialog.Show(m_api, npc);
									break;
								case NPCType.Inn:
									break;
								case NPCType.Bank:
									EOBankAccountDialog.Show(m_api, npc.Index);
									break;
								case NPCType.Barber:
									break;
								case NPCType.Guild:
									break;
								case NPCType.Priest:
									break;
								case NPCType.Law:
									break;
								case NPCType.Skills:
									EOSkillmasterDialog.Show(m_api, npc.Index);
									break;
								case NPCType.Quest:
									break;
							}
						}

						break;
					case TileInfo.ReturnType.IsOtherPlayer:
						//todo: this needs to be moved into some mouseover shit for character renderer
						//note: right-click and mouseover applies when mouse is over a character and that character
						//	is front-most in a given pile of characters that the mouse could be over

						_cursorSourceRect.Location = new Point(mouseCursor.Width/5, 0);
						EOCharacterRenderer _rend;
						_mouseoverName.Visible = true;
						_mouseoverName.Text =
							(_rend =
								otherRenderers.Find(_p => _p.Character.X == gridX && _p.Character.Y == gridY) ??
								World.Instance.ActiveCharacterRenderer).Character.Name;
						_mouseoverName.ResizeBasedOnText();
						_mouseoverName.ForeColor = System.Drawing.Color.White;
						_mouseoverName.DrawLocation = new Vector2(
							_rend.DrawAreaWithOffset.X + (32 - _mouseoverName.ActualWidth)/2f,
							_rend.DrawAreaWithOffset.Y + _rend.TopPixel - _mouseoverName.Texture.Height - 7);

						//handle right-clicking a player. menu when not ActiveCharacter, paperdoll when ActiveCharacter
						if (rightClicked)
						{
							if (_rend == World.Instance.ActiveCharacterRenderer)
								m_api.RequestPaperdoll((short) _rend.Character.ID);
							else
							{
								//show the right-click menu
								//make it a member instance since there is only one ever
								m_contextMenu.SetCharacterRenderer(_rend);
							}
						}

						break;
					default:
						if (gridX == c.X && gridY == c.Y)
							goto case TileInfo.ReturnType.IsOtherPlayer; //same logic if it's the active character

						_hideCursor = false;
						if (ti.ReturnValue == TileInfo.ReturnType.IsTileSpec)
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
									_cursorSourceRect.Location = new Point(mouseCursor.Width / 5, 0);
									if (mouseClicked && Math.Max(c.X - gridX, c.Y - gridY) <= 1 && (gridX == c.X || gridY == c.Y)) //must be directly adjacent
									{
										MapChest chest = MapRef.Chests.Find(_mc => _mc.x == gridX && _mc.y == gridY);
										if (chest == null) break;

										string requiredKey = null;
										switch (World.Instance.MainPlayer.ActiveCharacter.CanOpenChest(chest))
										{
											case ChestKey.Normal: requiredKey = "Normal Key"; break;
											case ChestKey.Silver: requiredKey = "Silver Key"; break;
											case ChestKey.Crystal: requiredKey = "Crystal Key"; break;
											case ChestKey.Wraith: requiredKey = "Wraith Key"; break;
											default:
												EOChestDialog.Show(m_api, chest.x, chest.y);
												break;
										}

										if (requiredKey != null)
										{
											EODialog.Show(DATCONST1.CHEST_LOCKED, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
											((EOGame)Game).Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION,
												" - " + requiredKey);
										}
									}
									break;
								case TileSpec.BankVault:
									_cursorSourceRect.Location = new Point(mouseCursor.Width / 5, 0);
									if (mouseClicked && Math.Max(c.X - gridX, c.Y - gridY) <= 1 && (gridX == c.X || gridY == c.Y))
									{
										EOLockerDialog.Show(m_api, (byte)gridX, (byte)gridY);
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
									_cursorSourceRect.Location = new Point(mouseCursor.Width/5, 0);
									break;
								case TileSpec.Jump:
								case TileSpec.Water:
								case TileSpec.Arena:
								case TileSpec.AmbientSource:
								case TileSpec.Spikes:
								case TileSpec.SpikesTrap:
								case TileSpec.SpikesTimed:
								case TileSpec.None:
									//normal cursor
									_cursorSourceRect.Location = new Point(0, 0);
									break;
							}
						}
						else if (ti.ReturnValue == TileInfo.ReturnType.IsMapSign)
						{
							_hideCursor = true;
							if (mouseClicked)
							{
								EODialog.Show(ti.Sign.message, ti.Sign.title, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
							}
						}
						else
							_cursorSourceRect.Location = new Point(0, 0);
						_mouseoverName.Text = "";
						break;
				}

				Point p;
				if (MapItems.ContainsKey(p = new Point(gridX, gridY)) && MapItems[p].Count > 0) //todo: conditional showing of mapitem text if character/npc on top of mapitem
				{
					MapItem mi = MapItems[p].Last(); //topmost item has label
					_cursorSourceRect.Location = new Point(2 * (mouseCursor.Width / 5), 0);
					_mouseoverName.Visible = true;
					_mouseoverName.Text = EOInventoryItem.GetNameString(mi.id, mi.amount);
					_mouseoverName.ResizeBasedOnText();
					_mouseoverName.ForeColor = EOInventoryItem.GetItemTextColor(mi.id);
					_mouseoverName.DrawLocation = new Vector2(
						cursorPos.X + 32 - _mouseoverName.ActualWidth/2f,
						cursorPos.Y - _mouseoverName.Texture.Height - 4);

					if (_prevState.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released)
					{
						if ((World.Instance.MainPlayer.ActiveCharacter.ID != mi.playerID && mi.playerID != 0) &&
							(mi.npcDrop && (DateTime.Now - mi.time).TotalSeconds <= World.Instance.NPCDropProtectTime) ||
							(!mi.npcDrop && (DateTime.Now - mi.time).TotalSeconds <= World.Instance.PlayerDropProtectTime))
						{
							Character charRef = otherPlayers.Find(_c => _c.ID == mi.id);
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
								EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
							}
							else if (c.Weight + (World.Instance.EIF.GetItemRecordByID(mi.id).Weight * mi.amount) > c.MaxWeight)
							{
								EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.DIALOG_ITS_TOO_HEAVY_WEIGHT);
							}
							else if (!m_api.GetItem(mi.uid)) //server validates anyway
								EOGame.Instance.LostConnectionDialog();
						}
					}
				}

				if (_mouseoverName.Text.Length > 0 && !Game.Components.Contains(_mouseoverName))
					Game.Components.Add(_mouseoverName);
			}
		}
		
		public override void Draw(GameTime gameTime)
		{
			if (MapRef != null)
			{
				m_drawingEvent.Wait();
				m_drawingEvent.Reset();

				_drawGroundLayer();
				if(MapItems.Count > 0)
					_drawMapItems();

				if (m_mapLoadTime != null && (DateTime.Now - m_mapLoadTime.Value).TotalMilliseconds > 2000)
					m_mapLoadTime = null;

				sb.Begin();

				_drawCursor();

				/*_drawPlayersNPCsAndMapObjects()*/
				sb.Draw(_rtMapObjAbovePlayer, Vector2.Zero, Color.White);
				sb.Draw(_rtMapObjBelowPlayer, Vector2.Zero, Color.White);
#if DEBUG
				sb.DrawString(EOGame.Instance.DBGFont, string.Format("FPS: {0}", World.FPS), new Vector2(30, 30), Color.White);
#endif
				_drawPlayerEquipIcons();

				sb.End();

				if (m_miniMapRenderer.Visible)
					m_miniMapRenderer.Draw(gameTime);

				m_drawingEvent.Set();
			}

			base.Draw(gameTime);
		}

		private void _drawPlayerEquipIcons()
		{
			Character c;
			if ((c = World.Instance.MainPlayer.ActiveCharacter) != null)
			{
				int widthDelta = statusIcons.Width/4;
				int heightDelta = statusIcons.Height/2;
				int extraOffset = 0; //changes based on presence or absence of other icons
				Color col = Color.FromNonPremultiplied(0x9e, 0x9f, 0x9e, 0xff);
				if (MapRef.IsPK)
				{
					sb.Draw(statusIcons, new Vector2(14, 285), new Rectangle(widthDelta*3, 0, widthDelta, heightDelta), col);
					extraOffset += 24;
				}
				if (c.SpellPrimed)
				{
					sb.Draw(statusIcons, new Vector2(extraOffset + 14, 285), new Rectangle(widthDelta*2, 0, widthDelta, heightDelta), col);
					extraOffset += 24;
				}
				if (c.PaperDoll[(int) EquipLocation.Weapon] != 0)
				{
					sb.Draw(statusIcons, new Vector2(extraOffset + 14, 285), new Rectangle(0, 0, widthDelta, heightDelta), col);
					extraOffset += 24;
				}
				if (c.PaperDoll[(int) EquipLocation.Shield] != 0)
					sb.Draw(statusIcons, new Vector2(extraOffset + 14, 285), new Rectangle(widthDelta, 0, widthDelta, heightDelta), col);
			}
		}

		private void _drawCursor()
		{
			if (!_hideCursor && gridX >= 0 && gridY >= 0 && gridX <= MapRef.Width && gridY <= MapRef.Height)
			{
				//don't draw cursor if context menu is visible and the context menu has the mouse over it
				if (!(m_contextMenu.Visible && m_contextMenu.MouseOver))
					sb.Draw(mouseCursor, cursorPos, _cursorSourceRect, Color.White);
			}
		}

		/* DRAWING-RELATED HELPER METHODS */
		// Special Thanks: HotDog's client. Used heavily as a reference for numeric offsets/techniques, with some adjustments here and there.
		private void _drawGroundLayer()
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			const int localViewLength = 10;
			int xMin = c.X - localViewLength < 0 ? 0 : c.X - localViewLength,
				xMax = c.X + localViewLength > MapRef.Width ? MapRef.Width : c.X + localViewLength;
			int yMin = c.Y - localViewLength < 0 ? 0 : c.Y - localViewLength,
				yMax = c.Y + localViewLength > MapRef.Height ? MapRef.Height : c.Y + localViewLength;
			int cOffX = c.OffsetX, cOffY = c.OffsetY;

			sb.Begin();

			Texture2D fillTileRef = null;
			for (int i = yMin; i <= yMax; ++i)
			{
				for (int j = xMin; j <= xMax; ++j)
				{
					Vector2 pos = _getDrawCoordinates(j, i, cOffX, cOffY);

					//only render fill layer when the ground layer is not present!
					if (MapRef.FillTile > 0 && MapRef.GFXLookup[0][i, j] < 0)
					{
						if (fillTileRef == null) //only do the cache lookup once!
							fillTileRef = GFXLoader.TextureFromResource(GFXTypes.MapTiles, MapRef.FillTile, true);

						sb.Draw(fillTileRef, new Vector2(pos.X - 1, pos.Y - 2),
							Color.FromNonPremultiplied(255, 255, 255, _getAlpha(j, i, c)));
					}

					//ground layer next
					int tile;
					if ((tile = MapRef.GFXLookup[0][i, j]) > 0)
					{
						Texture2D nextTile = GFXLoader.TextureFromResource(GFXTypes.MapTiles, tile, true);
						Rectangle? src = nextTile.Width > 64 ? new Rectangle?(new Rectangle((int)_tileSrc.X, (int)_tileSrc.Y, nextTile.Width / 4, nextTile.Height)) : null;
						if (nextTile.Width > 64)
							sb.Draw(nextTile, new Vector2(pos.X - 1, pos.Y - 2), src, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(j, i, c)));
						else
							sb.Draw(nextTile, new Vector2(pos.X - 1, pos.Y - 2), Color.FromNonPremultiplied(255, 255, 255, _getAlpha(j, i, c)));
					}

					Point loc = new Point(j, i);
					if (_waterTiles.ContainsKey(loc))
					{
						sb.Draw(WaterEffect.WaterTexture, new Vector2(pos.X - 1, pos.Y - 65), _waterTiles[loc].SourceRectangle, Color.White);
					}
				}
			}

			sb.End();
		}

		private void _drawMapItems()
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			
			// Queries (func) for the gfx items within range of the character's X coordinate
			Func<GFX, bool> xGFXQuery = gfx => gfx.x >= c.X - Constants.ViewLength && gfx.x <= c.X + Constants.ViewLength && gfx.x <= MapRef.Width;
			// Queries (func) for the gfxrow items within range of the character's Y coordinate
			Func<GFXRow, bool> yGFXQuery = row => row.y >= c.Y - Constants.ViewLength && row.y <= c.Y + Constants.ViewLength && row.y <= MapRef.Height;

			//items next! (changed to deep copy so I don't get "collection was modified, enumeration may not continued" errors)
			List<Point> keys = new List<Point>(MapItems.Keys.Where(_key => xGFXQuery(new GFX {x = (byte) _key.X}) && yGFXQuery(new GFXRow {y = (byte) _key.Y})));

			sb.Begin();
			foreach (Point pt in keys)
			{
				//deep copies!
				List<MapItem> local = new List<MapItem>(MapItems[pt]);
				foreach(MapItem item in local)
				{
					ItemRecord itemData = World.Instance.EIF.GetItemRecordByID(item.id);
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
			}
			sb.End();
		}

		private void _drawMapObjectsAndActors()
		{
			//also, certain spikes only appear when a player is over them...yikes.
			if (MapRef == null) return;

			Character c = World.Instance.MainPlayer.ActiveCharacter;
			List<EOCharacterRenderer> otherChars = new List<EOCharacterRenderer>(otherRenderers); //copy of list (can remove items)
			List<NPC> otherNpcs;
			lock(npcListLock) //when drawing a frame, don't want to consider NPCs that are added/removed mid-draw - they will be taken care of on next update
				otherNpcs = new List<NPC>(npcList);

			GraphicsDevice.SetRenderTarget(_rtMapObjAbovePlayer);
			GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
			sb.Begin();
			bool targetChanged = false;

			Dictionary<Point, Texture2D> drawRoofLater = new Dictionary<Point, Texture2D>();

			//no need to iterate over the entire map rows if they won't be included in the render.
			for (int rowIndex = Math.Max(c.Y - 22, 0); rowIndex <= Math.Min(c.Y + 22, MapRef.Height); ++rowIndex)
			{
				for (int colIndex = Math.Max(c.X - 22, 0); colIndex <= Math.Min(c.X + 22, MapRef.Width); ++colIndex)
				{
					//once we hit the main players (x, y) coordinate, we need to switch render targets
					if (!targetChanged &&
					    ((c.State != CharacterActionState.Walking && rowIndex == c.Y && colIndex == c.X) ||
					     (c.State == CharacterActionState.Walking && rowIndex == c.DestY && colIndex == c.DestX)))
					{
						sb.End();
						GraphicsDevice.SetRenderTarget(_rtMapObjBelowPlayer);
						GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
						sb.Begin();
						targetChanged = true;
					}

					Texture2D gfx;
					int gfxNum;

					//overlay and shadows: within 10 grid units
					if (Math.Abs(c.X - colIndex) <= 10 && Math.Abs(c.Y - rowIndex) <= 10)
					{
						//overlay/mask  objects
						if ((gfxNum = MapRef.GFXLookup[(int) MapLayers.OverlayObjects][rowIndex, colIndex]) > 0)
						{
							gfx = GFXLoader.TextureFromResource(GFXTypes.MapOverlay, gfxNum, true);
							Vector2 pos = _getDrawCoordinates(colIndex, rowIndex, c);
							pos = new Vector2(pos.X + 16, pos.Y - 11);
							sb.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(colIndex, rowIndex, c)));
						}

						//shadows
						if (World.Instance.ShowShadows && (gfxNum = MapRef.GFXLookup[(int) MapLayers.Shadow][rowIndex, colIndex]) > 0)
						{
							gfx = GFXLoader.TextureFromResource(GFXTypes.Shadows, gfxNum, true);
							Vector2 loc = _getDrawCoordinates(colIndex, rowIndex, c);
							sb.Draw(gfx, new Vector2(loc.X - 24, loc.Y - 12), Color.FromNonPremultiplied(255, 255, 255, 60));
						}
					}

					if (Math.Abs(c.X - colIndex) <= 20 && Math.Abs(c.Y - rowIndex) <= 20)
					{
						//right-facing walls
						if ((gfxNum = MapRef.GFXLookup[(int) MapLayers.WallRowsRight][rowIndex, colIndex]) > 0)
						{
							if (_door != null && _door.x == colIndex && _doorY == rowIndex && _door.doorOpened)
								gfxNum++;

							gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
							Vector2 loc = _getDrawCoordinates(colIndex, rowIndex, c);

							int gfxWidthDelta = gfx.Width/4;
							Rectangle? src = gfx.Width > 32
								? new Rectangle?(new Rectangle(gfxWidthDelta*_wallSrcIndex, 0, gfxWidthDelta, gfx.Height))
								: null;
							loc = new Vector2(loc.X - (int) Math.Round((gfx.Width > 32 ? gfxWidthDelta : gfx.Width)/2.0) + 47,
								loc.Y - (gfx.Height - 29));

							sb.Draw(gfx, loc, src, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(colIndex, rowIndex, c)));
						}

						//down-facing walls
						if ((gfxNum = MapRef.GFXLookup[(int)MapLayers.WallRowsDown][rowIndex, colIndex]) > 0)
						{
							if (_door != null && _door.x == colIndex && _doorY == rowIndex && _door.doorOpened)
								gfxNum++;

							gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
							Vector2 loc = _getDrawCoordinates(colIndex, rowIndex, c);

							int gfxWidthDelta = gfx.Width / 4;
							Rectangle? src = gfx.Width > 32
								? new Rectangle?(new Rectangle(gfxWidthDelta * _wallSrcIndex, 0, gfxWidthDelta, gfx.Height))
								: null;
							loc = new Vector2(loc.X - (int)Math.Round((gfx.Width > 32 ? gfxWidthDelta : gfx.Width) / 2.0) + 15,
								loc.Y - (gfx.Height - 29));

							sb.Draw(gfx, loc, src, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(colIndex, rowIndex, c)));
						}
					}

					//map objects (no filter on distance since that is contained within the loop bounds)
					if ((gfxNum = MapRef.GFXLookup[(int)MapLayers.Objects][rowIndex, colIndex]) > 0)
					{
						gfx = GFXLoader.TextureFromResource(GFXTypes.MapObjects, gfxNum, true);
						Vector2 loc = _getDrawCoordinates(colIndex, rowIndex, c);
						loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 29, loc.Y - (gfx.Height - 28));
						sb.Draw(gfx, loc, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(colIndex, rowIndex, c)));
					}

					//npcs and characters with this {x, y} location
					if (Math.Abs(rowIndex - c.Y) <= Constants.ViewLength && Math.Abs(colIndex - c.X) <= Constants.ViewLength)
					{
						int locrow = rowIndex;
						int loccol = colIndex;

						IEnumerable<NPC> thisLocNpcs = otherNpcs.Where(
							_npc => (_npc.Walking ? _npc.DestY == locrow : _npc.Y == locrow) &&
							        (_npc.Walking ? _npc.DestX == loccol : _npc.X == loccol));
						foreach (NPC npc in thisLocNpcs) npc.DrawToSpriteBatch(sb, true);

						IEnumerable<EOCharacterRenderer> thisLocChars = otherChars.Where(
							_char => (_char.Character.State == CharacterActionState.Walking
								? _char.Character.DestY == locrow && _char.Character.DestX == loccol
								: _char.Character.Y == locrow && _char.Character.X == loccol));
						foreach (EOCharacterRenderer _char in thisLocChars) _char.Draw(sb, true);
					}

					if (Math.Abs(c.X - colIndex) <= 10 && Math.Abs(c.Y - rowIndex) <= 10)
					{
						//roofs (after objects - for outdoor maps, which actually have roofs, this makes more sense)
						if ((gfxNum = MapRef.GFXLookup[(int) MapLayers.Roof][rowIndex, colIndex]) > 0)
						{
							gfx = GFXLoader.TextureFromResource(GFXTypes.MapOverlay, gfxNum, true);
							drawRoofLater.Add(new Point(colIndex, rowIndex), gfx);
						}

						if((gfxNum = MapRef.GFXLookup[(int)MapLayers.Unknown][rowIndex, colIndex]) > 0)
						{
							gfx = GFXLoader.TextureFromResource(GFXTypes.MapWallTop, gfxNum, true);
							Vector2 loc = _getDrawCoordinates(colIndex, rowIndex, c);
							loc = new Vector2(loc.X, loc.Y - 65);
							sb.Draw(gfx, loc, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(colIndex, rowIndex, c)));
						}

						//overlay tiles (counters, etc)
						if ((gfxNum = MapRef.GFXLookup[(int) MapLayers.OverlayTile][rowIndex, colIndex]) > 0)
						{
							gfx = GFXLoader.TextureFromResource(GFXTypes.MapTiles, gfxNum, true);
							Vector2 loc = _getDrawCoordinates(colIndex, rowIndex, c);
							loc = new Vector2(loc.X - 2, loc.Y - 31);
							sb.Draw(gfx, loc, Color.White);
						}
					}
				}
			}

			foreach (var kvp in drawRoofLater)
			{
				Vector2 loc = _getDrawCoordinates(kvp.Key.X, kvp.Key.Y, c);
				loc = new Vector2(loc.X - kvp.Value.Width/2f + 30, loc.Y - kvp.Value.Height + 28);
				sb.Draw(kvp.Value, loc, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(kvp.Key.X, kvp.Key.Y, c)));
			}

			try
			{
				sb.End();
			}
			catch(InvalidOperationException)
			{
				sb.Dispose();
				sb = new SpriteBatch(Game.GraphicsDevice);
			}

			sb.Begin(SpriteSortMode.Deferred, World.Instance.MainPlayer.ActiveCharacter.RenderData.hidden ? BlendState.NonPremultiplied : _playerBlend);
			World.Instance.ActiveCharacterRenderer.Draw(sb, true);
			sb.End();

			GraphicsDevice.SetRenderTarget(null);
		}

		/// <summary>
		/// does the offset for tiles/items
		/// <para>(x * 32 - y * 32 + 288 - c.OffsetX), (y * 16 + x * 16 + 144 - c.OffsetY)</para>
		/// <para>Additional offsets for some gfx will need to be made - this Vector2 is a starting point with calculations required for ALL gfx</para>
		/// </summary>
		private Vector2 _getDrawCoordinates(int x, int y, Character c)
		{
			return _getDrawCoordinates(x, y, c.OffsetX, c.OffsetY);
		}

		private Vector2 _getDrawCoordinates(int x, int y, int cOffX, int cOffY)
		{
			return new Vector2((x * 32) - (y * 32) + 288 - cOffX, (y * 16) + (x * 16) + 144 - cOffY);
		}
		
		private int _getAlpha(int objX, int objY, Character c)
		{
			if (!World.Instance.ShowTransition)
				return 255;

			//get greater of deltas between the map object and the character
			int metric = Math.Max(Math.Abs(objX - c.X), Math.Abs(objY - c.Y));
			const double TRANSITION_TIME_MS = 125.0; //1/8 second for transition on each tile metric

			int alpha;
			if (m_mapLoadTime == null || metric < m_transitionMetric || metric == 0)
				alpha = 255;
			else if (metric == m_transitionMetric)
			{
				double ms = (DateTime.Now - m_mapLoadTime.Value).TotalMilliseconds;
				alpha = (int)Math.Round((ms / TRANSITION_TIME_MS) * 255);
				if (ms / TRANSITION_TIME_MS >= 1)
				{
					m_mapLoadTime = DateTime.Now;
					m_transitionMetric++;
				}
			}
			else
				alpha = 0;

			return alpha;
		}

		/* DISPOSABLE INTERFACE OVERRIDES AND STUFF */
		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				base.Dispose(false);
				return;
			}

			m_drawingEvent.Dispose();
			m_drawingEvent = null;

			foreach (EOCharacterRenderer cr in otherRenderers)
				cr.Dispose();

			lock (npcListLock)
			{
				foreach (NPC npc in npcList)
					npc.Dispose();
			}

			_mouseoverName.Dispose();
			_rtMapObjAbovePlayer.Dispose();
			_rtMapObjBelowPlayer.Dispose();
			_playerBlend.Dispose();
			sb.Dispose();
			_doorTimer.Dispose();

			m_contextMenu.Dispose();

			base.Dispose(true);
		}
	}
}
