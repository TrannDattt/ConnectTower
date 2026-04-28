using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Patterns.EventBus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Assets._Scripts.Visuals.BoosterButtonVisual;

namespace Assets._Scripts.Visuals
{
    public class ShuffleEffectVisual : BoosterButtonEffectVisual
    {
        [Header("Shuffle Curves")]
        [SerializeField] private float _duration;
        [SerializeField] private int _cycles;
        [SerializeField] private AnimationCurve _rotateCurve;

        [SerializeField] private float _blockMoveDelayToMain;
        [SerializeField] private float _blockMoveDelayToBlock = .05f;
        [SerializeField] private float _totalDelayMove = 1.5f;
        [SerializeField] private float _blockMoveDur = .5f;
        [SerializeField] private AnimationCurve _blockMoveToCenterCurve;
        [SerializeField] private AnimationCurve _blockMoveToPositionCurve;

        [SerializeField] private float _radius; 
        [SerializeField] private float _radiusVariation;
        [SerializeField] private float _blockMoveCircleDur = .5f;
        [SerializeField] private float _blockCycle;

        [Header("Base Oscillation")]
        [SerializeField] private float _baseMoveAmplitude = 0.2f;
        [SerializeField] private int _baseMoveCycles = 2;
        [SerializeField] private AnimationCurve _baseScaleCurve;

        public override Sequence DoBoosterAnim(BoosterRuntimeData data, Image target)
        {
            var toShuffle = (data as ShuffleBoosterRuntimeData).AvailableBlocks;
            var sequence = DOTween.Sequence();

            sequence.Append(target.transform.DORotate(new Vector3(0, 0, 360 * _cycles), _duration, RotateMode.FastBeyond360).SetEase(Ease.InOutCubic))
                    .Insert(_blockMoveDelayToMain, DoShuffleBlockAnim(toShuffle));

            return sequence;
        }

        private Sequence DoShuffleBlockAnim(List<BlockController> blocks)
        {
            var sequence = DOTween.Sequence();
            if (blocks == null || blocks.Count == 0) return sequence;

            // Calculate delay between blocks to fit within _duration
            float remainingTime = Mathf.Max(0, _duration - _blockMoveCircleDur - 2 * _blockMoveDur);
            float calculatedDelay = blocks.Count > 1 ? remainingTime / (blocks.Count - 1) : 0;
            var delayMoveTime = Mathf.Min(calculatedDelay, _blockMoveDelayToBlock);
            
            var baseRotation = blocks[0].transform.localRotation;

            float currentTime = 0f;
            foreach (var block in blocks)
            {
                if (block == null) continue;
                
                var bTransform = block.transform;
                var baseTransform = block.Base.transform;
                // var blockVisual = block.GetComponent<BlockEffectVisual>();
                // blockVisual.SetTrailEnable(false);

                var curPos = bTransform.position;
                curPos.Set(curPos.x, curPos.y, _gatherPoint.z);
                var direction = curPos - _gatherPoint;
                var dist = Vector3.Magnitude(direction);
                
                // Add variety using _radiusVariation
                float randomRadius = _radius + Random.Range(-_radiusVariation, _radiusVariation);
                var stopPoint = (dist > 0 ? direction / dist : Vector3.right) * randomRadius + _gatherPoint;
                
                var blockPos = BoardController.Instance.GetBlockPosition(block);
                var worldPos = BlockMovementController.Instance.GetBlockPosition(blockPos.Item1, blockPos.Item2);

                // 1. Move to orbital point
                sequence.Insert(currentTime, bTransform.DOMove(stopPoint, _blockMoveDur).SetEase(_blockMoveToCenterCurve));
                
                // 2. Rotate + Base Oscillation
                sequence.InsertCallback(currentTime + _blockMoveDur, () => 
                {
                    if (block == null) return;
                    
                    var startPos = bTransform.position;
                    var relX = startPos.x - _gatherPoint.x;
                    var relY = startPos.y - _gatherPoint.y;
                    var phi = Mathf.Atan2(relY, relX);
                    var w = 2 * Mathf.PI / (_blockMoveCircleDur / _blockCycle);
                    var posZ = startPos.z;

                    // Orbital rotation using DOTween for better sync/pausing
                    DOTween.To(() => 0f, t => {
                        if (block == null) return;
                        var x = _gatherPoint.x + randomRadius * Mathf.Cos(w * t + phi);
                        var y = _gatherPoint.y + randomRadius * Mathf.Sin(w * t + phi);
                        bTransform.position = new Vector3(x, y, posZ);
                    }, _blockMoveCircleDur, _blockMoveCircleDur).SetEase(Ease.Linear).SetTarget(block).SetUpdate(true);

                    // Base up-down oscillation
                    baseTransform.DOLocalMoveY(_baseMoveAmplitude, _blockMoveCircleDur / (_baseMoveCycles * 2))
                        .SetEase(Ease.InOutSine)
                        .SetLoops(_baseMoveCycles * 2, LoopType.Yoyo)
                        .SetTarget(block)
                        .SetUpdate(true);

                    baseTransform.DOScale(2f, _blockMoveCircleDur / (_baseMoveCycles * 2))
                        .SetEase(_baseScaleCurve)
                        .SetLoops(_baseMoveCycles * 2, LoopType.Yoyo)
                        .SetTarget(block)
                        .SetUpdate(true);
                });

                // 3. Move to final board position
                sequence.Insert(currentTime + _blockMoveDur + _blockMoveCircleDur, bTransform.DOMove(worldPos, _blockMoveDur).SetEase(_blockMoveToPositionCurve));
                // sequence.AppendCallback(() => blockVisual.SetTrailEnable(true));
                
                currentTime += delayMoveTime;
            }

            sequence.AppendCallback(() => 
            {
                EventBus<BlocksMovedEvent>.Publish(new BlocksMovedEvent {MovedByPlayer = false});
            });

            sequence.OnComplete(() =>
            {
                foreach (var block in blocks) block.transform.localRotation = baseRotation;
            });

            sequence.OnKill(() =>
            {
                foreach (var block in blocks) block.transform.localRotation = baseRotation;
            });

            return sequence;
        }
    }
}