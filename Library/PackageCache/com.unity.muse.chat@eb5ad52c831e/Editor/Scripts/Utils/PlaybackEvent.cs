using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Muse.Chat.Utils
{
    class PlaybackEvent<T>
    {
        readonly ConcurrentQueue<T> _history = new();
        readonly ConcurrentQueue<Action<T>> _listeners = new();

        public void Subscribe(Action<T> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            _listeners.Enqueue(listener);

            // Replay history for new subscriber
            foreach (var historicValue in _history)
                listener(historicValue);
        }

        public void Invoke(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _history.Enqueue(value);

            // Notify all listeners
            foreach (var listener in _listeners)
                listener(value);
        }

        public void Unsubscribe(Action<T> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var tempQueue = new Queue<Action<T>>();
            while (_listeners.TryDequeue(out var existingListener))
            {
                if (!existingListener.Equals(listener))
                    tempQueue.Enqueue(existingListener);
            }

            foreach (Action<T> action in tempQueue)
                _listeners.Enqueue(action);
        }

        public void ClearHistory()
        {
            _history.Clear();
        }
    }
}
