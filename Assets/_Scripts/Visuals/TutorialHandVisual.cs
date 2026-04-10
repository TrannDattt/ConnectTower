using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TutorialHandVisual : MonoBehaviour
    {
        [SerializeField] private Image _handPoint;
        [SerializeField] private Image _handClick;

        public IEnumerator MoveAndClick(Vector3 from, Vector3 to)
        {
            float moveDuration = .3f;
            float delaySequence = .2f;

            DOTween.Kill(gameObject, true);
            
            transform.position = from;
            _handClick.gameObject.SetActive(false);
            _handPoint.gameObject.SetActive(true);

            var sequence = DOTween.Sequence().SetTarget(gameObject).SetLoops(-1, LoopType.Restart).SetDelay(delaySequence);
            sequence.Append(transform.DOMove(to, moveDuration). SetEase(Ease.OutQuad));
            sequence.AppendCallback(() =>
            {
                _handClick.gameObject.SetActive(true);
                _handPoint.gameObject.SetActive(false);
            });

            yield return sequence.WaitForCompletion();
        }
    }
}