using System.Collections.Generic;
using System.Linq;

namespace Assets._Scripts.Patterns.EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> _eventBindings = new();

        public static void Subscribe(IEventBinding<T> eventBinding)
        {
            _eventBindings.Add(eventBinding);
        }

        public static void Unsubscribe(IEventBinding<T> eventBinding)
        {
            _eventBindings.Remove(eventBinding);
        }

        public static void Publish(T @event)
        {
            var bindingsSnapshot = _eventBindings.ToArray();
            for(int i = 0; i < bindingsSnapshot.Length; i++)
            {
                var binding = bindingsSnapshot[i];
                if (binding == null) continue;
                binding.OnEvent?.Invoke();
                binding.OnArgEvent?.Invoke(@event);
            }
        }

        public static void Clear()
        {
            _eventBindings.Clear();
        }
    }
}
