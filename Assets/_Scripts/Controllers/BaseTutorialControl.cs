using Assets._Scripts.Enums;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    [RequireComponent(typeof(TutorialPopupVisual))]
    public abstract class BaseTutorialControl : MonoBehaviour
    {
        public ETutorial Type {get; protected set;}
        public bool IsFinished {get; protected set;}

        protected TutorialPopupVisual _visual;

        protected EventBinding<PlayerClickEvent> _playerClickBinding;

        protected abstract void SetupBinding();
        public abstract void Begin();
        public abstract void End();

        protected virtual void Awake()
        {
            _visual = GetComponent<TutorialPopupVisual>();
        }

        protected virtual void Start()
        {
            SetupBinding();
        }

        protected virtual void OnEnable()
        {
            EventBus<PlayerClickEvent>.Subscribe(_playerClickBinding);
        }

        protected virtual void OnDisable()
        {
            EventBus<PlayerClickEvent>.Unsubscribe(_playerClickBinding);
        }
    }

    public struct PlayerClickEvent : IEvent
    {
        public GameObject target;
    }
}