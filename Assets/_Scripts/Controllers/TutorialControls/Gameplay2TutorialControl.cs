using System.Collections;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class Gameplay2TutorialControl : BaseTutorialControl
    {
        [SerializeField] private Vector2 _characterPos;

        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private int _pillar1Id;
        [SerializeField] private float _endDelay;

        private PillarController _pillar1;
        private int _pillar1BaseLayer;

        private EventBinding<PillarClickedEvent> _pillarClickedBinding;
        private Coroutine _finishTutorialCoroutine;
        private TutorialStep _currentStep;

        private enum TutorialStep
        {
            None,
            WaitForFirstClick,
            WaitForSecondClick,
            Ending,
        }

        public override void Begin()
        {
            var pillars = BoardController.Instance.GetAllPillars();
            _pillar1 = pillars.FirstOrDefault(p => p.Id == _pillar1Id);
            if (_pillar1 == null)
            {
                Debug.LogError($"Cant find pillar with id {_pillar1Id}");
                return;
            }

            _pillar1BaseLayer = _pillar1.gameObject.layer;
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

            RestorePillarState();
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
            RestorePillarState();

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
            _currentStep = TutorialStep.WaitForFirstClick;
            ChangePillarLayer(_targetLayer);
            PlayDialog(0, EnableFirstPillarInteraction);
            yield break;
        }

        private void OnPillarClicked(PillarClickedEvent @event)
        {
            if (IsFinished || @event.Pillar != _pillar1)
            {
                return;
            }

            TryHandleDialogClick(() => HandleTutorialPillarClick(@event.Pillar));
        }

        private void HandleTutorialPillarClick(PillarController clickedPillar)
        {
            switch (_currentStep)
            {
                case TutorialStep.WaitForFirstClick when clickedPillar == _pillar1:
                    DisableGameplayPillarInteraction();
                    _currentStep = TutorialStep.WaitForSecondClick;
                    PlayDialog(1, EnableSecondPillarInteraction);
                    break;

                case TutorialStep.WaitForSecondClick when clickedPillar == _pillar1:
                    DisableGameplayPillarInteraction();
                    _currentStep = TutorialStep.Ending;
                    _finishTutorialCoroutine ??= StartCoroutine(WaitAndEndTutorial());
                    break;
            }
        }

        private void EnableFirstPillarInteraction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForFirstClick)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar1 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnableSecondPillarInteraction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForSecondClick)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar1 });
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

        private void RestorePillarState()
        {
            if (_pillar1 != null)
            {
                ChangePillarLayer(_pillar1BaseLayer);
            }
        }

        private void ChangePillarLayer(LayerMask layer)
        {
            _pillar1.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }

        private void ChangePillarLayer(int layer)
        {
            _pillar1.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }
    }
}
