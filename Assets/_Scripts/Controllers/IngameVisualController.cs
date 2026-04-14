using System.Collections;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class IngameVisualController : Singleton<IngameVisualController>
    {
        [SerializeField] private RectTransform _canvasRt;
        [SerializeField] private MoveCountVisual _moveCount;
        [SerializeField] private ProgressBarVisual _progressBar;
        [SerializeField] private DifficultyTagVisual _difficultyTag;
        [SerializeField] private LevelIndexVisual _levelIndex;
        [SerializeField] private CoinDisplayVisual _coinDisplay;
        [SerializeField] private GameButtonVisual _settingButton;
        [SerializeField] private BoosterButtonVisual _extraMoveButton;
        [SerializeField] private BoosterButtonVisual _shuffleButton;
        [SerializeField] private BoosterButtonVisual _hintButton;
        // [SerializeField] private Transform _centerPoint;
        private Vector3 _centerPoint => _canvasRt.TransformPoint(_canvasRt.rect.center);

        public void InitVisual(LevelRuntimeData data)
        {
            _moveCount.UpdateMoveCount(data.MoveCount);
            _progressBar.UpdateProgress(0, data.TotalGroups);
            _difficultyTag.SetDifficulty(data.Difficulty);
            _coinDisplay.UpdateVisual();
            _levelIndex.SetLevelIndex(data.Index);

            //TODO: Check if booster is unlocked
            _extraMoveButton.ChangeLockStatus(BoosterController.Instance.GetLockStatus(EBooster.ExtraMove));
            _shuffleButton.ChangeLockStatus(BoosterController.Instance.GetLockStatus(EBooster.Shuffle));
            _hintButton.ChangeLockStatus(BoosterController.Instance.GetLockStatus(EBooster.Hint));

            _extraMoveButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.ExtraMove));
            _shuffleButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.Shuffle));
            _hintButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.Hint));
        }

        public void PrepareIntroducingAnim()
        {
            _levelIndex.PrepareAnim();
            _difficultyTag.PrepareAnim();
        }

        public IEnumerator DoLevelIntroducingAnim()
        {
            yield return _levelIndex.DoLevelIndexAnim(_centerPoint);
            if (LevelManager.PlayingLevel.Difficulty != EDifficulty.Normal)
                yield return _difficultyTag.DoDifficultyAnim(_centerPoint);
        }

        public Tween UpdateMoveCount(int count, bool doAnim = false)
        {
            return _moveCount.UpdateMoveCount(count, doAnim ? 1f : 0f);
        }

        public void UpdateProgressBar(int current, int target)
        {
            _progressBar.UpdateProgress(current, target);
        }

        void Start()
        {
            _settingButton.OnClicked.AddListener(() =>
            {
                StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Setting));
            });

            UserManager.OnBoosterChanged.AddListener((t, a) =>
            {
                var (toUpdate, curAmount) = t switch
                {
                    EBooster.ExtraMove => (_extraMoveButton, UserManager.CurUser.ExtraMoveCount),
                    EBooster.Shuffle => (_shuffleButton, UserManager.CurUser.ShuffleCount),
                    EBooster.Hint => (_hintButton, UserManager.CurUser.HintCount),
                    _ => (null, 0)
                };
                if (toUpdate) toUpdate.SetCount(curAmount);
            });

            void UseBoosterButton(BoosterButtonVisual button, EBooster type)
            {
                if (BoosterController.Instance.IsInMechanic) return;

                var boosterData = BoosterController.Instance.GetBoosterData(type);
                if (boosterData == null) return;
                
                var useCount = BoosterController.Instance.GetUseCount(type);
                if (button.IsLocked) button.ShowPopupText();
                else if (useCount > 0)
                {
                    BoosterController.Instance.UseBooster(type);
                    button.SetCount(BoosterController.Instance.GetUseCount(type));
                    StartCoroutine(button.DoOnUseBoosterAnim(boosterData, _centerPoint));
                }
                else 
                {
                    PopupManager.Instance.ShowBundlePopup(EPopup.Booster, BundleManager.Instance.GetIngameBoosterBundle(type));
                    Debug.Log($"{type} is out of use");
                }
            }

            _extraMoveButton.OnClicked.AddListener(() => UseBoosterButton(_extraMoveButton, EBooster.ExtraMove));
            _shuffleButton.OnClicked.AddListener(() => UseBoosterButton(_shuffleButton, EBooster.Shuffle));
            _hintButton.OnClicked.AddListener(() => UseBoosterButton(_hintButton, EBooster.Hint));
        }
    }
}