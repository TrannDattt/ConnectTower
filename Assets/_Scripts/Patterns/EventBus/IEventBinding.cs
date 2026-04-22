using UnityEngine.Events;

namespace Assets._Scripts.Patterns.EventBus
{
    public interface IEventBinding<T>
    {
        public UnityAction OnEvent {get; set;}
        public UnityAction<T> OnArgEvent {get; set;}
    }

    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        UnityAction onEvent = () => {};
        UnityAction<T> onArgEvent = _ => {};

        public UnityAction OnEvent
        {
            get => onEvent;
            set => onEvent = value;
        }

        public UnityAction<T> OnArgEvent
        {
            get => onArgEvent;
            set => onArgEvent = value;
        }

        public EventBinding(UnityAction onEvent) => this.onEvent = onEvent;
        public EventBinding(UnityAction<T> onArgEvent) => this.onArgEvent = onArgEvent;

        public void Add(UnityAction onEvent) => this.onEvent += onEvent;
        public void Add(UnityAction<T> onArgEvent) => this.onArgEvent += onArgEvent;

        public void Remove(UnityAction onEvent) => this.onEvent -= onEvent;
        public void Remove(UnityAction<T> onArgEvent) => this.onArgEvent -= onArgEvent;
    }
}
