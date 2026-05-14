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
        [SerializeField] private TutorialHandVisual _hand;

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

        public Sequence DisplayText(string message, UnityAction onFinishTalking = null)
        {
            return _tutorialCharacter.Talk(message, () => onFinishTalking?.Invoke());
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
