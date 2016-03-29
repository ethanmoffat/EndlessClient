// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using XNAControls;

namespace EndlessClient.Controls
{
	public class KeyboardSubscriberTabEventHandler
	{
		private readonly KeyboardDispatcher _dispatcher;
		private readonly IKeyboardSubscriber[] _subscribers;

		public KeyboardSubscriberTabEventHandler(KeyboardDispatcher dispatcher, params IKeyboardSubscriber[] subscribers)
		{
			_dispatcher = dispatcher;
			_subscribers = subscribers;
		}

		public void OnTabPressed(object sender, EventArgs e)
		{
			for (int i = 0; i < _subscribers.Length; ++i)
			{
				if (_subscribers[i] != sender)
					continue;

				var next = (i+1)%_subscribers.Length;

				_subscribers[i].Selected = false;

				_dispatcher.Subscriber = _subscribers[next];
				_subscribers[next].Selected = true;

				break;
			}
		}
	}
}
