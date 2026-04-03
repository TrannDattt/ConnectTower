using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Controllers
{
    public class IngameVisualController : Singleton<IngameVisualController>
    {
        [SerializeField] private MoveCountVisual _moveCount;
        [SerializeField] private ProgressBarVisual _progressBar;
        [SerializeField] private DifficultyTagVisual _difficultyTag;
        [SerializeField] private LevelIndexVisual _levelIndex;
        [SerializeField] private CoinDisplayVisual _coinDisplay;
        [SerializeField] private ShopVisualControl _shopPanel;
        [SerializeField] private GameButtonVisual _settingButton;
        [SerializeField] private SettingPopupVisual _settingPopup;
        [SerializeField] private BoosterButtonVisual _extraMoveButton;
        [SerializeField] private BoosterButtonVisual _shuffleButton;
        [SerializeField] private BoosterButtonVisual _hintButton;
        [SerializeField] private BoosterPurchasePopupVisual _boosterPurchasePopup;
        [SerializeField] private RevivePopupVisual _revivePopup;
        [SerializeField] private LevelFinishedVisual _levelFinishedPopup;
        [SerializeField] private LevelFailedVisual _levelFailedPopup;
        [SerializeField] private Transform _centerPoint;

        public void InitVisual(LevelRuntimeData data)
        {
            _moveCount.UpdateMoveCount(data.MoveCount);
            _progressBar.UpdateProgress(0, data.TotalGroups);
            _difficultyTag.SetDifficulty(data.Difficulty);
            _levelIndex.SetLevelIndex(data.Index);

            //TODO: Check if booster is unlocked
            _extraMoveButton.ChangeLockStatus(BoosterController.Instance.GetLockStatus(EBooster.ExtraMove));
            _shuffleButton.ChangeLockStatus(BoosterController.Instance.GetLockStatus(EBooster.Shuffle));
            _hintButton.ChangeLockStatus(BoosterController.Instance.GetLockStatus(EBooster.Hint));

            _extraMoveButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.ExtraMove));
            _shuffleButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.Shuffle));
            _hintButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.Hint));

            _levelFinishedPopup.SetData(data);
            _levelFailedPopup.SetData(data);
        }

        public Tween UpdateMoveCount(int count, bool doAnim = false)
        {
            return _moveCount.UpdateMoveCount(count, doAnim ? 1f : 0f);
        }

        public void UpdateProgressBar(int current, int target)
        {
            _progressBar.UpdateProgress(current, target);
        }

        public void ShowRevivePopup()
        {
            _revivePopup.ShowBundle(BundleManager.Instance.GetReviveBundle());
        }

        public void ShowFinishedPopup(bool firstClear)
        {
            _levelFinishedPopup.Show();
        }

        public void ShowFailedPopup()
        {
            _levelFailedPopup.Show();
        }

        public void OpenShop() => _shopPanel.Show();

        void Start()
        {
            _settingButton.OnClicked.AddListener(() =>
            {
                _settingPopup.Show();
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
            _extraMoveButton.OnClicked.AddListener(() => 
            {
                var useCount = BoosterController.Instance.GetUseCount(EBooster.ExtraMove);
                if (_extraMoveButton.IsLocked) _extraMoveButton.ShowPopupText();
                else if (useCount > 0)
                {
                    _extraMoveButton.DoOnUseBoosterAnim(_centerPoint.position, () =>
                    {
                        BoosterController.Instance.UseBooster(EBooster.ExtraMove);
                        _extraMoveButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.ExtraMove));
                    });
                }
                else 
                {
                    _boosterPurchasePopup.ShowBundle(BundleManager.Instance.GetIngameBoosterBundle(EBooster.ExtraMove));
                    Debug.Log("Extra Move is out of use");
                }

            });
            _shuffleButton.OnClicked.AddListener(() =>
            {
                var useCount = BoosterController.Instance.GetUseCount(EBooster.Shuffle);
                if (_shuffleButton.IsLocked) _shuffleButton.ShowPopupText();
                else if (useCount > 0) 
                {
                    _shuffleButton.DoOnUseBoosterAnim(_centerPoint.position, () =>
                    {
                        BoosterController.Instance.UseBooster(EBooster.Shuffle);
                        _shuffleButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.Shuffle));
                    });
                }
                else 
                {
                    _boosterPurchasePopup.ShowBundle(BundleManager.Instance.GetIngameBoosterBundle(EBooster.Shuffle));
                    Debug.Log("Shuffle is out of use");
                }
            });
            _hintButton.OnClicked.AddListener(() =>
            {
                var useCount = BoosterController.Instance.GetUseCount(EBooster.Hint);
                if (_hintButton.IsLocked) _hintButton.ShowPopupText();
                else if (useCount > 0) 
                {
                    _hintButton.DoOnUseBoosterAnim(_centerPoint.position, () =>
                    {
                        BoosterController.Instance.UseBooster(EBooster.Hint);
                        _hintButton.SetCount(BoosterController.Instance.GetUseCount(EBooster.Hint));
                    });
                }
                else 
                {
                    _boosterPurchasePopup.ShowBundle(BundleManager.Instance.GetIngameBoosterBundle(EBooster.Hint));
                    Debug.Log("Hint is out of use");
                }
            });
        }
    }
}