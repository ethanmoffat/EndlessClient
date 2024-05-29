using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
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

        public BlinkingLabel(string spriteFontContentName)
            : base(spriteFontContentName)
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
            try
            {
                if (_callbackStartTime.HasValue && (DateTime.Now - _callbackStartTime.Value).TotalMilliseconds > _callbackDueTime)
                {
                    _callback?.Invoke();
                    _callbackStartTime = null;
                }

                if (BlinkRate.HasValue && (DateTime.Now - _lastToggleTime > TimeSpan.FromMilliseconds(BlinkRate.Value)))
                {
                    _lastToggleTime = DateTime.Now;
                    Visible = !Visible;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
            }

            try
            {
                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Base Update error: {ex.Message}"); // Catching and logging base class update errors
            }
        }




    }
}
