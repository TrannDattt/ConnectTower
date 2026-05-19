using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TEST : MonoBehaviour
    {
        public RectTransform _base;
        public Image _strand;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                DoTest();
            }
        }

        private void DoTest()
        {
            var angle = 45;
            var strandRt = _strand.rectTransform;

            var sequence = DOTween.Sequence();
            sequence.Append(strandRt.DORotate(new Vector3(0, 0, angle), .5f));
            sequence.Join(strandRt.DOAnchorPos(new Vector2(-.3f, 0), .5f));
            sequence.OnComplete(() =>
            {
                _strand.rectTransform.anchoredPosition = Vector2.zero;
                _strand.transform.rotation = Quaternion.identity;
            });
        }
    }
}
