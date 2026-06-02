using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TutorialPopupVisual : GamePopupVisual
    {
        [SerializeField] private TutorialCharacterVisual _tutorialCharacter;
        [SerializeField] private GameButtonVisual _skipButton;
        [SerializeField] private Text _name;
        [SerializeField] private Image _image;
        [SerializeField] private Text _detail;
        [SerializeField] private RectTransform _gameObjectHolder;

        public bool IsFinished => _activeTutorial == null || _activeTutorial.IsFinished;
        public bool IsDisplayingText => _tutorialCharacter != null && _tutorialCharacter.IsTalking;
        private BaseTutorialControl _activeTutorial = null;

        public IEnumerator ShowTutorial(ETutorial type)
        {
            Debug.Log($"Show tutorial {type}");
            if (_activeTutorial != null) _activeTutorial.enabled = false;
            TutorialManager.GetBehavior(type, out _activeTutorial);
            if (_activeTutorial == null)
            {
                Debug.LogError($"Cant find tutorial of type {type}");
                yield break;
            } 
            _activeTutorial.enabled = true;

            yield return Show();

            _activeTutorial.Begin();
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
                    UserManager.CurUser.MarkTutorialPlayed(_activeTutorial.Type);
                    Debug.Log($"Finish tutorial {_activeTutorial.Type}");
                }
            }
            yield return base.Hide();
        }

        public Sequence DisplayText(DialogAction dialogAction, UnityAction onFinishTalking = null)
        {
            _tutorialCharacter.EnableHand(dialogAction._showHand);
            if (dialogAction._showHand)
                _tutorialCharacter.PointAt(dialogAction._handPos);
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
    }
}
