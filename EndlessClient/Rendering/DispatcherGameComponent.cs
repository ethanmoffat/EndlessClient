using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EndlessClient.Rendering
{
    [MappedType(BaseType = typeof(IGameComponent), IsSingleton = true)]
    public class DispatcherGameComponent : GameComponent
    {
        private static readonly object _queuelocker_ = new object();
        private static readonly Queue<Action> _actions = new Queue<Action>();
        private static readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public DispatcherGameComponent(IEndlessGameProvider endlessGameProvider)
            : base((Game)endlessGameProvider.Game)
        {
        }

        public static async Task Invoke(Action action)
        {
            lock (_queuelocker_)
            {
                _actions.Enqueue(action);
            }

            await _signal.WaitAsync().ConfigureAwait(false);
        }

        public override void Update(GameTime gameTime)
        {
            lock(_queuelocker_)
            {
                if (_actions.Any())
                {
                    _actions.Dequeue().Invoke();
                    _signal.Release();
                }
            }

            base.Update(gameTime);
        }
    }
}
