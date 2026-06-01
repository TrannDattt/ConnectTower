using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Assets._Scripts.Visuals.BoosterButtonVisual;

namespace Assets._Scripts.Visuals
{
    public class AddPillarEffectVisual : BoosterButtonEffectVisual
    {
        [Header("Add Pillar")]
        [SerializeField] private float _pillarMoveDelay = .05f;
        [SerializeField] private float _repositionDur = .5f;
        [SerializeField] private float _pillarFallDelay = .2f;
        [SerializeField] private float _offsetY;
        [SerializeField] private float _pillarScaleDur = .3f;
        [SerializeField] private float _pillarFallDur = .6f;
        [SerializeField] private AnimationCurve _pillarMoveCurve;
        [SerializeField] private AnimationCurve _pillarScaleCurve;
        [SerializeField] private Vector3 _baseTiltAngle = new (-20, 0, 0);
        [SerializeField] private float _baseSpinCycle = 1f;
        [SerializeField] private float _baseRotateDur = .4f;
        [SerializeField] private float _baseRotationResetDur = .1f;
        [SerializeField] private AudioClip _fxPillarPop;
        [SerializeField] private AudioClip _fxPillarFall;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _portalHolder;
        [SerializeField] private Image _portalIcon;
        [SerializeField] private Vector3 _portalOffset;
        [SerializeField] private AnimationCurve _portalScaleCurve;
        [SerializeField] private AnimationCurve _portalDisappearCurve;
        [SerializeField] private float _portalRotationCycleDur;
        [SerializeField] private float _portalScaleDur = .5f;
        [SerializeField] private float _pillarSpawnDelay = .1f;
        [SerializeField] private float _glowMoveDur = .5f;
        [SerializeField] private float _glowOffsetY = 100f;
        [SerializeField] private Image _glowImage;

        private Transform _portalOriginalParent;
        private int _portalOriginalSiblingIndex;

        private static Camera GetCanvasCamera(Canvas canvas)
        {
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        private void SetPortalPosition(Vector3 worldPosition)
        {
            if (_canvas == null || _portalHolder == null || _portalIcon == null)
            {
                return;
            }

            var rootCanvas = _canvas.rootCanvas;
            var rootCanvasRect = rootCanvas.transform as RectTransform;
            if (rootCanvasRect == null)
            {
                return;
            }

            if (_portalOriginalParent == null)
            {
                _portalOriginalParent = _portalHolder.parent;
                _portalOriginalSiblingIndex = _portalHolder.GetSiblingIndex();
            }

            if (_portalHolder.parent != rootCanvasRect)
            {
                _portalHolder.SetParent(rootCanvasRect, false);
                _portalHolder.localScale = Vector3.one;
                _portalHolder.localRotation = Quaternion.identity;
            }

            Camera canvasCamera = GetCanvasCamera(rootCanvas);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPosition);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, screenPos, canvasCamera, out Vector2 localPos))
            {
                _portalHolder.anchoredPosition = localPos;
                _portalIcon.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        // public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        // {
        //     var addPillarData = data as AddPillarRuntimeData;
        //     var pillar = addPillarData.NewPillar;
        //     var allPillars = BoardController.Instance.GetAllPillars();
        //     var newCount = allPillars.Count;

        //     // Get new positions for all pillars
        //     var allPositions = SlotLayoutManager.Instance.GetPillarPositions(newCount, BoardController.Instance.BoardTransform);

        //     var sequence = DOTween.Sequence();
            
        //     // Move existing pillars to their new positions
        //     for (int i = 0; i < allPillars.Count; i++)
        //     {
        //         if (allPillars[i] == pillar) continue;
        //         sequence.Insert(_pillarMoveDelay * i, allPillars[i].transform.DOMove(allPositions[i], _repositionDur).SetEase(Ease.OutQuad));
        //     }

        //     // Prepare the new pillar
        //     int pillarIndex = allPillars.IndexOf(pillar);
        //     if (pillarIndex != -1)
        //     {
        //         Vector3 targetPos = allPositions[pillarIndex];
        //         pillar.transform.position = targetPos + Vector3.up * _offsetY;
        //         pillar.transform.localScale = Vector3.zero;
        //         pillar.Base.localRotation = Quaternion.Euler(_baseTiltAngle); // Tilt it a bit
                
        //         // Animation for new pillar
        //         sequence.AppendInterval(_pillarFallDelay); // Small delay before falling
        //         sequence.Append(pillar.transform.DOScale(Vector3.one, _pillarScaleDur).SetEase(Ease.OutBack));
        //         sequence.Join(pillar.transform.DOMove(targetPos, _pillarFallDur).SetEase(_pillarMoveCurve));
                
        //         var baseRotateSequece = DOTween.Sequence();
        //         baseRotateSequece.Append(pillar.Base.DOBlendableLocalRotateBy(_baseTiltAngle * -1, _baseRotateDur).SetEase(Ease.InQuad));
        //         baseRotateSequece.Join(pillar.Base.DOBlendableLocalRotateBy(new Vector3(0, 360 * _baseSpinCycle, 0), _baseRotateDur, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
        //         baseRotateSequece.Append(pillar.Base.DOLocalRotate(new Vector3(0, pillar.Base.rotation.y - (0 - 360 * _baseSpinCycle % 360), 0), _baseRotationResetDur).SetEase(Ease.OutQuad));
                
        //         sequence.Append(baseRotateSequece);
        //         sequence.Join(pillar.transform.DOPunchPosition(Vector3.up * 0.2f, _baseRotateDur, 2, 0.5f));
        //     }

        //     // sequence.OnComplete(() => pillar.Base.localRotation = Quaternion.identity);

        //     return sequence;
        // }

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            var addPillarData = data as AddPillarRuntimeData;
            var pillar = addPillarData.NewPillar;
            var allPillars = BoardController.Instance.GetAllPillars();
            var newCount = allPillars.Count;
            var baseScale = pillar.Base.localScale;

            // Get new positions for all pillars
            var allPositions = SlotLayoutManager.Instance.GetPillarPositions(newCount, BoardController.Instance.BoardTransform);
                
            int pillarIndex = allPillars.IndexOf(pillar);
            Vector3 targetPos = allPositions[pillarIndex];
            pillar.transform.position = targetPos;
            SetPortalPosition(pillar.PortalBase.position);
            pillar.Base.localScale = Vector3.zero;
            _portalHolder.gameObject.SetActive(true);
            _portalIcon.transform.localScale = Vector3.zero;

            void reset()
            {
                _portalIcon.transform.localScale = Vector3.zero;
                _glowImage.transform.localPosition = Vector3.one * -_glowOffsetY;

                if (_portalOriginalParent != null)
                {
                    _portalHolder.SetParent(_portalOriginalParent, false);
                    _portalHolder.SetSiblingIndex(_portalOriginalSiblingIndex);
                }

                _portalHolder.anchoredPosition = Vector2.zero;
                _portalHolder.gameObject.SetActive(false);

                pillar.Base.localScale = baseScale;
                pillar.Base.localRotation = Quaternion.identity;
                pillar.transform.localRotation = Quaternion.identity;
                pillar.transform.position = targetPos;
            }

            var sequence = DOTween.Sequence();
            
            // Move existing pillars to their new positions
            for (int i = 0; i < allPillars.Count; i++)
            {
                if (allPillars[i] == pillar) continue;
                sequence.Insert(_pillarMoveDelay * i, allPillars[i].transform.DOMove(allPositions[i], _repositionDur).SetEase(Ease.OutQuad));
            }

            // Open portal
            sequence.AppendCallback(() =>
            {
                _portalIcon.transform.DOLocalRotate(new Vector3(0, 0, -360), _portalRotationCycleDur, RotateMode.FastBeyond360)
                                     .SetLoops(int.MaxValue, LoopType.Restart)
                                     .SetEase(Ease.Linear)
                                     .SetRelative(true)
                                     .SetLink(_portalIcon.gameObject, LinkBehaviour.KillOnDisable);
            });
            sequence.Append(_portalIcon.transform.DOScale(Vector3.one, _portalScaleDur).SetEase(_portalScaleCurve));
            sequence.Join(_glowImage.transform.DOMoveY(_glowOffsetY, _glowMoveDur).SetEase(Ease.OutQuad));
            sequence.Join(_glowImage.DOFade(0f, _glowMoveDur).SetEase(Ease.OutQuad));

            sequence.AppendInterval(_pillarSpawnDelay);

            sequence.AppendCallback(() =>
            {
                SoundManager.Instance.PlaySFX(_fxPillarPop);
            });
            sequence.Append(pillar.transform.DOMoveY(_offsetY, _pillarFallDur).SetEase(_pillarMoveCurve).SetRelative());
            sequence.Join(pillar.Base.DOScale(baseScale, _pillarScaleDur).SetEase(_pillarScaleCurve));
            // sequence.Join(pillar.transform.DOLocalRotate(_baseTiltAngle, _pillarFallDur));
            sequence.Join(pillar.Base.DOLocalRotate(_baseTiltAngle, _pillarFallDur));
            sequence.AppendCallback(() =>
            {
                SoundManager.Instance.PlaySFX(_fxPillarFall);
            });

            sequence.Append(_portalIcon.transform.DOScale(Vector3.zero, _portalScaleDur).SetEase(_portalDisappearCurve));

            var baseRotateSequece = DOTween.Sequence();
            // baseRotateSequece.Append(pillar.transform.DOLocalRotate(_baseTiltAngle * -1, _baseRotateDur).SetEase(Ease.InQuad));
            baseRotateSequece.Append(pillar.Base.DOBlendableLocalRotateBy(_baseTiltAngle * -1, _baseRotateDur).SetEase(Ease.InQuad));
            // baseRotateSequece.Join(pillar.Base.DORotate(new Vector3(0, 360 * _baseSpinCycle, 0), _baseRotateDur, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
            baseRotateSequece.Join(pillar.Base.DOBlendableLocalRotateBy(new Vector3(0, 360 * _baseSpinCycle, 0), _baseRotateDur, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
            baseRotateSequece.Append(pillar.Base.DOLocalRotate(new Vector3(0, pillar.Base.rotation.eulerAngles.y - (0 - 360 * _baseSpinCycle % 360), 0), _baseRotationResetDur).SetEase(Ease.OutQuad));
            sequence.Join(baseRotateSequece);

            sequence.OnComplete(reset).OnKill(reset);

            return sequence;
        }
    }
}
