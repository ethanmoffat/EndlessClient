using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient
{
	public class BlinkingLabel : XNALabel
	{
		/// <summary>
		/// Get or Set the rate of blinking in milliseconds
		/// </summary>
		public int? BlinkRate { get; set; }

		private DateTime? _callbackStartTime;
		private int _callbackDueTime;
		private DateTime _lastToggleTime;
		private Action _callback;

		public BlinkingLabel(Rectangle location, string fontFamily, float fontSize)
			: base(location, fontFamily, fontSize)
		{
			_lastToggleTime = DateTime.Now;
		}

		/// <summary>
		/// Sets some action that is invoked after the specified amount of time
		/// </summary>
		/// <param name="dueTime">Time to wait before invoking (in milliseconds)</param>
		/// <param name="a">Action to invoke</param>
		public void SetCallback(int dueTime, Action a)
		{
			_callbackDueTime = dueTime;
			_callbackStartTime = DateTime.Now;
			_callback = a;
		}

		public override void Update(GameTime gameTime)
		{
			if (_callbackStartTime.HasValue && (DateTime.Now - _callbackStartTime.Value).TotalMilliseconds > _callbackDueTime)
			{
				_callback();
				_callbackStartTime = null;
			}

			if (BlinkRate.HasValue && (DateTime.Now - _lastToggleTime).TotalMilliseconds > BlinkRate)
			{
				_lastToggleTime = DateTime.Now;
				Visible = !Visible;
			}

			base.Update(gameTime);
		}
	}
}
