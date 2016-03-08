// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public class NPCSpriteSheet
	{
		private readonly INativeGraphicsManager _gfxManager;
		private readonly NPCRenderer _npcRenderer;

		public NPCSpriteSheet(INativeGraphicsManager gfxManager, NPCRenderer npcToWatch)
		{
			_gfxManager = gfxManager;
			_npcRenderer = npcToWatch;
		}

		public Texture2D GetNPCTexture()
		{
			EODirection dir = _npcRenderer.NPC.Direction;
			int baseGfx = (_npcRenderer.NPC.Data.Graphic - 1) * 40;
			int offset;
			switch (_npcRenderer.Frame)
			{
				case NPCFrame.Standing:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 1 : 3;
					break;
				case NPCFrame.StandingFrame1:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 2 : 4;
					break;
				case NPCFrame.WalkFrame1:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 5 : 9;
					break;
				case NPCFrame.WalkFrame2:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 6 : 10;
					break;
				case NPCFrame.WalkFrame3:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 7 : 11;
					break;
				case NPCFrame.WalkFrame4:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 8 : 12;
					break;
				case NPCFrame.Attack1:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 13 : 15;
					break;
				case NPCFrame.Attack2:
					offset = dir == EODirection.Down || dir == EODirection.Right ? 14 : 16;
					break;
				default:
					return null;
			}

			return _gfxManager.TextureFromResource(GFXTypes.NPC, baseGfx + offset, true);
		}
	}
}