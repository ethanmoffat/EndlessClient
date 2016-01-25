// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Graphics;
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

	public sealed class EffectRenderer : IDisposable
	{
		private readonly DrawableGameComponent _target;
		private readonly Action _cleanupAction;
		private readonly EffectSpriteManager _effectSpriteManager;

		private IList<EffectSpriteInfo> _effectInfo;
		private DateTime _lastFrameChange;

		private int _effectID;
		private EffectType _effectType;

		private bool _disposed;

		public EffectType EffectType { get { return _effectType; } }

		public EffectRenderer(INativeGraphicsManager gfxManager,
							  NPCRenderer npc,
							  Action cleanupAction = null)
			: this(gfxManager, (DrawableGameComponent)npc, cleanupAction) { }

		public EffectRenderer(INativeGraphicsManager gfxManager,
							  CharacterRenderer character,
							  Action cleanupAction = null)
			: this(gfxManager, (DrawableGameComponent)character, cleanupAction) { }

		private EffectRenderer(INativeGraphicsManager gfxManager,
							   DrawableGameComponent target,
							   Action cleanupAction)
		{
			_target = target;
			_cleanupAction = cleanupAction;
			_effectSpriteManager = new EffectSpriteManager(gfxManager);

			SetEffectInfoTypeAndID(EffectType.Invalid, -1);
		}

		public void SetEffectInfoTypeAndID(EffectType effectType, int effectID)
		{
			_effectID = effectID;
			_effectType = effectType;
		}

		public void ShowEffect()
		{
			if (_effectID < 0 || _effectType == EffectType.Invalid)
				throw new InvalidOperationException("Call SetEffectInfoTypeAndID before initializing");

			_lastFrameChange = DateTime.Now;
			_effectInfo = _effectSpriteManager.GetEffectInfo(_effectType, _effectID);
		}

		public void Restart()
		{
			foreach (var effect in _effectInfo)
				effect.Restart();
		}

		public void Update()
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
					Dispose();
			}
		}

		public void DrawBehindTarget(SpriteBatch sb, bool beginHasBeenCalled = true)
		{
			DrawEffects(sb, beginHasBeenCalled, _effectInfo.Where(x => !x.OnTopOfCharacter));
		}

		public void DrawInFrontOfTarget(SpriteBatch sb, bool beginHasBeenCalled = true)
		{
			DrawEffects(sb, beginHasBeenCalled, _effectInfo.Where(x => x.OnTopOfCharacter));
		}

		#region Drawing Helpers

		private void DrawEffects(SpriteBatch sb, bool beginHasBeenCalled, IEnumerable<EffectSpriteInfo> effectInfo)
		{
			if (_disposed) return;

			if (!beginHasBeenCalled)
				sb.Begin();

			foreach (var ei in effectInfo)
				ei.DrawToSpriteBatch(sb, GetTargetRectangle((dynamic) _target));

			if (!beginHasBeenCalled)
				sb.End();
		}

		private Rectangle GetTargetRectangle(NPCRenderer npc)
		{
			return npc.MapProjectedDrawArea;
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

		#endregion

		#region IDisposable

		~EffectRenderer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			_disposed = true;

			if (disposing)
			{
				_cleanupAction();
			}
		}

		#endregion
	}
}
