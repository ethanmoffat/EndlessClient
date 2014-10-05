using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Handlers;
using EOLib;
using Microsoft.Xna.Framework;

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
		private List<Character> otherPlayers = new List<Character>();
		public List<NPC> NPCs { get; set; } 

		public MapFile MapRef { get; set; }

		public EOMapRenderer(EOGame g, MapFile mapObj)
			: base(g)
		{
			if(g == null)
				throw new NullReferenceException("The game must not be null");

			MapRef = mapObj;
			MapItems = new List<MapItem>();
			NPCs = new List<NPC>();
		}

		//super basic implementation for passing on chat to the game's actual HUD
		//map renderer will have to show the speech bubble
		public void RenderChatMessage(Handlers.TalkType messageType, int playerID, string message, ChatType chatType = ChatType.None)
		{
			//convert the messageType into a valid ChatTab to pass everything on to
			ChatTabs tab;
			switch (messageType)
			{
				case TalkType.Local: tab = ChatTabs.Local;
					break;
				default: throw new NotImplementedException();
			}

			//get the character name for the player ID that was received
			string playerName = otherPlayers.Find(x => x.ID == playerID).Name;

			if (EOGame.Instance.Hud == null)
				return;
			EOGame.Instance.Hud.AddChat(tab, playerName, message, chatType);

			//TODO: Add whatever magic is necessary to make chat bubble appear
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
	}
}
