using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TutorialPopupVisual : GamePopupVisual
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TutorialCharacterVisual _tutorialCharacter;
        [SerializeField] private GameButtonVisual _skipButton;
        [SerializeField] private TutorialHandVisual _hand;

        public bool IsFinished => _activeTutorial == null || _activeTutorial.IsFinished;
        public bool IsDisplayingText => _tutorialCharacter != null && _tutorialCharacter.IsTalking;
        private BaseTutorialControl _activeTutorial = null;

        public IEnumerator ShowTutorial(ETutorial type)
        {
            Debug.Log($"Show tutorial {type}");
            if (_activeTutorial != null) _activeTutorial.enabled = false;
            _activeTutorial = GetLocalBehavior(type);

            if (_activeTutorial == null)
            {
                Debug.LogError($"Cant find local tutorial behavior of type {type} on popup {name}");
                yield break;
            }

            _activeTutorial.enabled = true;

            yield return Show();

            if (_activeTutorial.gameObject != gameObject)
            {
                Debug.LogError($"Tutorial behavior {type} is bound to wrong popup instance: {name}");
                yield break;
            }

            if (!gameObject.activeInHierarchy)
            {
                ForceActivateHierarchy();
                PopupManager.Instance?.EnsureTutorialPresentationActive();
                yield return null;
            }

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError($"Tutorial popup {name} is still inactive after Show(). Hierarchy: {GetHierarchyState()}");
                yield break;
            }

            _activeTutorial.Begin();
        }

        private BaseTutorialControl GetLocalBehavior(ETutorial type)
        {
            var behaviors = GetComponents<BaseTutorialControl>();
            foreach (var behavior in behaviors)
            {
                if (behavior != null && behavior.Type == type)
                {
                    return behavior;
                }
            }

            return null;
        }

        public void StopPointing()
        {
            _tutorialCharacter.StopPoint(true);
        }

        public Tween MoveNarrator(Vector2 pos)
        {
            return _tutorialCharacter.Move(pos);
        }

        public override IEnumerator Show()
        {
            IsActive = true;
            gameObject.SetActive(true);
            yield return DoShowAnim();
        }

        public override IEnumerator Hide()
        {
            if (_activeTutorial != null)
            {
                if (!_activeTutorial.IsFinished)
                {
                    _activeTutorial.End();
                }
                else
                {
                    UserManager.MarkTutorialPlayed(_activeTutorial.Type);
                    Debug.Log($"Finish tutorial {_activeTutorial.Type}");
                }
            }
            yield return base.Hide();
        }

        public Sequence DisplayText(DialogAction dialogAction, UnityAction onFinishTalking = null)
        {
            _tutorialCharacter.EnableHand(dialogAction._showHand);
            if (dialogAction._showHand)
            {
                _tutorialCharacter.PointAt(dialogAction._handPos);
            }

            return _tutorialCharacter.Talk(dialogAction.Message, () => onFinishTalking?.Invoke());
        }

        public void CompleteDisplayedText()
        {
            _tutorialCharacter?.CompleteTalk();
        }

        public void FocusTo(GameObject target)
        {
            _tutorialCharacter.PointAt(target.transform.position);
        }

        private void ForceActivateHierarchy()
        {
            Transform current = transform;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                current = current.parent;
            }
        }

        private string GetHierarchyState()
        {
            var builder = new StringBuilder();
            Transform current = transform;
            while (current != null)
            {
                if (builder.Length > 0)
                {
                    builder.Append(" <- ");
                }

                builder.Append(current.name)
                       .Append("(self=")
                       .Append(current.gameObject.activeSelf)
                       .Append(",hier=")
                       .Append(current.gameObject.activeInHierarchy)
                       .Append(')');

                current = current.parent;
            }

            return builder.ToString();
        }
    }
}
