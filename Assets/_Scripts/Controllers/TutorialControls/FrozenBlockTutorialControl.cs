using System.Collections;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class FrozenBlockTutorialControl : BaseTutorialControl
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
        private Coroutine _endTutorialCoroutine;
        private TutorialStep _currentStep;

        private enum TutorialStep
        {
            None,
            WaitForAnyClickAfterDialog1,
            WaitForPillar2,
            WaitForPillar1AfterDialog3,
            WaitForAnyClickAfterDialog4,
            WaitForPillar3,
            WaitForPillar1AfterDialog6,
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

            _pillar3 = pillars.FirstOrDefault(p => p.Id == _pillar3Id);
            if (_pillar3 == null)
            {
                Debug.LogError($"Cant find pillar with id {_pillar3Id}");
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
            if (!handledDialogClick)
            {
                switch (_currentStep)
                {
                    case TutorialStep.WaitForAnyClickAfterDialog1:
                        StartPillar2Step();
                        break;

                    case TutorialStep.WaitForAnyClickAfterDialog4:
                        StartPillar3Step();
                        break;
                }
            }

            RegisterPlayerClick(@event);
        }

        private IEnumerator DoTutorial()
        {
            _currentStep = TutorialStep.WaitForAnyClickAfterDialog1;
            ChangePillarLayer(_pillar3, _pillar3BaseLayer);
            ChangePillarLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarLayer(_pillar1, _targetLayer);
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
                case TutorialStep.WaitForPillar2 when clickedPillar == _pillar2:
                    DisableGameplayPillarInteraction();
                    PlayDialog(2, EnablePillar1AfterDialog3Interaction);
                    break;

                case TutorialStep.WaitForPillar1AfterDialog3 when clickedPillar == _pillar1:
                    DisableGameplayPillarInteraction();
                    PlayDialog(3);
                    _currentStep = TutorialStep.WaitForAnyClickAfterDialog4;
                    break;

                case TutorialStep.WaitForPillar3 when clickedPillar == _pillar3:
                    DisableGameplayPillarInteraction();
                    PlayDialog(5, EnablePillar1AfterDialog6Interaction);
                    break;

                case TutorialStep.WaitForPillar1AfterDialog6 when clickedPillar == _pillar1:
                    DisableGameplayPillarInteraction();
                    _visual.StopPointing();
                    _currentStep = TutorialStep.Ending;
                    _endTutorialCoroutine ??= StartCoroutine(WaitAndEndTutorial());
                    break;
            }
        }

        private void StartPillar2Step()
        {
            DisableGameplayPillarInteraction();
            // ChangePillarLayer(_pillar1, _pillar1BaseLayer);
            ChangePillarLayer(_pillar2, _targetLayer);
            _currentStep = TutorialStep.WaitForPillar2;
            PlayDialog(1, EnablePillar2Interaction);
        }

        private void StartPillar3Step()
        {
            DisableGameplayPillarInteraction();
            // ChangePillarLayer(_pillar1, _pillar1BaseLayer);
            ChangePillarLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarLayer(_pillar3, _targetLayer);
            _currentStep = TutorialStep.WaitForPillar3;
            PlayDialog(4, EnablePillar3Interaction);
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

        private void EnablePillar1AfterDialog3Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar2)
            {
                return;
            }

            // ChangePillarLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarLayer(_pillar1, _targetLayer);
            _currentStep = TutorialStep.WaitForPillar1AfterDialog3;
            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar1 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnablePillar3Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar3)
            {
                return;
            }

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new[] { _pillar3 });
            GameManager.Instance.SubcribeIngameEvent?.Invoke();
        }

        private void EnablePillar1AfterDialog6Interaction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForPillar3)
            {
                return;
            }

            // ChangePillarLayer(_pillar3, _pillar3BaseLayer);
            ChangePillarLayer(_pillar1, _targetLayer);
            _currentStep = TutorialStep.WaitForPillar1AfterDialog6;
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
            _endTutorialCoroutine = null;
            End();
        }

        private void RestorePillarState()
        {
            if (_pillar1 != null)
            {
                ChangePillarLayer(_pillar1, _pillar1BaseLayer);
            }

            if (_pillar2 != null)
            {
                ChangePillarLayer(_pillar2, _pillar2BaseLayer);
            }

            if (_pillar3 != null)
            {
                ChangePillarLayer(_pillar3, _pillar3BaseLayer);
            }
        }

        private void ChangePillarLayer(PillarController pillar, LayerMask layer)
        {
            pillar.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }

        private void ChangePillarLayer(PillarController pillar, int layer)
        {
            pillar.GetComponent<PillarEffectVisual>().ChangeLayer(layer);
        }
    }
}
