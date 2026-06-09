using System.Collections;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class HiddenBlockTutorialControl : BaseTutorialControl
    {
        [SerializeField] private Vector2 _characterPos;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private int _pillar1Id;
        [SerializeField] private int _pillar2Id;
        [SerializeField] private float _endDelay;

        private PillarController _pillar1;
        private PillarController _pillar2;
        private int _pillar1BaseLayer;
        private int _pillar2BaseLayer;

        private EventBinding<PillarClickedEvent> _pillarClickedBinding;
        private Coroutine _endTutorialCoroutine;
        private TutorialStep _currentStep;

        private enum TutorialStep
        {
            None,
            WaitForAnyClick,
            WaitForPillar1,
            WaitForPillar2,
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

            _pillar2 = pillars.FirstOrDefault(p => p.Id == _pillar2Id);
            if (_pillar2 == null)
            {
                Debug.LogError($"Cant find pillar with id {_pillar2Id}");
                return;
            }

            _pillar1BaseLayer = _pillar1.gameObject.layer;
            _pillar2BaseLayer = _pillar2.gameObject.layer;
            MoveNarratorToTutorialTarget(_characterPos);

            DisableGameplayPillarInteraction();
            StartCoroutine(DoTutorial());
        }

        public override void End()
        {
            if (_endTutorialCoroutine != null)
            {
                StopCoroutine(_endTutorialCoroutine);
                _endTutorialCoroutine = null;
            }

            RestorePillarState();
            EnableAllGameplayPillarInteraction();
            _visual.StopPointing();
            _currentStep = TutorialStep.None;
            // BoardController.Instance.ClearBoard();
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

            if (_endTutorialCoroutine != null)
            {
                StopCoroutine(_endTutorialCoroutine);
                _endTutorialCoroutine = null;
            }

            base.OnDisable();
        }

        protected override void HandlingEvent(PlayerClickEvent @event)
        {
            var handledDialogClick = TryHandleDialogClick();
            if (!handledDialogClick && _currentStep == TutorialStep.WaitForAnyClick)
            {
                StartPillar1Step();
            }

            RegisterPlayerClick(@event);
        }

        private IEnumerator DoTutorial()
        {
            _currentStep = TutorialStep.WaitForAnyClick;
            ChangePillarsLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarsLayer(_pillar1, _targetLayer);
            PlayDialog(0);
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
                case TutorialStep.WaitForPillar1 when clickedPillar == _pillar1:
                    DisableGameplayPillarInteraction();
                    // ChangePillarsLayer(_pillar1, _pillar1BaseLayer);
                    ChangePillarsLayer(_pillar2, _targetLayer);
                    _currentStep = TutorialStep.WaitForPillar2;
                    PlayDialog(2, EnablePillar2Interaction);
                    break;

                case TutorialStep.WaitForPillar2 when clickedPillar == _pillar2:
                    DisableGameplayPillarInteraction();
                    _visual.StopPointing();
                    _currentStep = TutorialStep.Ending;
                    _endTutorialCoroutine ??= StartCoroutine(WaitAndEndTutorial());
                    break;
            }
        }

        private void StartPillar1Step()
        {
            DisableGameplayPillarInteraction();
            _currentStep = TutorialStep.WaitForPillar1;
            PlayDialog(1, EnablePillar1Interaction);
        }

        private void EnablePillar1Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar1)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar1 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnablePillar2Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar2)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar2 });
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
            _endTutorialCoroutine = null;
            End();
        }

        private void RestorePillarState()
        {
            if (_pillar1 != null)
            {
                ChangePillarsLayer(_pillar1, _pillar1BaseLayer);
            }

            if (_pillar2 != null)
            {
                ChangePillarsLayer(_pillar2, _pillar2BaseLayer);
            }
        }

        private void ChangePillarsLayer(PillarController pillar, LayerMask layer)
        {
            pillar.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }

        private void ChangePillarsLayer(PillarController pillar, int layer)
        {
            pillar.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }
    }
}
