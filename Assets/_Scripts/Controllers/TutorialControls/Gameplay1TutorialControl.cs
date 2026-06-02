using System.Collections;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class Gameplay1TutorialControl : BaseTutorialControl
    {
        // Description: Tutorial for level 1: Show player how to move block from pillar1 to pillar2
        // CHARACTER
        [SerializeField] private Vector2 _characterPos;

        // CHANGE LAYER
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private float _pillarNewScaleFactor;

        [SerializeField] private int _pillar1Id; // Pillar with 3 blocks
        [SerializeField] private int _pillar2Id; // Other pillar

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
            _visual.MoveNarrator(_characterPos);

            GameManager.Instance.SetInteractablePillarsEvent?.Invoke(new PillarController[] {_pillar1, _pillar2});
            GameManager.Instance.UnsubcribeIngameEvent?.Invoke();

            StartCoroutine(DoTutorial());
        }

        private IEnumerator DoTutorial()
        {
            _currentStep = TutorialStep.WaitForPillar1;
            ChangePillarsLayer(_pillar2, _pillar2BaseLayer);
            ChangePillarsLayer(_pillar1, _targetLayer);

            PlayDialog(0);

            yield break;
        }

        public override void End()
        {
            if (_endTutorialCoroutine != null)
            {
                StopCoroutine(_endTutorialCoroutine);
                _endTutorialCoroutine = null;
            }

            RestorePillarState();
            _currentStep = TutorialStep.None;
            BoardController.Instance.ClearBoard();
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
            TryHandleDialogClick();
            RegisterPlayerClick(@event);
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
                    // ChangePillarsLayer(_pillar1, _pillar1BaseLayer);
                    ChangePillarsLayer(_pillar2, _targetLayer);
                    _currentStep = TutorialStep.WaitForPillar2;
                    PlayDialog(1);
                    break;

                case TutorialStep.WaitForPillar2 when clickedPillar == _pillar2:
                    // ChangePillarsLayer(_pillar2, _pillar2BaseLayer);
                    _currentStep = TutorialStep.Ending;
                    _endTutorialCoroutine ??= StartCoroutine(WaitAndEndTutorial());
                    break;
            }
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
