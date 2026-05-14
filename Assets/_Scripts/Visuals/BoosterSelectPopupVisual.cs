using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class BoosterSelectPopupVisual : GamePopupVisual
    {
        [SerializeField] private Text _levelIndex;
        [SerializeField] private TextMeshProUGUI _levelScore;
        [SerializeField] private TextMeshProUGUI _levelScoreShadow;
        [SerializeField] private GameButtonVisual _playButton;
        [SerializeField] private BoosterSelectButton[] _boosterSelectButtons;

        private const int MAX_BOOSTER = 3;

        private LevelRuntimeData _data;
        private HashSet<EBooster> _selectedBooster = new();

        public IEnumerator ShowSelector(LevelRuntimeData data)
        {
            _data = data;
            if (_data == null)
            {
                Debug.LogError("Level is null");;
                yield break;
            }

            _levelIndex.text = $"Level {_data.Index}";

            yield return Show();
        }

        void OnEnable()
        {
            foreach(var booster in _selectedBooster)
            {
                var button = _boosterSelectButtons.FirstOrDefault(b => b.Key.Equals(booster));
                if (button != null) button.Selected();
            }
        }

        protected override void Start()
        {
            base.Start();

            _playButton.OnClicked.AddListener(() =>
            {
                StartCoroutine(Hide());
                GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                {
                    if (_data != null)
                        GameManager.Instance.StartLevel(_data, boosters: _selectedBooster.ToArray());
                });
            });

            foreach(var button in _boosterSelectButtons)
            {
                button.OnToggled.AddListener((toggleTrue) =>
                {
                    if (!button.IsEnabled)
                    {
                        button.ShowPopupText("Lock!");
                        return;
                    }

                    if (BoosterController.Instance.GetUseCount(button.Key) <= 0)
                    {
                        PopupManager.Instance.ShowBundlePopup(EPopup.Booster, BundleManager.Instance.GetIngameBoosterBundle(button.Key));
                    }

                    if (toggleTrue) 
                    {
                        if (_selectedBooster.Count == MAX_BOOSTER)
                        {
                            button.ShowPopupText("Maximun boosters");
                            button.UpdateToggle(false);
                            return;
                        }
                        _selectedBooster.Add(button.Key);
                    }
                    else _selectedBooster.Remove(button.Key);
                });
            }
        }
    }
}