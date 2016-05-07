// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using EOLib.IO.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class GameLoadingDialog : EODialogBase
	{
		private readonly ILocalizedStringService _localizedStringService;
		private readonly Texture2D _backgroundSprite;
		private DateTime _lastBackgroundUpdate;
		private int _bgSrcIndex;

		public GameLoadingDialog(INativeGraphicsManager nativeGraphicsManager,
								 IGameStateProvider gameStateProvider,
								 IGraphicsDeviceProvider graphicsDeviceProvider,
								 IClientWindowSizeProvider clientWindowSizeProvider,
								 ILocalizedStringService localizedStringService)
			: base(nativeGraphicsManager)
		{
			_localizedStringService = localizedStringService;
			_backgroundSprite = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 33);

			DrawLocation = new Vector2(clientWindowSizeProvider.Width - _backgroundSprite.Width / 4 - 10,
									   clientWindowSizeProvider.Height - _backgroundSprite.Height - 10);

			_setSize(_backgroundSprite.Width / 4, _backgroundSprite.Height);

			_bgSrcIndex = 0;
			_lastBackgroundUpdate = DateTime.Now;

			caption = new XNALabel(new Rectangle(12, 9, 1, 1), Constants.FontSize10)
			{
				Text = _localizedStringService.GetString(DATCONST2.LOADING_GAME_PLEASE_WAIT),
				ForeColor = ColorConstants.LightYellowText
			};
			caption.SetParent(this);

			var gen = new Random();
			var messageTextID = (DATCONST2)gen.Next((int)DATCONST2.LOADING_GAME_HINT_FIRST, (int)DATCONST2.LOADING_GAME_HINT_LAST);
			var localizedMessage = _localizedStringService.GetString(messageTextID);

			message = new XNALabel(new Rectangle(18, 61, 1, 1), Constants.FontSize08)
			{
				TextWidth = 175,
				ForeColor = ColorConstants.MediumGrayText,
				Text = localizedMessage
			};
			message.SetParent(this);

			CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider, false);
		}

		public override void Update(GameTime gt)
		{
			if ((int) (DateTime.Now - _lastBackgroundUpdate).TotalMilliseconds > 500)
			{
				_bgSrcIndex = _bgSrcIndex == 3 ? 0 : _bgSrcIndex + 1;
				_lastBackgroundUpdate = DateTime.Now;
			}

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			SpriteBatch.Begin();
			SpriteBatch.Draw(_backgroundSprite,
				DrawAreaWithOffset,
				new Rectangle(_bgSrcIndex * (_backgroundSprite.Width / 4), 0, _backgroundSprite.Width / 4, _backgroundSprite.Height),
				Color.White);
			SpriteBatch.End();

			base.Draw(gt);
		}

		public void SetState(GameLoadingDialogState whichState)
		{
			switch (whichState)
			{
				case GameLoadingDialogState.Map:
					CaptionText = _localizedStringService.GetString(DATCONST2.LOADING_GAME_UPDATING_MAP);
					break;
				case GameLoadingDialogState.Item:
					CaptionText = _localizedStringService.GetString(DATCONST2.LOADING_GAME_UPDATING_ITEMS);
					break;
				case GameLoadingDialogState.NPC:
					CaptionText = _localizedStringService.GetString(DATCONST2.LOADING_GAME_UPDATING_NPCS);
					break;
				case GameLoadingDialogState.Spell:
					CaptionText = _localizedStringService.GetString(DATCONST2.LOADING_GAME_UPDATING_SKILLS);
					break;
				case GameLoadingDialogState.Class:
					CaptionText = _localizedStringService.GetString(DATCONST2.LOADING_GAME_UPDATING_CLASSES);
					break;
				case GameLoadingDialogState.LoadingGame:
					CaptionText = _localizedStringService.GetString(DATCONST2.LOADING_GAME_LOADING_GAME);
					break;
				default:
					throw new ArgumentOutOfRangeException("whichState", whichState, null);
			}
		}
	}

	public enum GameLoadingDialogState
	{
		Map,
		Item,
		NPC,
		Spell,
		Class,
		LoadingGame
	}
}
