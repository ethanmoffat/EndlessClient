// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using XNAControls;

namespace EndlessClient.Controls
{
	public class KeyboardSubscriberClickEventHandler
	{
		private readonly KeyboardDispatcher _dispatcher;
		private readonly IKeyboardSubscriber[] _subscribers;

		public KeyboardSubscriberClickEventHandler(KeyboardDispatcher dispatcher, params IKeyboardSubscriber[] subscribers)
		{
			_dispatcher = dispatcher;
			_subscribers = subscribers;
		}

		public void OnClicked(object sender, EventArgs e)
		{
			var ndx = _subscribers.ToList().FindIndex(x => x == sender);
			var previous = ndx - 1 < 0 ? _subscribers.Length - 1 : ndx - 1;

			_subscribers[previous].Selected = false;

			_dispatcher.Subscriber = _subscribers[ndx];
			_subscribers[ndx].Selected = true;
		}
	}
}
