using System;
using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets._Scripts.Controllers
{
    [RequireComponent(typeof(TutorialPopupVisual))]
    public abstract class BaseTutorialControl : MonoBehaviour
    {
        [field: SerializeField] public ETutorial Type {get; protected set;}
        public bool IsFinished {get; protected set;}

        [SerializeField] protected DialogAction[] _dialogActions;

        protected TutorialPopupVisual _visual;

        protected EventBinding<PlayerClickEvent> _playerClickBinding;
        protected int _clickCount;

        protected abstract void HandlingEvent(PlayerClickEvent @event);
        public abstract void Begin();
        public abstract void End();

        protected virtual void Awake()
        {
            _visual = GetComponent<TutorialPopupVisual>();
        }

        protected virtual void OnEnable()
        {
            IsFinished = false;
            _clickCount = 0;
            _playerClickBinding = new (HandlingEvent);
            EventBus<PlayerClickEvent>.Subscribe(_playerClickBinding);
        }

        protected virtual void OnDisable()
        {
            EventBus<PlayerClickEvent>.Unsubscribe(_playerClickBinding);
        }

        protected virtual void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Dùng Input Manager
            {
                // 1. Tạo dữ liệu Pointer giả lập
                PointerEventData eventData = new(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                // 2. Danh sách chứa các kết quả va chạm (Raycast)
                var results = new List<RaycastResult>();

                // 3. Bắn Raycast từ EventSystem
                EventSystem.current.RaycastAll(eventData, results);
                EventBus<PlayerClickEvent>.Publish(new PlayerClickEvent{Time = Time.time, Data = eventData, Results = results});

                if (results.Count > 0)
                {
                    Debug.Log("Bạn vừa click trúng UI: " + results[0].gameObject.name);
                }
            }
        }

        protected void RegisterPlayerClick(PlayerClickEvent _)
        {
            _clickCount++;
            Debug.Log($"Clicked {_clickCount} times");
        }
    }

    [Serializable]
    public struct DialogAction
    {
        [TextArea(minLines: 1, maxLines: 3)] public string Message;
        public bool StopFlag;
    }

    public struct PlayerClickEvent : IEvent
    {
        public float Time;
        public PointerEventData Data;
        public List<RaycastResult> Results;
    }
}
