using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
    [MappedType(BaseType = typeof(IGameComponent), IsSingleton = true)]
    public class DispatcherGameComponent : GameComponent
    {
        private static readonly object _queuelocker_ = new object();
        private static readonly Queue<Action> _actions = new Queue<Action>();
        private static readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private static bool _waiting = false;

        public DispatcherGameComponent(IEndlessGameProvider endlessGameProvider)
            : base((Game)endlessGameProvider.Game)
        {
        }

        public static async Task InvokeAsync(Action action)
        {
            lock (_queuelocker_)
            {
                _actions.Enqueue(action);
                _waiting = true;
            }

            await _signal.WaitAsync().ConfigureAwait(false);
        }

        public static void Invoke(Action action)
        {
            lock (_queuelocker_)
            {
                _actions.Enqueue(action);
            }
        }

        public override void Update(GameTime gameTime)
        {
            lock (_queuelocker_)
            {
                if (_actions.Any())
                {
                    _actions.Dequeue().Invoke();

                    if (_waiting)
                    {
                        _waiting = false;
                        _signal.Release();
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
