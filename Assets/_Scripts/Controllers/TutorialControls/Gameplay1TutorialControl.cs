using System.Collections;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class Gameplay1TutorialControl : BaseTutorialControl
    {
        // Description: Tutorial for level 1: Show player how to move block from pillar1 to pillar2
        // CHARACTER
        [SerializeField] private float _dialogDelay;
        [SerializeField] private Vector2[] _characterPos;

        // CHANGE LAYER
        [SerializeField] private float _overlayOpacity;
        [SerializeField] private float _changeLayerDur;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private float _pillarNewScaleFactor;
        
        // HIGHLIGHT
        [SerializeField] private float _pillarHighlightScaleFactor;
        [SerializeField] private float _highlightTransitionDur;

        // DISABLE HIGHLIGHT
        [SerializeField] private float _blockMoveDelay;
        [SerializeField] private float _disableHighlightTransitionDur;

        // WAIT VICTORY POPUP
        [SerializeField] private float _victoryPopupDur;

        private PillarController _pillar1; // Pillar with 3 blocks
        private PillarController _pillar2; // Other pillar 
        private Vector3 _pillarBaseScale;
        private int _pillarBaseLayer;

        private PlayerClickEvent _latestClickEvent;
        private Coroutine _dialogCoroutine;

        // private int SkipStopPoint(int curIndex)
        // {
        //     for(; curIndex < _dialogActions.Length; curIndex++)
        //     {
        //         if (_dialogActions[curIndex].StopFlag) break;
        //     }

        //     Debug.Log($"Skip to dialog {curIndex}");
        //     return curIndex;
        // }

        public override void Begin()
        {   
            var pillars = BoardController.Instance.GetAllPillars();
            _pillar1 = pillars.FirstOrDefault(p => p.GetAllBlocks().Count == 1);
            if (_pillar1 == null)
            {
                Debug.LogError("Cant find pillar");
                return;
            }
            pillars.Remove(_pillar1);
            _pillar2 = pillars[0];
            _pillarBaseScale = _pillar1.transform.localScale;
            _pillarBaseLayer = _pillar1.gameObject.layer;

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new PillarController[] {_pillar1, _pillar2});
            GameManager.Instance.UnsubcribeIngameEvent?.Invoke();

            StartCoroutine(DoTutorial());
        }

        // private IEnumerator DoTutorial()
        // {
        //     int dialogIndex = 0;
        //     EventBinding<PillarClickedEvent> pillarClickBinding;

        //     int dialog3Index = -1;
        //     PillarController chosen = null, other = null;
        //     pillarClickBinding = new EventBinding<PillarClickedEvent>((e) =>
        //     {
        //         if (chosen == null)
        //         {
        //             //TODO: Disable highlight _pillar1 + Show dialog 3
        //             //TODO: Highlight _pillar2
        //             chosen = e.Pillar;
        //             Debug.Log($"Disable pillar {chosen.Id}");
        //             DisableHighlightPillar(chosen);
        //             Debug.Log($"Highlight pillar {(e.Pillar.Id == _pillar1.Id ? _pillar2.Id : _pillar1.Id)}");
        //             HighlightPillar(e.Pillar.Id == _pillar1.Id ? _pillar2 : _pillar1);
        //             if (dialogIndex <= dialog3Index)
        //             {
        //                 dialogIndex = dialog3Index;
        //                 StartCoroutine(doDialog());
        //             } 
        //         }
        //         //TODO: Wait for click
        //         else
        //         {
        //             if (chosen.Id == e.Pillar.Id)
        //             {
        //                 Debug.LogWarning("HUH?");
        //                 DisableHighlightPillar(chosen);
        //                 chosen = null;
        //                 HighlightPillar(_pillar1);
        //             }
        //             else
        //             {
        //                 other = e.Pillar;
        //                 DisableHighlightPillar(other);
        //             }
        //         }
        //     });

        //     IEnumerator doDialog(bool skipStop = false)
        //     {
        //         for(; dialogIndex < _dialogActions.Length; dialogIndex++)
        //         {
        //             Debug.Log($"Do dialog {dialogIndex}");
        //             var action = _dialogActions[dialogIndex];
        //             var clickCountAtLineStart = _clickCount;
        //             _visual.DisplayText(action.Message);

        //             yield return new WaitUntil(() => !_visual.IsDisplayingText || _clickCount > clickCountAtLineStart);

        //             if (_visual.IsDisplayingText)
        //             {
        //                 _visual.CompleteDisplayedText();
        //             }

        //             yield return new WaitUntil(() => !_visual.IsDisplayingText);
                    
        //             if (action.StopFlag)
        //             {
        //                 if (skipStop) dialogIndex++;
        //                 break;
        //             }

        //             var clickCountAtLineEnd = _clickCount;
        //             yield return new WaitUntil(() => _clickCount > clickCountAtLineEnd);
        //         }
        //     }

        //     //TODO: Show dialog 1
        //     yield return _visual.MoveNarrator(_characterPos[0]).WaitForCompletion();
        //     yield return doDialog(true);

        //     //TODO: Fade overlay + Change pillars' layer
        //     yield return _visual.MoveNarrator(_characterPos[1]).WaitForCompletion();
        //     yield return ChangePillarsLayer().WaitForCompletion();
        //     GameManager.Instance.SubcribeIngameEvent?.Invoke();

        //     //TODO: Show dialog 2 + Highlight _pillar1
        //     HighlightPillar(_pillar1);
        //     EventBus<PillarClickedEvent>.Subscribe(pillarClickBinding);
        //     yield return doDialog();

        //     //TODO: Wait for click
        //     dialog3Index = SkipStopPoint(dialogIndex);
        //     yield return new WaitUntil(() => chosen != null && other != null);
        //     _visual.MoveNarrator(_characterPos[2]);
        //     yield return new WaitForSeconds(_blockMoveDelay);
        //     ResetPillars();
        //     EventBus<PillarClickedEvent>.Unsubscribe(pillarClickBinding);

        //     //TODO: Show dialog 4
        //     dialogIndex = SkipStopPoint(dialogIndex);
        //     yield return doDialog();
        //     GameManager.Instance.UnsubcribeIngameEvent?.Invoke();

        //     //TODO: End
        //     End();
        // }

        private IEnumerator DoTutorial()
        {
            int dialogIndex = 0;
            EventBinding<PillarClickedEvent> pillarClickBinding;
            PillarController chosen = null, other = null;
            pillarClickBinding = new EventBinding<PillarClickedEvent>((e) =>
            {
                if (chosen == null)
                {
                    //TODO: Disable highlight _pillar1 + Show dialog 3
                    //TODO: Highlight _pillar2
                    chosen = e.Pillar;
                    // Debug.Log($"Pick up chosen: {chosen.Id}");
                    DisableHighlightPillar(chosen);
                    HighlightPillar(e.Pillar.Id == _pillar1.Id ? _pillar2 : _pillar1);
                    if (_dialogCoroutine == null)
                    {
                        _dialogCoroutine = StartCoroutine(StartDialogNextFrame(doDialog(4, 5)));
                    }
                }
                //TODO: Wait for click
                else
                {
                    if (chosen.Id == e.Pillar.Id)
                    {
                        Debug.LogWarning("HUH?");
                        DisableHighlightPillar(chosen.Id == _pillar1.Id ? _pillar2 : _pillar1);
                        chosen = null;
                        HighlightPillar(_pillar1);
                    }
                    else
                    {
                        other = e.Pillar;
                        DisableHighlightPillar(other);
                    }
                }
            });

            IEnumerator doDialog(int from, int to, string[] messagePack = default)
            {
                messagePack = messagePack == default ? _dialogActions.Select(da => da.Message).ToArray() : messagePack;
                dialogIndex = from == -1 ? dialogIndex : from;
                to = Mathf.Min(to, _dialogActions.Length - 1);

                for(int i = dialogIndex; i <= to; i++)
                {
                    Debug.Log($"Do dialog {i}");
                    var action = _dialogActions[i];
                    var clickCountAtLineStart = _clickCount;
                    _visual.DisplayText(action.Message);

                    yield return new WaitUntil(() => !_visual.IsDisplayingText || _clickCount > clickCountAtLineStart);

                    if (_visual.IsDisplayingText)
                    {
                        _visual.CompleteDisplayedText();
                    }

                    yield return new WaitUntil(() => !_visual.IsDisplayingText);

                    var clickCountAtLineEnd = _clickCount;
                    yield return new WaitUntil(() => _clickCount > clickCountAtLineEnd);
                }

                _dialogCoroutine = null;
            }

            IEnumerator StartDialogNextFrame(IEnumerator dialogRoutine)
            {
                yield return null;
                yield return dialogRoutine;
            }

            //TODO: Show dialog 1
            yield return _visual.MoveNarrator(_characterPos[0]).WaitForCompletion();
            yield return doDialog(0, 1);

            //TODO: Fade overlay + Change pillars' layer
            yield return _visual.MoveNarrator(_characterPos[1]).WaitForCompletion();
            yield return ChangePillarsLayer().WaitForCompletion();

            //TODO: Show dialog 2 + Highlight _pillar1
            HighlightPillar(_pillar1);
            yield return doDialog(2, 3);
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
            EventBus<PillarClickedEvent>.Subscribe(pillarClickBinding);

            //TODO: Wait for click
            yield return new WaitUntil(() => chosen != null && other != null);
            _visual.MoveNarrator(_characterPos[2]);
            yield return new WaitForSeconds(_blockMoveDelay);
            ResetPillars();
            EventBus<PillarClickedEvent>.Unsubscribe(pillarClickBinding);

            //TODO: Show dialog 4
            yield return doDialog(6, 7);
            GameManager.Instance.UnsubcribeIngameEvent?.Invoke();

            //TODO: End
            End();
        }

        public override void End()
        {
            // throw new System.NotImplementedException();
            BoardController.Instance.ClearBoard();
            IsFinished = true;
        }

        protected override void HandlingEvent(PlayerClickEvent @event)
        {
            _latestClickEvent = @event;
            RegisterPlayerClick(@event);
        }

        private Sequence ChangePillarsLayer()
        {
            _pillar1.GetComponent<PillarEffectVisual>().ChangeLayer(_targetLayer);
            _pillar2.GetComponent<PillarEffectVisual>().ChangeLayer(_targetLayer);
            var pillarTargetScale = _pillarBaseScale * _pillarNewScaleFactor;
            var sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);

            sequence.Append(PopupManager.Instance.ChangeOverlayOpacity(_overlayOpacity, _changeLayerDur, Ease.OutQuad));
            sequence.Join(_pillar1.transform.DOScale(pillarTargetScale, _changeLayerDur).SetEase(Ease.OutQuad));
            sequence.Join(_pillar2.transform.DOScale(pillarTargetScale, _changeLayerDur).SetEase(Ease.OutQuad));

            return sequence;
        }

        private Sequence HighlightPillar(PillarController pillar)
        {
            Debug.Log($"Highlight pillar {pillar.Id}");
            var pillarTargetScale = _pillarBaseScale * _pillarHighlightScaleFactor;
            DOTween.Kill(pillar, "Highlight");
            var sequence = DOTween.Sequence().SetTarget(pillar).SetId("Highlight").SetUpdate(true);

            sequence.Append(pillar.transform.DOScale(pillarTargetScale, _highlightTransitionDur).SetEase(Ease.InOutQuad).SetLoops(int.MaxValue, LoopType.Yoyo));
            sequence.OnKill(() => pillar.transform.localScale = _pillarBaseScale * _pillarNewScaleFactor);

            return sequence;
        }

        private Sequence DisableHighlightPillar(PillarController pillar)
        {
            var pillarTargetScale = _pillarBaseScale * _pillarNewScaleFactor;
            DOTween.Kill(pillar, "Highlight");
            var sequence = DOTween.Sequence().SetTarget(pillar).SetId("Highlight").SetUpdate(true);

            sequence.Append(pillar.transform.DOScale(pillarTargetScale, _disableHighlightTransitionDur).SetEase(Ease.OutBack));
            sequence.OnKill(() => pillar.transform.localScale = _pillarBaseScale * _pillarNewScaleFactor);

            return sequence;
        }

        private void ResetPillars()
        {
            void resetPillar(PillarController pillar)
            {
                DOTween.Kill(pillar, "Highlight");
                pillar.GetComponent<PillarEffectVisual>().ChangeLayer(_pillarBaseLayer);
                pillar.transform.DOScale(_pillarBaseScale, .3f).SetEase(Ease.OutQuad).SetTarget(pillar).SetId("Update").SetUpdate(true);
            }
            resetPillar(_pillar1);
            resetPillar(_pillar2);
        }
    }
}
