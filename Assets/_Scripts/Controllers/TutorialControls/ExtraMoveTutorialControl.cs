using System.Collections;
using System.Linq;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers.Tutorials
{
    public class ExtraMoveTutorialControl : BaseTutorialControl
    {
        [SerializeField] private Vector2 _characterPos;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private float _endDelay;
        [SerializeField] private MoveCountVisual _moveCounter;
        [SerializeField] private BoosterButtonVisual _extraMoveButton;

        private int _moveCounterBaseLayer;
        private int _extraMoveButtonBaseLayer;
        private Coroutine _endTutorialCoroutine;
        private TutorialStep _currentStep;
        private bool _canHandleExtraMoveButtonClick;

        private enum TutorialStep
        {
            None,
            WaitForAnyClick,
            WaitForExtraMoveButton,
            Ending,
        }

        public override void Begin()
        {
            if (!ResolveReferences())
            {
                return;
            }

            _moveCounterBaseLayer = _moveCounter.gameObject.layer;
            _extraMoveButtonBaseLayer = _extraMoveButton.gameObject.layer;
            MoveNarratorToTutorialTarget(_characterPos);

            _extraMoveButton.OnClicked.RemoveListener(OnExtraMoveButtonClicked);
            // _extraMoveButton.OnClicked.AddListener(OnExtraMoveButtonClicked);

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

            if (_extraMoveButton != null)
            {
                _extraMoveButton.OnClicked.RemoveListener(OnExtraMoveButtonClicked);
            }

            RestoreUiState();
            EnableAllGameplayPillarInteraction();
            _visual.StopPointing();
            _canHandleExtraMoveButtonClick = false;
            _currentStep = TutorialStep.None;
            BoardController.Instance.ClearBoard();
            IsFinished = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _canHandleExtraMoveButtonClick = false;
            _currentStep = TutorialStep.None;
        }

        protected override void OnDisable()
        {
            if (_extraMoveButton != null)
            {
                _extraMoveButton.OnClicked.RemoveListener(OnExtraMoveButtonClicked);
            }

            RestoreUiState();

            if (_endTutorialCoroutine != null)
            {
                StopCoroutine(_endTutorialCoroutine);
                _endTutorialCoroutine = null;
            }

            _canHandleExtraMoveButtonClick = false;
            base.OnDisable();
        }

        protected override void HandlingEvent(PlayerClickEvent @event)
        {
            var handledDialogClick = TryHandleDialogClick();
            if (!handledDialogClick && _currentStep == TutorialStep.WaitForAnyClick)
            {
                StartExtraMoveStep();
            }

            RegisterPlayerClick(@event);
        }

        private IEnumerator DoTutorial()
        {
            _currentStep = TutorialStep.WaitForAnyClick;
            _canHandleExtraMoveButtonClick = false;
            ChangeLayer(_extraMoveButton.gameObject, _extraMoveButtonBaseLayer);
            ChangeLayer(_moveCounter.gameObject, _targetLayer);
            PlayDialog(0);
            yield break;
        }

        private void StartExtraMoveStep()
        {
            ChangeLayer(_moveCounter.gameObject, _moveCounterBaseLayer);
            ChangeLayer(_extraMoveButton.gameObject, _targetLayer);
            _currentStep = TutorialStep.WaitForExtraMoveButton;
            PlayDialog(1, EnableExtraMoveButtonInteraction);
        }

        private void EnableExtraMoveButtonInteraction()
        {
            if (IsFinished || _currentStep != TutorialStep.WaitForExtraMoveButton)
            {
                return;
            }

            _canHandleExtraMoveButtonClick = true;
        }

        private void OnExtraMoveButtonClicked()
        {
            if (IsFinished || !_canHandleExtraMoveButtonClick || _currentStep != TutorialStep.WaitForExtraMoveButton)
            {
                return;
            }

            _canHandleExtraMoveButtonClick = false;
            _currentStep = TutorialStep.Ending;
            _endTutorialCoroutine ??= StartCoroutine(WaitAndEndTutorial());
        }

        private IEnumerator WaitAndEndTutorial()
        {
            yield return new WaitForSeconds(_endDelay);
            _endTutorialCoroutine = null;
            End();
        }

        private void RestoreUiState()
        {
            if (_moveCounter != null)
            {
                ChangeLayer(_moveCounter.gameObject, _moveCounterBaseLayer);
            }

            if (_extraMoveButton != null)
            {
                ChangeLayer(_extraMoveButton.gameObject, _extraMoveButtonBaseLayer);
            }
        }

        private bool ResolveReferences()
        {
            _moveCounter ??= Object.FindFirstObjectByType<MoveCountVisual>(FindObjectsInactive.Include);

            if (_extraMoveButton == null)
            {
                _extraMoveButton = Object.FindObjectsByType<BoosterButtonVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                                         .FirstOrDefault(button => button.BoosterKey == EBooster.ExtraMove);
            }

            if (_moveCounter == null)
            {
                Debug.LogError("Cant find MoveCountVisual for ExtraMove tutorial");
                return false;
            }

            if (_extraMoveButton == null)
            {
                Debug.LogError("Cant find ExtraMove button for ExtraMove tutorial");
                return false;
            }

            return true;
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

        private void ChangeLayer(GameObject target, LayerMask layer)
        {
            ChangeLayer(target, LayerMaskToLayerIndex(layer));
        }

        private void ChangeLayer(GameObject target, int layer)
        {
            if (target == null || layer < 0 || layer > 31)
            {
                return;
            }

            SetLayerRecursively(target.transform, layer);
        }

        private static void SetLayerRecursively(Transform target, int layer)
        {
            target.gameObject.layer = layer;
            foreach (Transform child in target)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private static int LayerMaskToLayerIndex(LayerMask mask)
        {
            var value = mask.value;
            if (value <= 0 || (value & (value - 1)) != 0)
            {
                return -1;
            }

            var index = 0;
            while (value > 1)
            {
                value >>= 1;
                index++;
            }

            return index;
        }
    }
}
