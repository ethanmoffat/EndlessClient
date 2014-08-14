using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAControls;

namespace EndlessClient
{
	public enum InGameStates
	{
		News = -1,
		Inventory = 0,
		Active = 2,
		Passive = 3,
		Chat = 4,
		Stats = 5,
		Online = 6,
		Party = 7,
		Settings = 9,
		Help = 10
	}

	/// <summary>
	/// Note that this is NOT an XNAControl - it is just a DrawableGameComponent
	/// </summary>
	public class HUD : DrawableGameComponent
	{
		private const int NUM_BTN = 11;
		private Texture2D mainFrame, mainButtonTexture;
		private XNAButton[] mainBtn;
		private SpriteBatch SpriteBatch;
		private InGameStates state;

		public HUD(Game g)
			: base(g)
		{
			mainFrame = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 1, true);
			mainButtonTexture = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 25, false);
			mainBtn = new XNAButton[NUM_BTN];

			for (int i = 0; i < NUM_BTN; ++i)
			{
				Texture2D _out = new Texture2D(g.GraphicsDevice, mainButtonTexture.Width / 2, mainButtonTexture.Height / NUM_BTN);
				Texture2D _ovr = new Texture2D(g.GraphicsDevice, mainButtonTexture.Width / 2, mainButtonTexture.Height / NUM_BTN);

				Rectangle _outRec = new Rectangle(0, i * _out.Height, _out.Width, _out.Height);
				Rectangle _ovrRec = new Rectangle(_ovr.Width, i * _ovr.Height, _ovr.Width, _ovr.Height);

				Color[] _outBuf = new Color[_outRec.Width * _outRec.Height];
				Color[] _ovrBuf = new Color[_ovrRec.Width * _ovrRec.Height];

				mainButtonTexture.GetData(0, _outRec, _outBuf, 0, _outBuf.Length);
				_out.SetData<Color>(_outBuf);

				mainButtonTexture.GetData(0, _ovrRec, _ovrBuf, 0, _ovrBuf.Length);
				_ovr.SetData<Color>(_ovrBuf);

				//0-5: left side, starting at 59, 327 with increments of 20
				//6-10: right side, starting at 587, 347
				Vector2 btnLoc = new Vector2(i < 6 ? 62 : 590, (i < 6 ? 330 : 350) + ((i < 6 ? i : i - 6) * 20));

				mainBtn[i] = new XNAButton(g, new Texture2D[] { _out, _ovr }, btnLoc);
				//mainBtn[i].Visible = false;
			}

			//left button onclick events
			mainBtn[0].OnClick += OnViewInventory;
			mainBtn[1].OnClick += OnViewMap;
			mainBtn[2].OnClick += OnViewActiveSkills;
			mainBtn[3].OnClick += OnViewPassiveSkills;
			mainBtn[4].OnClick += OnViewChat;
			mainBtn[5].OnClick += OnViewStats;

			//right button onclick events
			mainBtn[6].OnClick += OnViewPlayers;
			mainBtn[7].OnClick += OnViewParty;
			//mainBtn[8].OnClick += OnViewMacro; //not implemented in EO client
			mainBtn[9].OnClick += OnViewSettings;
			mainBtn[10].OnClick += OnViewHelp;

			SpriteBatch = new SpriteBatch(g.GraphicsDevice);

			state = InGameStates.News;
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch.Begin();
			SpriteBatch.Draw(mainFrame, new Vector2(0, 0), Color.White);

			switch(state)
			{
				default:
					break;
			}

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		#region ButtonClickEventHandlers
		private void OnViewInventory(object sender, EventArgs e)
		{
			state = InGameStates.Inventory;
		}

		private void OnViewMap(object sender, EventArgs e)
		{
			/* Check if map file allows minimap viewing */
			/* set flag accordingly */
		}

		private void OnViewActiveSkills(object sender, EventArgs e)
		{
			state = InGameStates.Active;
		}
		private void OnViewPassiveSkills(object sender, EventArgs e)
		{
			state = InGameStates.Passive;
		}
		private void OnViewChat(object sender, EventArgs e)
		{
			state = InGameStates.Chat;
		}
		private void OnViewStats(object sender, EventArgs e)
		{
			state = InGameStates.Stats;
		}
		private void OnViewPlayers(object sender, EventArgs e)
		{
			state = InGameStates.Online;
		}
		private void OnViewParty(object sender, EventArgs e)
		{
			state = InGameStates.Party;
		}
		private void OnViewSettings(object sender, EventArgs e)
		{
			state = InGameStates.Settings;
		}
		private void OnViewHelp(object sender, EventArgs e)
		{
			/* pop up the help dialog */
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			foreach (XNAButton btn in mainBtn)
				btn.Dispose();

			SpriteBatch.Dispose();

			base.Dispose(disposing);
		}
	}
}
