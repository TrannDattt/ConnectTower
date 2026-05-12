using System.Collections;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private BoosterButtonVisual[] _boosterButtons;

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private Button _clearLevel;
        [SerializeField] private Button _failLevel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _editButton;
#endif
        // [SerializeField] private Transform _centerPoint;
        private Vector3 _centerPoint => _canvasRt.TransformPoint(_canvasRt.rect.center);

        private EventBinding<CurrencyChangedEvent> _currencyChangedBinding;

        public void InitVisual(LevelRuntimeData data)
        {
            _moveCount.UpdateMoveCount(data.MoveCount);
            _progressBar.UpdateProgress(0, data.TotalGroups);
            _difficultyTag.SetDifficulty(data.Difficulty);
            _coinDisplay.UpdateVisual();
            _levelIndex.SetLevelIndex(data.Index);

            foreach(var button in _boosterButtons)
            {
                button.ChangeLockStatus(BoosterController.Instance.GetLockStatus(button.BoosterKey));
                button.SetCount(BoosterController.Instance.GetUseCount(button.BoosterKey));
            }
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

        public Tween UpdateMoveCount(int count, float duration = 0f)
        {
            return _moveCount.UpdateMoveCount(count, duration);
        }

        public void UpdateProgressBar(int current, int target)
        {
            _progressBar.UpdateProgress(current, target);
        }

        private void OnEnable()
        {
            _currencyChangedBinding ??= new((evt) =>
            {
                for (int i = 0; i < evt.BoostersChanged.Length; i++)
                {
                    var type = evt.BoostersChanged[i].Item1;
                    var toUpdate = _boosterButtons.FirstOrDefault(b =>b.BoosterKey == type);
                    if (toUpdate != null && toUpdate.gameObject.activeInHierarchy) 
                        toUpdate.SetCount(UserManager.CurUser.BoosterCount[type]);
                }
            });
            EventBus<CurrencyChangedEvent>.Subscribe(_currencyChangedBinding);
        }

        void Start()
        {
            _settingButton.OnClicked.AddListener(() =>
            {
                StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Setting));
            });

            void UseBoosterButton(BoosterButtonVisual button)
            {
                if (!GameManager.Instance.IsPlaying || BoosterController.Instance.IsInMechanic) return;
                if (button.BoosterKey == EBooster.AddPillar && BoardController.Instance.GetPillarCount() == BoardController.MAX_PILLAR)
                {
                    button.ShowPopupText("Maximum pillar reached!");
                    return;
                }

                var type = button.BoosterKey;
                var boosterData = BoosterController.Instance.GetBoosterData(type);
                if (boosterData == null) return;
                
                var useCount = BoosterController.Instance.GetUseCount(type);
                if (button.IsLocked) button.ShowPopupText("Locked!");
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

            foreach (var button in _boosterButtons)
            {
                button.OnClicked.AddListener(() => UseBoosterButton(button));
            }
            Debug.Log($"Init with {_boosterButtons.Length} boosters");

#if UNITY_EDITOR
            if (_clearLevel != null)
                _clearLevel.onClick.AddListener(() => GameManager.Instance.ClearedLevel(false));
            if (_failLevel != null)
                _failLevel.onClick.AddListener(() => GameManager.Instance.FailedLevel(false));
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() =>
                {
                    GameManager.Instance.StartLevel(LevelManager.PlayingLevel);
                });
            if (_editButton != null)
                _editButton.onClick.AddListener(() =>
                {
                    BoardController.Instance.ClearBoard();
                    GameSceneManager.Instance.ChangeScene(EGameScene.Editor);
                });
#endif
        }

        private void OnDisable()
        {
            EventBus<CurrencyChangedEvent>.Unsubscribe(_currencyChangedBinding);
        }
    }
}