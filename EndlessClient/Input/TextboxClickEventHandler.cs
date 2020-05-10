using System;
using System.Linq;
using XNAControls;

namespace EndlessClient.Input
{
    public sealed class TextBoxClickEventHandler : IDisposable
    {
        private readonly KeyboardDispatcher _dispatcher;
        private readonly IXNATextBox[] _subscribers;

        public TextBoxClickEventHandler(KeyboardDispatcher dispatcher, params IXNATextBox[] subscribers)
        {
            _dispatcher = dispatcher;
            _subscribers = subscribers;

            foreach (var textBox in _subscribers)
                textBox.OnClicked += OnClicked;
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var ndx = _subscribers.ToList().FindIndex(x => x == sender);

            _dispatcher.Subscriber = _subscribers[ndx];
        }

        public void Dispose()
        {
            foreach (var textBox in _subscribers)
                textBox.OnClicked -= OnClicked;
        }
    }
}
