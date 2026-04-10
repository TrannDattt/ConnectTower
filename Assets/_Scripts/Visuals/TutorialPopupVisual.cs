using System.Collections;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class TutorialPopupVisual : GamePopupVisual
    {
        [SerializeField] private RectTransform _gameObjectHolder;
        [SerializeField] private TutorialHandVisual _hand;

        private ETutorial _curTutorial = ETutorial.None;

        public IEnumerator ShowTutorial(ETutorial type)
        {
            _curTutorial = type;
            return type switch
            {
                ETutorial.ExtraMove or ETutorial.Shuffle or ETutorial.Hint => ShowBoosterTutorial(TutorialManager.TutorialToBooster(type)),
                ETutorial.HiddenBlock or ETutorial.CoveredPillar or ETutorial.FrozenBlock => ShowMechanicTutorial(TutorialManager.TutorialToMechanic(type)),
                _ => null,
            };
        }

        private IEnumerator ShowMechanicTutorial(EMechanic? type)
        {
            if (type == null) yield break;
            Debug.Log($"Show tutorial popup for mechanic {type}");
            yield return Show();
        }

        private IEnumerator ShowBoosterTutorial(EBooster? type)
        {
            if (type == null) yield break;
            Debug.Log($"Show tutorial popup for booster {type}");
            yield return Show();
        }

        public override IEnumerator Show()
        {
            IsActive = true;
            gameObject.SetActive(true);
            yield return DoShowAnim();
        }

        public override IEnumerator Hide()
        {
            // UserManager.MarkTutorialPlayed(_curTutorial);
            return base.Hide();
        }
    }
}