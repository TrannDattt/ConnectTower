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
        [SerializeField] private float _repositionDur = .5f;
        [SerializeField] private float _pillarFallDelay = .2f;
        [SerializeField] private float _offsetY;
        [SerializeField] private float _pillarScaleDur = .3f;
        [SerializeField] private float _pillarFallDur = .6f;
        [SerializeField] private AnimationCurve _pillarMoveCurve;
        [SerializeField] private Vector3 _baseTiltAngle = new (-20, 0, 0);
        [SerializeField] private float _baseSpinCycle = 1f;
        [SerializeField] private float _baseRotateDur = .4f;
        [SerializeField] private float _baseRotationResetDur = .1f;

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            var addPillarData = data as AddPillarRuntimeData;
            var pillar = addPillarData.NewPillar;
            var allPillars = BoardController.Instance.GetAllPillars();
            var newCount = allPillars.Count;

            // Get new positions for all pillars
            var allPositions = SlotLayoutManager.Instance.GetPillarPositions(newCount, BoardController.Instance.BoardTransform);

            var sequence = DOTween.Sequence();
            
            // Move existing pillars to their new positions
            for (int i = 0; i < allPillars.Count; i++)
            {
                if (allPillars[i] == pillar) continue;
                sequence.Join(allPillars[i].transform.DOMove(allPositions[i], _repositionDur).SetEase(Ease.OutQuad));
            }

            // Prepare the new pillar
            int pillarIndex = allPillars.IndexOf(pillar);
            if (pillarIndex != -1)
            {
                Vector3 targetPos = allPositions[pillarIndex];
                pillar.transform.position = targetPos + Vector3.up * _offsetY;
                pillar.transform.localScale = Vector3.zero;
                pillar.Base.localRotation = Quaternion.Euler(_baseTiltAngle); // Tilt it a bit
                
                // Animation for new pillar
                sequence.AppendInterval(_pillarFallDelay); // Small delay before falling
                sequence.Append(pillar.transform.DOScale(Vector3.one, _pillarScaleDur).SetEase(Ease.OutBack));
                sequence.Join(pillar.transform.DOMove(targetPos, _pillarFallDur).SetEase(_pillarMoveCurve));
                
                var baseRotateSequece = DOTween.Sequence();
                baseRotateSequece.Append(pillar.Base.DOBlendableLocalRotateBy(_baseTiltAngle * -1, _baseRotateDur).SetEase(Ease.InQuad));
                baseRotateSequece.Join(pillar.Base.DOBlendableLocalRotateBy(new Vector3(0, 360 * _baseSpinCycle, 0), _baseRotateDur, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
                baseRotateSequece.Append(pillar.Base.DOLocalRotate(new Vector3(0, pillar.Base.rotation.y - (0 - 360 * _baseSpinCycle % 360), 0), _baseRotationResetDur).SetEase(Ease.OutQuad));
                
                sequence.Append(baseRotateSequece);
                sequence.Join(pillar.transform.DOPunchPosition(Vector3.up * 0.2f, _baseRotateDur, 2, 0.5f));
            }

            // sequence.OnComplete(() => pillar.Base.localRotation = Quaternion.identity);

            return sequence;
        }
    }
}