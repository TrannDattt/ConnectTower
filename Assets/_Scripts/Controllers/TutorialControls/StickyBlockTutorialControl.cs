using System.Collections;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class StickyBlockTutorialControl : BaseTutorialControl
    {
        [SerializeField] private Vector2 _characterPos;

        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private int _pillar1Id;
        [SerializeField] private int _pillar2Id;
        [SerializeField] private int _pillar3Id;
        [SerializeField] private float _endDelay;

        private PillarController _pillar1;
        private PillarController _pillar2;
        private PillarController _pillar3;

        private int _pillar1BaseLayer;
        private int _pillar2BaseLayer;
        private int _pillar3BaseLayer;

        private EventBinding<PillarClickedEvent> _pillarClickedBinding;
        private Coroutine _finishTutorialCoroutine;
        private TutorialStep _currentStep;

        private enum TutorialStep
        {
            None,
            WaitForPillar1Click,
            WaitForPillar2PlaceClick,
            WaitForPillar2PickupClick,
            WaitForPillar3Click,
            Ending,
        }

        public override void Begin()
        {
            var pillars = BoardController.Instance.GetAllPillars();
            _pillar1 = pillars.FirstOrDefault(p => p.Id == _pillar1Id);
            _pillar2 = pillars.FirstOrDefault(p => p.Id == _pillar2Id);
            _pillar3 = pillars.FirstOrDefault(p => p.Id == _pillar3Id);

            if (_pillar1 == null || _pillar2 == null || _pillar3 == null)
            {
                Debug.LogError($"Cant find sticky tutorial pillars: {_pillar1Id}, {_pillar2Id}, {_pillar3Id}");
                return;
            }

            _pillar1BaseLayer = _pillar1.gameObject.layer;
            _pillar2BaseLayer = _pillar2.gameObject.layer;
            _pillar3BaseLayer = _pillar3.gameObject.layer;

            MoveNarratorToTutorialTarget(_characterPos);

            DisableGameplayPillarInteraction();
            StartCoroutine(DoTutorial());
        }

        public override void End()
        {
            if (_finishTutorialCoroutine != null)
            {
                StopCoroutine(_finishTutorialCoroutine);
                _finishTutorialCoroutine = null;
            }

            RestorePillarStates();
            EnableAllGameplayPillarInteraction();
            _visual.StopPointing();
            _currentStep = TutorialStep.None;
            IsFinished = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentStep = TutorialStep.None;
            _pillarClickedBinding = new(OnPillarClicked);
            EventBus<PillarClickedEvent>.Subscribe(_pillarClickedBinding);
        }

        protected override void OnDisable()
        {
            EventBus<PillarClickedEvent>.Unsubscribe(_pillarClickedBinding);
            RestorePillarStates();

            if (_finishTutorialCoroutine != null)
            {
                StopCoroutine(_finishTutorialCoroutine);
                _finishTutorialCoroutine = null;
            }

            base.OnDisable();
        }

        protected override void HandlingEvent(PlayerClickEvent @event)
        {
            TryHandleDialogClick();
            RegisterPlayerClick(@event);
        }

        private IEnumerator DoTutorial()
        {
            _currentStep = TutorialStep.WaitForPillar1Click;
            ChangePillarLayer(_pillar1, _pillar1BaseLayer);
            ChangePillarLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarLayer(_pillar3, _pillar3BaseLayer);
            ChangePillarLayer(_pillar1, _targetLayer);
            PlayDialog(0, EnablePillar1Interaction);
            yield break;
        }

        private void OnPillarClicked(PillarClickedEvent @event)
        {
            if (IsFinished || @event.Pillar == null)
            {
                return;
            }

            TryHandleDialogClick(() => HandleTutorialPillarClick(@event.Pillar));
        }

        private void HandleTutorialPillarClick(PillarController clickedPillar)
        {
            switch (_currentStep)
            {
                case TutorialStep.WaitForPillar1Click when clickedPillar == _pillar1:
                    DisableGameplayPillarInteraction();
                    _currentStep = TutorialStep.WaitForPillar2PlaceClick;
                    // HighlightOnly(_pillar2);
                    ChangePillarLayer(_pillar2, _targetLayer);
                    PlayDialog(1, EnablePillar2PlaceInteraction);
                    break;

                case TutorialStep.WaitForPillar2PlaceClick when clickedPillar == _pillar2:
                    DisableGameplayPillarInteraction();
                    _currentStep = TutorialStep.WaitForPillar2PickupClick;
                    ChangePillarLayer(_pillar1, _pillar1BaseLayer);
                    ChangePillarLayer(_pillar3, _targetLayer);
                    PlayDialog(2, EnablePillar2PickupInteraction);
                    break;

                case TutorialStep.WaitForPillar2PickupClick when clickedPillar == _pillar2:
                    DisableGameplayPillarInteraction();
                    _currentStep = TutorialStep.WaitForPillar3Click;
                    PlayDialog(3, EnablePillar3Interaction);
                    break;

                case TutorialStep.WaitForPillar3Click when clickedPillar == _pillar3:
                    DisableGameplayPillarInteraction();
                    _currentStep = TutorialStep.Ending;
                    _finishTutorialCoroutine ??= StartCoroutine(WaitAndEndTutorial());
                    break;
            }
        }

        private void EnablePillar1Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar1Click)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar1 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnablePillar2PlaceInteraction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar2PlaceClick)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar2 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnablePillar2PickupInteraction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar2PickupClick)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar2 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnablePillar3Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar3Click)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar3 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnableAllGameplayPillarInteraction()
        {
            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new PillarController[0]);
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void DisableGameplayPillarInteraction()
        {
            GameManager.Instance.UnsubcribeIngameEvent?.Invoke();
        }

        private IEnumerator WaitAndEndTutorial()
        {
            yield return new WaitForSeconds(_endDelay);
            _finishTutorialCoroutine = null;
            End();
        }

        private void RestorePillarStates()
        {
            ChangePillarLayer(_pillar1, _pillar1BaseLayer);
            ChangePillarLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarLayer(_pillar3, _pillar3BaseLayer);
        }

        private void ChangePillarLayer(PillarController pillar, LayerMask layer)
        {
            if (pillar == null)
            {
                return;
            }

            pillar.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }

        private void ChangePillarLayer(PillarController pillar, int layer)
        {
            if (pillar == null)
            {
                return;
            }

            pillar.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }
    }
}
