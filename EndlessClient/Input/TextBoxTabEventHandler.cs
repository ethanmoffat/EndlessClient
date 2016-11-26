// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using XNAControls.Old;

namespace EndlessClient.Input
{
    public sealed class TextBoxTabEventHandler : IDisposable
    {
        private readonly KeyboardDispatcher _dispatcher;
        private readonly XNATextBox[] _subscribers;

        public TextBoxTabEventHandler(KeyboardDispatcher dispatcher, params XNATextBox[] subscribers)
        {
            _dispatcher = dispatcher;
            _subscribers = subscribers;

            foreach (var textBox in _subscribers)
                textBox.OnTabPressed += OnTabPressed;
        }

        private void OnTabPressed(object sender, EventArgs e)
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

        public void Dispose()
        {
            foreach (var textBox in _subscribers)
                textBox.OnTabPressed -= OnTabPressed;
        }
    }
}
