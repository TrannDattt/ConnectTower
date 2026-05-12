using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;
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

        private Dictionary<ETutorial, BaseTutorialControl> _tutorialBehaviorDict = new();
        private BaseTutorialControl _activeTutorial = null;

        public IEnumerator ShowTutorial(ETutorial type)
        {
            if (_activeTutorial != null) _activeTutorial.enabled = false;
            _tutorialBehaviorDict.TryGetValue(type, out _activeTutorial);
            if (_activeTutorial == null) yield break;
            _activeTutorial.enabled = true;

            yield return Show();

            _activeTutorial.Begin();
        }

        public override IEnumerator Show()
        {
            IsActive = true;
            gameObject.SetActive(true);
            yield return DoShowAnim();
        }

        public void DisplayText(string message)
        {
            _tutorialCharacter.Talk(message);
        }

        public void FocusTo(GameObject target)
        {
            _tutorialCharacter.PointAt(target.transform.position);
        }

        void Awake()
        {
            var behaviors = GetComponents<BaseTutorialControl>();
            foreach(var behavior in behaviors)
            {
                _tutorialBehaviorDict[behavior.Type] = behavior;
            }
        }
    }
}