using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelIndexVisual : MonoBehaviour
    {
        [SerializeField] private Text _levelIndexText;
        [SerializeField] private float _animDuration = 0.5f;

        private Vector3 _initialPos;
        private Vector3 _initialScale;

        public void SetLevelIndex(int index)
        {
            if (_levelIndexText != null)
            {
                _levelIndexText.text = $"Level {index}";
            }
        }

        public void PrepareAnim()
        {
            gameObject.SetActive(false);
            _initialPos = transform.position;
            _initialScale = transform.localScale;
        }

        public IEnumerator DoLevelIndexAnim(Vector2 pos)
        {
            Vector3 startScale = new(5, 5, 5);
            float scaleDuration = .7f;
            float stayDuration = .5f;

            transform.position = pos;
            transform.localScale = startScale;
            gameObject.SetActive(true);

            yield return DOTween.Sequence()
                                .Append(transform.DOScale(startScale * 1.15f, scaleDuration * 0.5f).SetEase(Ease.OutQuad))
                                .Append(transform.DOScale(startScale, scaleDuration * 0.5f).SetEase(Ease.InQuad))
                                .AppendInterval(stayDuration)
                                .Append(transform.DOMove(_initialPos, _animDuration).SetEase(Ease.OutQuad))
                                .Join(transform.DOScale(_initialScale, _animDuration).SetEase(Ease.OutQuad))
                                .SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable)
                                .WaitForCompletion();
        }
    }
}
