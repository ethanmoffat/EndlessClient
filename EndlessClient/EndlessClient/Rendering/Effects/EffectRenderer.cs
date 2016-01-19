// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public enum EffectType
	{
		Invalid,
		Potion,
		Spell,
		WarpOriginal,
		WarpDestination,
		WaterSplashies
	}

	public class EffectRenderer : DrawableGameComponent
	{
		private readonly DrawableGameComponent _target;
		private readonly Action _cleanupAction;
		private readonly EffectSpriteManager _effectSpriteManager;
		private readonly SpriteBatch _sb;

		private IList<EffectSpriteInfo> _effectInfo;
		private DateTime _lastFrameChange;

		private int _effectID;
		private EffectType _effectType;

		private bool _disposed;

		public EffectRenderer(EOGame game, NPCRenderer npc, Action cleanupAction)
			: this(game, (DrawableGameComponent)npc, cleanupAction) { }

		public EffectRenderer(EOGame game, CharacterRenderer character, Action cleanupAction)
			: this(game, (DrawableGameComponent)character, cleanupAction) { }

		private EffectRenderer(EOGame game, DrawableGameComponent target, Action cleanupAction)
			: base(game)
		{
			_target = target;
			_cleanupAction = cleanupAction;
			_effectSpriteManager = new EffectSpriteManager(game.GFXManager);
			_sb = new SpriteBatch(game.GraphicsDevice);

			SetEffectInfoTypeAndID(EffectType.Invalid, -1);
		}

		public void SetEffectInfoTypeAndID(EffectType effectType, int effectID)
		{
			_effectID = effectID;
			_effectType = effectType;
		}

		public void ShowEffect()
		{
			if (Game.Components.Contains(this))
				throw new InvalidOperationException("This component is automatically managed. Do not manually add to Game Components list.");
			Game.Components.Add(this); //calls Initialize() as part of the Add
		}

		public void Restart()
		{
			foreach (var effect in _effectInfo)
				effect.Restart();
		}

		public override void Initialize()
		{
			if (_effectID < 0 || _effectType == EffectType.Invalid)
				throw new InvalidOperationException("Call SetEffectInfoTypeAndID before initializing");

			_lastFrameChange = DateTime.Now;
			_effectInfo = _effectSpriteManager.GetEffectInfo(_effectType, _effectID);

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			if (_disposed) return;

			var nowTime = DateTime.Now;
			if ((nowTime - _lastFrameChange).TotalMilliseconds > 100)
			{
				_lastFrameChange = nowTime;
				_effectInfo.ToList().ForEach(ei => ei.NextFrame());

				var doneEffects = _effectInfo.Where(ei => ei.Done);
				doneEffects.ToList().ForEach(ei => _effectInfo.Remove(ei));

				if (_effectInfo.Count == 0)
				{
					Dispose();
					return;
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (_disposed) return;

			var drawLater = _effectInfo.Where(x => x.OnTopOfCharacter).ToList();
			var drawFirst = _effectInfo.Except(drawLater);

			_sb.Begin();
			drawFirst.ToList().ForEach(x => x.DrawToSpriteBatch(_sb, GetTargetRectangle((dynamic) _target)));

			DrawTarget((dynamic)_target);

			drawLater.ForEach(x => x.DrawToSpriteBatch(_sb, GetTargetRectangle((dynamic) _target)));
			_sb.End();

			base.Draw(gameTime);
		}

		private void DrawTarget(NPCRenderer npc)
		{
			npc.DrawToSpriteBatch(_sb, true);
		}

		private void DrawTarget(CharacterRenderer character)
		{
			character.Draw(_sb, true);
		}

		private void DrawTarget(object fail)
		{
			throw new ArgumentException("fail: " + fail);
		}

		private Rectangle GetTargetRectangle(NPCRenderer npc)
		{
			return npc.DrawArea;
		}

		private Rectangle GetTargetRectangle(CharacterRenderer character)
		{
			//Because the rendering code is terrible, the character rectangle needs an additional offset
			var rect = character.DrawAreaWithOffset;
			rect.Offset(6, 11);
			return rect;
		}

		private Rectangle GetTargetRectangle(object fail)
		{
			//Seriously, the Skywalker family has a great history of being able to say NOOO in a dramatic way
			throw new ArgumentException("No. Nooo. NOOOOO! THAT'S NOT TRUE! THAT'S IMPOSSIBLE! " + fail, "fail");
		}

		protected override void Dispose(bool disposing)
		{
			_disposed = true;

			if (disposing)
			{
				Game.Components.Remove(this);

				_cleanupAction();

				_sb.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
