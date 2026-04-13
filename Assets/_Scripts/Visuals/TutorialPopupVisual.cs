using System.Collections;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TutorialPopupVisual : GamePopupVisual
    {
        [SerializeField] private Text _name;
        [SerializeField] private Image _image;
        [SerializeField] private Text _detail;
        [SerializeField] private RectTransform _gameObjectHolder;
        [SerializeField] private TutorialHandVisual _hand;

        public IEnumerator ShowTutorial(ETutorial type)
        {
            var data = TutorialManager.GetTutorialData(type);
            if (data == null) yield break;

            _name.text = data.Name;
            _image.sprite = data.Image;
            _detail.text = data.Detail;

            yield return type switch
            {
                ETutorial.ExtraMove or ETutorial.Shuffle or ETutorial.Hint => ShowBoosterTutorial(data),
                ETutorial.HiddenBlock or ETutorial.CoveredPillar or ETutorial.FrozenBlock => ShowMechanicTutorial(data),
                _ => null,
            };
        }

        private IEnumerator ShowMechanicTutorial(TutorialSO data)
        {
            if (data == null) yield break;
            Debug.Log($"Show tutorial popup for mechanic {data.Type}");
            yield return Show();
        }

        private IEnumerator ShowBoosterTutorial(TutorialSO data)
        {
            if (data == null) yield break;
            Debug.Log($"Show tutorial popup for booster {data.Type}");
            yield return Show();
        }

        public override IEnumerator Show()
        {
            IsActive = true;
            gameObject.SetActive(true);
            yield return DoShowAnim();
        }
    }
}