// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class GameLoadingDialog : EODialogBase
	{
		//different captions as the dialog progresses through different states
		private readonly string map;
		private readonly string item;
		private readonly string npc;
		private readonly string skill;
		private readonly string classes;
		private readonly string loading;

		private readonly Texture2D bgSprites;
		private int bgSrcIndex;
		private TimeSpan? timeOpened;

		private bool updatingFiles;

		public WelcomeMessageData WelcomeData { get; private set; }

		public GameLoadingDialog(PacketAPI apiHandle)
			: base(apiHandle)
		{
			bgTexture = null; //don't use the built in bgtexture, we're going to use a sprite sheet for the BG
			bgSprites = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 33);
			DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth - (bgSprites.Width / 4) - 10,
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight - bgSprites.Height - 10);
			_setSize(bgSprites.Width / 4, bgSprites.Height);
			bgSrcIndex = 0;

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized2];
			map = file.Data[(int)DATCONST2.LOADING_GAME_UPDATING_MAP];
			item = file.Data[(int)DATCONST2.LOADING_GAME_UPDATING_ITEMS];
			npc = file.Data[(int)DATCONST2.LOADING_GAME_UPDATING_NPCS];
			skill = file.Data[(int)DATCONST2.LOADING_GAME_UPDATING_SKILLS];
			classes = file.Data[(int)DATCONST2.LOADING_GAME_UPDATING_CLASSES];
			loading = file.Data[(int)DATCONST2.LOADING_GAME_LOADING_GAME];

			caption = new XNALabel(new Rectangle(12, 9, 1, 1), Constants.FontSize10)
			{
				Text = file.Data[(int)DATCONST2.LOADING_GAME_PLEASE_WAIT],
				ForeColor = Constants.LightYellowText
			};
			caption.SetParent(this);

			Random gen = new Random();
			int msgTxt = gen.Next((int)DATCONST2.LOADING_GAME_HINT_FIRST, (int)DATCONST2.LOADING_GAME_HINT_LAST);

			message = new XNALabel(new Rectangle(18, 61, 1, 1), Constants.FontSize08)
			{
				TextWidth = 175,
				ForeColor = Constants.MediumGrayText,
				Text = file.Data[msgTxt]
			};
			message.SetParent(this);

			endConstructor(false);
		}

		public override async void Update(GameTime gt)
		{
			if (timeOpened == null)
			{
				timeOpened = gt.TotalGameTime;
			}

#if DEBUG
			const int intialTimeDelay = 0; //set to zero on debug builds
#else
			const int intialTimeDelay = 5; //I think the client waits 5 seconds?
#endif

			if (!updatingFiles && ((int)gt.TotalGameTime.TotalSeconds - (int)(timeOpened.Value.TotalSeconds)) >= intialTimeDelay)
			{
				updatingFiles = true;

				bool result = await FetchFilesAsync();
				Close(null, result ? XNADialogResult.OK : XNADialogResult.NO_BUTTON_PRESSED);
			}

			//every half a second
			if (((int)gt.TotalGameTime.TotalMilliseconds - (int)(timeOpened.Value.TotalMilliseconds)) % 500 == 0)
			{
				//switch the background image to the next one
				bgSrcIndex = bgSrcIndex == 3 ? 0 : bgSrcIndex + 1;
			}

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			SpriteBatch.Begin();
			SpriteBatch.Draw(bgSprites, DrawAreaWithOffset, new Rectangle(bgSrcIndex * (bgSprites.Width / 4), 0, bgSprites.Width / 4, bgSprites.Height), Color.White);
			SpriteBatch.End();

			base.Draw(gt);
		}

		private async Task<bool> FetchFilesAsync()
		{
			if (!await _getMapTask())
				return false;
			if (!await _getEIFTask())
				return false;
			if (!await _getENFTask())
				return false;
			if (!await _getESFTask())
				return false;
			if (!await _getECFTask())
				return false;

			caption.Text = loading;
			WelcomeMessageData data;
			if (!m_api.WelcomeMessage(World.Instance.MainPlayer.ActiveCharacter.ID, out data))
				return false;
			WelcomeData = data;

			await TaskFramework.Delay(1000);
			return true;
		}

		#region Task Methods

		private async Task<bool> _getMapTask()
		{
			if (World.Instance.NeedMap != -1)
			{
				caption.Text = map;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Map, World.Instance.MainPlayer.ActiveCharacter.CurrentMap))
						return false;
				} while (!_isMapValid());
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getEIFTask()
		{
			if (World.Instance.NeedEIF)
			{
				caption.Text = item;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Item))
						return false;
				} while (!_isPubValid(InitFileType.Item));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getENFTask()
		{
			if (World.Instance.NeedENF)
			{
				caption.Text = npc;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Npc))
						return false;
				} while (!_isPubValid(InitFileType.Npc));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getESFTask()
		{
			if (World.Instance.NeedESF)
			{
				caption.Text = skill;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Spell))
						return false;
				} while (!_isPubValid(InitFileType.Spell));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		private async Task<bool> _getECFTask()
		{
			if (World.Instance.NeedECF)
			{
				caption.Text = classes;
				int tries = 0;
				do
				{
					if (tries++ == 3)
						return false;

					if (!m_api.RequestFile(InitFileType.Class))
						return false;
				} while (!_isPubValid(InitFileType.Class));
				await TaskFramework.Delay(1000);
			}
			return true;
		}

		#endregion

		#region File Validation

		private static bool _isMapValid()
		{
			try
			{
				// ReSharper disable once UnusedVariable
				var map = new MapFile(string.Format(Constants.MapFileFormatString, World.Instance.NeedMap));
			}
			catch { return false; }

			return true;
		}

		private static bool _isPubValid(InitFileType fileType)
		{
			try
			{
				EODataFile file;
				switch (fileType)
				{
					case InitFileType.Item:
						file = new ItemFile();
						break;
					case InitFileType.Npc:
						file = new NPCFile();
						break;
					case InitFileType.Spell:
						file = new SpellFile();
						break;
					case InitFileType.Class:
						file = new ClassFile();
						break;
					default:
						return false;
				}

				if (file.Data.Count <= 1) return false;
			}
			catch
			{
				return false;
			}

			return true;
		}

		#endregion
	}
}
