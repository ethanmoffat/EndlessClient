using System;
using XNAControls;

namespace EndlessClient.Input
{
    public sealed class TextBoxTabEventHandler : IDisposable
    {
        private readonly KeyboardDispatcher _dispatcher;
        private readonly IXNATextBox[] _subscribers;

        public TextBoxTabEventHandler(KeyboardDispatcher dispatcher, params IXNATextBox[] subscribers)
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

                _dispatcher.Subscriber = _subscribers[next];

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
