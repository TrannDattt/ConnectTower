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
            for(int i = 0; i < _eventBindings.Count; i++)
            {
                var binding = _eventBindings.ElementAt(i);
                binding.OnEvent.Invoke();
                binding.OnArgEvent.Invoke(@event);
            }
        }

        public static void Clear()
        {
            _eventBindings.Clear();
        }
    }
}
