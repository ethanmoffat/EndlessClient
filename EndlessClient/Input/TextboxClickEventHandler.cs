// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using XNAControls.Old;

namespace EndlessClient.Input
{
    public sealed class TextBoxClickEventHandler : IDisposable
    {
        private readonly KeyboardDispatcher _dispatcher;
        private readonly XNATextBox[] _subscribers;

        public TextBoxClickEventHandler(KeyboardDispatcher dispatcher, params XNATextBox[] subscribers)
        {
            _dispatcher = dispatcher;
            _subscribers = subscribers;

            foreach (var textBox in _subscribers)
                textBox.OnClicked += OnClicked;
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var ndx = _subscribers.ToList().FindIndex(x => x == sender);
            var previous = ndx - 1 < 0 ? _subscribers.Length - 1 : ndx - 1;

            _subscribers[previous].Selected = false;

            _dispatcher.Subscriber = _subscribers[ndx];
            _subscribers[ndx].Selected = true;
        }

        public void Dispose()
        {
            foreach (var textBox in _subscribers)
                textBox.OnClicked -= OnClicked;
        }
    }
}
