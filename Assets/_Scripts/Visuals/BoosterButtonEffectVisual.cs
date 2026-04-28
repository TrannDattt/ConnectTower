using System.Collections;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public partial class BoosterButtonVisual
    {
        [RequireComponent(typeof(BoosterButtonVisual))]
        public abstract class BoosterButtonEffectVisual : MonoBehaviour
        {
            [Header("Base Curves")]
            // Begin
            [SerializeField] private float _beginMoveDur;
            [SerializeField] private AnimationCurve _beginMoveCurve;
            [SerializeField] private float _beginScaleDur;
            [SerializeField] private float _beginScaleFactor;
            [SerializeField] private AnimationCurve _beginScaleCurve;
            // End
            [SerializeField] private float _endMoveDur;
            [SerializeField] private AnimationCurve _endMoveXCurve;
            [SerializeField] private AnimationCurve _endMoveYCurve;
            [SerializeField] private float _endScaleDur;
            [SerializeField] private AnimationCurve _endScaleCurve;

            private BoosterButtonVisual _buttonVisual;
            private Image _iconImage => _buttonVisual._iconImage;
            private Button _button => _buttonVisual._button;

            private Vector3 _originalIconLocalPos, _originalIconScale;
            private Quaternion _originalIconRotation;
            protected Vector3 _gatherPoint;

            public IEnumerator DoOnUseBoosterAnim(BoosterRuntimeData data, Vector3 gatherPoint)
            {
                Debug.Log($"Do Booster {data.Key} Anim");
                _button.interactable = false;
                _gatherPoint = gatherPoint;
                var boosterSFX = data.Key switch
                {
                    EBooster.ExtraMove => ESfx.ExtraMove,
                    EBooster.Shuffle => ESfx.Shuffle,
                    EBooster.Hint => ESfx.Hint,
                    _ => ESfx.None
                };

                void reset()
                {
                    _iconImage.transform.localPosition = _originalIconLocalPos;
                    _iconImage.transform.localScale = _originalIconScale;
                    _iconImage.transform.localRotation = _originalIconRotation;
                    _button.interactable = true;
                    BoosterController.Instance.FinishBooster();
                    EventBus<UseBoosterEvent>.Publish(new UseBoosterEvent { IsFinish = true });
                }

                EventBus<UseBoosterEvent>.Publish(new UseBoosterEvent { IsFinish = false });
                Sequence beginSequence = DOTween.Sequence()
                                                .Append(_iconImage.transform.DOMove(_gatherPoint, _beginMoveDur).SetEase(_beginMoveCurve))
                                                .Insert(0f, _iconImage.transform.DOScale(_originalIconScale * _beginScaleFactor, _beginScaleDur).SetEase(_beginScaleCurve));

                Sequence endSequence = DOTween.Sequence()
                                            .Append(_iconImage.transform.DOScale(_originalIconScale, _endScaleDur).SetEase(_endScaleCurve))
                                            .Insert(0f, _iconImage.transform.DOLocalMoveX(_originalIconLocalPos.x, _endMoveDur).SetEase(_endMoveXCurve))
                                            .Insert(0f, _iconImage.transform.DOLocalMoveY(_originalIconLocalPos.y, _endMoveDur).SetEase(_endMoveYCurve));

                Sequence mainSequence = DOTween.Sequence()
                                               .Append(DoBoosterAnim(data, _iconImage))
                                               .JoinCallback(() => SoundManager.Instance.PlayRandomSFX(boosterSFX));

                Sequence masterSequence = DOTween.Sequence().SetTarget(gameObject).SetLink(gameObject, LinkBehaviour.KillOnDisable).SetUpdate(true);
                masterSequence.AppendCallback(() => Debug.Log($"Start Booster Anim: {data.Key}"))
                .Append(beginSequence).Append(mainSequence)
                .Append(endSequence).AppendCallback(() => Debug.Log($"Finish Booster Anim: {data.Key}"))
                .OnKill(() =>
                {
                    reset();
                })
                .OnComplete(() => 
                {
                    reset();
                });
                
                yield return masterSequence.WaitForCompletion();
            }

            public abstract Sequence DoBoosterAnim(BoosterRuntimeData data, Image target);

            void Start()
            {
                _buttonVisual = GetComponent<BoosterButtonVisual>();

                _originalIconLocalPos = _iconImage.transform.localPosition;
                _originalIconScale = _iconImage.transform.localScale;
                _originalIconRotation = _iconImage.transform.localRotation;
            }
        }
    }

    public struct UseBoosterEvent : IEvent
    {
        public bool IsFinish;
    }
}